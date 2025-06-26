using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Healink.API.Middlewares;
using Healink.Application.Common.Interfaces;
using Healink.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using System.Reflection;

namespace Healink.API.Injection;

/// <summary>
/// Dependency injection configuration for API layer
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Add API layer services
    /// </summary>
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        // Register MediatR for API layer if not already registered
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        
        // Register controllers
        services.AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                // Customize API behavior here if needed
                options.SuppressModelStateInvalidFilter = false;
            });
        
        // Register API documentation
        services.AddEndpointsApiExplorer();
        
        return services;
    }

    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add HttpContextAccessor
        services.AddHttpContextAccessor();
        
        // Register role-based access filters
        services.AddScoped<UserRoleAccessFilter>();
        services.AddScoped<StaffRoleAccessFilter>();
        services.AddScoped<AdminRoleAccessFilter>();

        // Register validation configuration
        services.AddValidationConfiguration();

        return services;
    }

    public static IApplicationBuilder UseApiConfiguration(this IApplicationBuilder app)
    {
        // Use global exception handling
        app.UseGlobalExceptionHandling();

        // Use JWT middleware
        app.UseJwtMiddleware();

        return app;
    }
} 