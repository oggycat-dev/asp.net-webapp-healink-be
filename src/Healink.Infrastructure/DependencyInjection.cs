using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Healink.Application.Common.Interfaces;
using Healink.Application.Common.Interfaces.Repositories;
using Healink.Application.Common.Interfaces.Services;
using Healink.Domain.Entities.Identity;
using Healink.Infrastructure.Persistence;
using Healink.Infrastructure.Services;
using Healink.Infrastructure.Models;
using Healink.Infrastructure.Repositories;
using Healink.Infrastructure.Configurations;
using Healink.Application.Common.Models;
using Amazon.S3;
using Amazon;

namespace Healink.Infrastructure;

/// <summary>
/// Dependency injection configuration for the Infrastructure layer
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Add infrastructure services to the dependency container
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Load environment variables
        EnvironmentConfiguration.LoadEnvironmentVariables();

        // Get connection string from environment or configuration
        var connectionString = EnvironmentConfiguration.GetConnectionString();
        if (string.IsNullOrEmpty(connectionString))
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string not found. Please set the CONNECTION_STRING environment variable or configure it in appsettings.json");
        }
        
        // Configure Identity database context
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(connectionString, builder =>
            {
                builder.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            });
        });

        // Configure Business database context
        services.AddDbContext<HealinkDbContext>(options =>
        {
            options.UseSqlServer(connectionString, builder =>
            {
                builder.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            });

            // Enable soft delete global filter
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });
                
        // Register context interfaces
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IHealinkDbContext>(provider => provider.GetRequiredService<HealinkDbContext>());
            
        // Configure Identity
        services.AddIdentity<AppUser, AppRole>(options =>
        {
            // Password settings
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
            
            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            
            // User settings
            options.User.RequireUniqueEmail = true;
            
            // SignIn settings
            options.SignIn.RequireConfirmedEmail = false;
            options.SignIn.RequireConfirmedPhoneNumber = false;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();
        
        // Configure JWT from environment or configuration
        var jwtConfig = EnvironmentConfiguration.GetJwtConfiguration();
        
        // Configure JWT settings for Application layer
        services.Configure<Application.Common.Models.JwtSettings>(options =>
        {
            options.SecretKey = jwtConfig.SecretKey;
            options.Issuer = jwtConfig.Issuer;
            options.Audience = jwtConfig.Audience;
            options.ExpiresInMinutes = jwtConfig.ExpiresInMinutes;
            options.RefreshTokenExpiresInDays = jwtConfig.RefreshTokenExpiresInDays;
        });

        // Try to get from configuration if environment is empty
        if (string.IsNullOrEmpty(jwtConfig.SecretKey))
        {
            var jwtSection = configuration.GetSection("Jwt");
            jwtConfig.SecretKey = jwtSection["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not found");
            jwtConfig.Issuer = jwtSection["Issuer"] ?? "Healink";
            jwtConfig.Audience = jwtSection["Audience"] ?? "HealinkUsers";
            jwtConfig.ExpiresInMinutes = int.TryParse(jwtSection["ExpiresInMinutes"], out var expiry) ? expiry : 60;
        }

        // Configure Storage Settings
        var storageSettings = EnvironmentConfiguration.GetStorageConfiguration();
        services.Configure<StorageSettings>(options =>
        {
            options.ProviderType = storageSettings.ProviderType;
            options.BaseUrl = storageSettings.BaseUrl;
            options.LocalStorage = storageSettings.LocalStorage;
            options.AmazonS3 = storageSettings.AmazonS3;
            options.AzureBlob = storageSettings.AzureBlob;
        });

        // Register repositories
        services.AddScoped<IStaffProfileRepository, StaffProfileRepository>();
        services.AddScoped<IPodcastRepository, PodcastRepository>();
        services.AddScoped<IPodcastCategoryRepository, PodcastCategoryRepository>();
        services.AddScoped<IUserProfileRepository, UserProfileRepository>();
        services.AddScoped<IPodcastPlayHistoryRepository, PodcastPlayHistoryRepository>();

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register services
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPodcastService, PodcastService>();
        services.AddScoped<IPodcastCategoryService, PodcastCategoryService>();
        services.AddScoped<IFileStorageService, FileStorageService>();
        services.AddScoped<IMoodRecommendationService, MoodRecommendationService>();

        // Register AWS S3
        services.AddSingleton<IAmazonS3>(provider =>
        {
            var accessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
            var secretKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");
            var region = Environment.GetEnvironmentVariable("AWS_REGION") ?? "us-east-1";

            if (!string.IsNullOrEmpty(accessKey) && !string.IsNullOrEmpty(secretKey))
            {
                return new AmazonS3Client(accessKey, secretKey, RegionEndpoint.GetBySystemName(region));
            }
            
            // Use default AWS credentials chain (IAM roles, environment variables, etc.)
            return new AmazonS3Client(RegionEndpoint.GetBySystemName(region));
        });

        // Register AutoMapper
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);

        return services;
    }
} 