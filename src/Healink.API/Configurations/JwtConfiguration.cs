using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Healink.API.Configurations;

/// <summary>
/// Extension methods for JWT configuration
/// </summary>
public static class JwtConfiguration
{
    /// <summary>
    /// Configure JWT authentication
    /// </summary>
    public static IServiceCollection AddJwtConfiguration(this IServiceCollection services, 
        IConfiguration configuration, bool requireHttps = false)
    {
        // Get JWT settings
        var jwtSecret = configuration["Jwt:SecretKey"];
        var jwtIssuer = configuration["Jwt:Issuer"];
        var jwtAudience = configuration["Jwt:Audience"];

        if (string.IsNullOrEmpty(jwtSecret))
            throw new ArgumentNullException(nameof(jwtSecret), "JWT Secret is not configured");

        if (string.IsNullOrEmpty(jwtIssuer))
            throw new ArgumentNullException(nameof(jwtIssuer), "JWT Issuer is not configured");

        if (string.IsNullOrEmpty(jwtAudience))
            throw new ArgumentNullException(nameof(jwtAudience), "JWT Audience is not configured");

        // Add JWT authentication
        var key = Encoding.UTF8.GetBytes(jwtSecret);
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = requireHttps;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
            
            // Add event handlers for JWT bearer events
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                    {
                        context.Response.Headers.Add("Token-Expired", "true");
                    }
                    return Task.CompletedTask;
                }
            };
        });
        
        return services;
    }
} 