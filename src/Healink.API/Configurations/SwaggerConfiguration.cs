using Microsoft.OpenApi.Models;

namespace Healink.API.Configurations;

/// <summary>
/// Configuration for Swagger UI
/// </summary>
public static class SwaggerConfiguration
{
    /// <summary>
    /// Configure Swagger generation options
    /// </summary>
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            // Enable annotations support
            options.EnableAnnotations();

            // Configure OpenAPI specification  
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "1.0.0",
                Title = "Healink API",
                Description = "A comprehensive healthcare management API built with ASP.NET Core",
                Contact = new OpenApiContact
                {
                    Name = "Healink Support",
                    Email = "support@healink.com"
                },
                License = new OpenApiLicense
                {
                    Name = "MIT",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                }
            });

            // Configure JWT authentication
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                }
            });

            // Configure operation IDs
            options.CustomOperationIds(apiDesc =>
                $"{apiDesc.ActionDescriptor.RouteValues["controller"]}_{apiDesc.ActionDescriptor.RouteValues["action"]}");
        });

        return services;
    }

    /// <summary>
    /// Configure Swagger middleware
    /// </summary>
    public static IApplicationBuilder UseSwaggerConfiguration(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Healink API v1");
            options.RoutePrefix = "swagger";
            options.DisplayRequestDuration();
            options.EnableTryItOutByDefault();
            options.EnableDeepLinking();
            options.ShowExtensions();
            options.EnableValidator();
            options.DocumentTitle = "Healink API Documentation";
        });

        return app;
    }
} 