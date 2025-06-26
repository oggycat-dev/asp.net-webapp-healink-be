using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Healink.API.Configurations;

/// <summary>
/// Configuration for Cross-Origin Resource Sharing (CORS)
/// </summary>
public static class CorsConfiguration
{
    private const string DefaultCorsPolicy = "DefaultCorsPolicy";
    private const string ProductionCorsPolicy = "ProductionCorsPolicy";
    private const string DevelopmentCorsPolicy = "DevelopmentCorsPolicy";
    
    /// <summary>
    /// Configure CORS with environment-specific policies
    /// </summary>
    public static IServiceCollection AddCorsConfiguration(this IServiceCollection services, 
        IConfiguration configuration, bool allowAnyOrigin = false)
    {
        // Extract allowed origins from configuration
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
        
        // Get current environment
        var environment = services.BuildServiceProvider().GetRequiredService<IWebHostEnvironment>();
        var isProduction = environment.IsProduction();

        services.AddCors(options =>
        {
            // Default policy (for backward compatibility)
            options.AddPolicy(DefaultCorsPolicy, policy =>
            {
                ConfigurePolicy(policy, allowedOrigins, isProduction, allowAnyOrigin);
            });

            // Production-specific policy with strict security
            options.AddPolicy(ProductionCorsPolicy, policy =>
            {
                if (allowedOrigins.Length > 0)
                {
                    policy.WithOrigins(allowedOrigins);
                }
                else
                {
                    policy.AllowAnyOrigin();
                }
                
                policy
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .WithExposedHeaders("Content-Disposition", "Token-Expired")
                    .SetIsOriginAllowedToAllowWildcardSubdomains();
            });

            // Development-specific policy with looser security
            options.AddPolicy(DevelopmentCorsPolicy, policy =>
            {
                if (allowedOrigins.Length > 0)
                {
                    policy.WithOrigins(allowedOrigins);
                }
                else
                {
                    policy.AllowAnyOrigin();
                }
                
                policy
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .WithExposedHeaders("Content-Disposition", "Token-Expired");
            });
        });
        
        return services;
    }

    // Helper method to configure a policy based on environment
    private static void ConfigurePolicy(Microsoft.AspNetCore.Cors.Infrastructure.CorsPolicyBuilder policy, 
        string[] allowedOrigins, bool isProduction, bool allowAnyOrigin)
    {
        // Configure origins
        if (allowAnyOrigin)
        {
            policy.AllowAnyOrigin();
        }
        else
        {
            if (allowedOrigins.Length == 0)
            {
                policy.AllowAnyOrigin();
            }
            else
            {
                policy.WithOrigins(allowedOrigins);
            }
        }
        
        // Configure general settings
        policy
            .AllowAnyMethod()
            .AllowAnyHeader()
            .WithExposedHeaders("Content-Disposition", "Token-Expired");
    }
    
    /// <summary>
    /// Use the environment-appropriate CORS policy
    /// </summary>
    public static IApplicationBuilder UseCorsConfiguration(this IApplicationBuilder app)
    {
        // Get current environment
        var environment = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
        
        // Use the appropriate policy based on environment
        if (environment.IsProduction())
        {
            app.UseCors(ProductionCorsPolicy);
        }
        else
        {
            app.UseCors(DevelopmentCorsPolicy);
        }
        
        return app;
    }
} 