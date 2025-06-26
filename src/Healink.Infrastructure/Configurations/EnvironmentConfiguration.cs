using DotNetEnv;
using Healink.Application.Common.Models;
using Healink.Infrastructure.Models;

namespace Healink.Infrastructure.Configurations;

/// <summary>
/// Environment configuration helper
/// </summary>
public static class EnvironmentConfiguration
{
    /// <summary>
    /// Load environment variables from .env file
    /// </summary>
    public static void LoadEnvironmentVariables()
    {
        try
        {
            // Try to load from API project first
            var apiEnvPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
            if (File.Exists(apiEnvPath))
            {
                Env.Load(apiEnvPath);
            }
            else
            {
                // Fallback to root directory
                Env.Load();
            }
        }
        catch
        {
            // .env file not found or cannot be loaded, continue with system environment variables
        }
    }

    /// <summary>
    /// Get environment variable with fallback
    /// </summary>
    public static string GetEnvironmentVariable(string key, string defaultValue = "")
    {
        return Environment.GetEnvironmentVariable(key) ?? defaultValue;
    }

    /// <summary>
    /// Get database connection string
    /// </summary>
    public static string GetConnectionString()
    {
        return Environment.GetEnvironmentVariable("CONNECTION_STRING") ?? string.Empty;
    }

    /// <summary>
    /// Get JWT configuration
    /// </summary>
    public static Application.Common.Models.JwtSettings GetJwtConfiguration()
    {
        return new Application.Common.Models.JwtSettings
        {
            SecretKey = Environment.GetEnvironmentVariable("JWT_SECRET") ?? 
                       Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? string.Empty,
            Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "healink",
            Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "web",
            ExpiresInMinutes = int.TryParse(Environment.GetEnvironmentVariable("JWT_EXPIRES_IN_MINUTES"), out var expiry) ? expiry : 60
        };
    }

    /// <summary>
    /// Get refresh token configuration
    /// </summary>
    public static int GetRefreshTokenExpiryDays()
    {
        return int.TryParse(Environment.GetEnvironmentVariable("REFRESH_TOKEN_EXPIRES_IN_DAYS"), out var days) ? days : 7;
    }

    /// <summary>
    /// Get admin account configuration
    /// </summary>
    public static AdminAccountSettings GetAdminConfiguration()
    {
        return new AdminAccountSettings
        {
            Email = Environment.GetEnvironmentVariable("ADMIN_EMAIL") ?? "admin@healink.com",
            Password = Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? "Admin123!",
            Username = Environment.GetEnvironmentVariable("ADMIN_USERNAME") ?? "admin"
        };
    }

    /// <summary>
    /// Get storage configuration
    /// </summary>
    public static StorageSettings GetStorageConfiguration()
    {
        var settings = new StorageSettings
        {
            ProviderType = Environment.GetEnvironmentVariable("STORAGE_PROVIDER_TYPE") ?? "LocalStorage",
            BaseUrl = Environment.GetEnvironmentVariable("STORAGE_BASE_URL") ?? string.Empty
        };

        // Local Storage Settings
        settings.LocalStorage = new LocalStorageSettings
        {
            RootPath = Environment.GetEnvironmentVariable("LOCAL_STORAGE_ROOT_PATH") ?? "wwwroot/uploads",
            MaxFileSize = long.TryParse(Environment.GetEnvironmentVariable("LOCAL_STORAGE_MAX_FILE_SIZE"), out var localMaxSize) ? localMaxSize : 10 * 1024 * 1024,
            AllowedExtensions = ParseExtensions(Environment.GetEnvironmentVariable("LOCAL_STORAGE_ALLOWED_EXTENSIONS"))
        };

        // Amazon S3 Settings
        settings.AmazonS3 = new AmazonS3Settings
        {
            AccessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID") ?? string.Empty,
            SecretKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY") ?? string.Empty,
            BucketName = Environment.GetEnvironmentVariable("AWS_S3_BUCKET_NAME") ?? string.Empty,
            Region = Environment.GetEnvironmentVariable("AWS_S3_REGION") ?? "ap-southeast-2",
            UseHttps = bool.TryParse(Environment.GetEnvironmentVariable("AWS_S3_USE_HTTPS"), out var useHttps) ? useHttps : true,
            MaxFileSize = long.TryParse(Environment.GetEnvironmentVariable("AWS_S3_MAX_FILE_SIZE"), out var awsMaxSize) ? awsMaxSize : 50 * 1024 * 1024,
            AllowedExtensions = ParseExtensions(Environment.GetEnvironmentVariable("AWS_ALLOWED_EXTENSIONS"))
        };

        // Azure Blob Settings
        settings.AzureBlob = new AzureBlobSettings
        {
            ConnectionString = Environment.GetEnvironmentVariable("AZURE_CONNECTION_STRING") ?? string.Empty,
            ContainerName = Environment.GetEnvironmentVariable("AZURE_CONTAINER_NAME") ?? string.Empty,
            MaxFileSize = long.TryParse(Environment.GetEnvironmentVariable("AZURE_MAX_FILE_SIZE"), out var azureMaxSize) ? azureMaxSize : 50 * 1024 * 1024,
            AllowedExtensions = ParseExtensions(Environment.GetEnvironmentVariable("AZURE_ALLOWED_EXTENSIONS"))
        };

        return settings;
    }

    /// <summary>
    /// Get CORS configuration
    /// </summary>
    public static CorsSettings GetCorsConfiguration()
    {
        var allowAnyOrigin = bool.TryParse(Environment.GetEnvironmentVariable("CORS_ALLOW_ANY_ORIGIN"), out var allow) && allow;
        
        return new CorsSettings
        {
            FrontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL") ?? "http://localhost:3000",
            AllowAnyOrigin = allowAnyOrigin,
            AllowedOrigins = GetCorsAllowedOrigins()
        };
    }

    /// <summary>
    /// Get CORS allowed origins
    /// </summary>
    public static string[] GetCorsAllowedOrigins()
    {
        var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL");
        var corsOrigins = Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS");
        
        var origins = new List<string>();
        
        if (!string.IsNullOrEmpty(frontendUrl))
        {
            origins.Add(frontendUrl);
        }
        
        if (!string.IsNullOrEmpty(corsOrigins))
        {
            origins.AddRange(corsOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                      .Select(origin => origin.Trim()));
        }
        
        if (origins.Count == 0)
        {
            return new[] { "http://localhost:3000", "http://localhost:4200", "https://localhost:3000", "https://localhost:4200" };
        }

        return origins.ToArray();
    }

    /// <summary>
    /// Get environment mode
    /// </summary>
    public static string GetEnvironmentMode()
    {
        return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
    }

    /// <summary>
    /// Get security settings
    /// </summary>
    public static SecuritySettings GetSecuritySettings()
    {
        return new SecuritySettings
        {
            RequireHttps = bool.TryParse(Environment.GetEnvironmentVariable("REQUIRE_HTTPS"), out var requireHttps) && requireHttps
        };
    }

    /// <summary>
    /// Get system settings
    /// </summary>
    public static SystemSettings GetSystemSettings()
    {
        return new SystemSettings
        {
            Name = Environment.GetEnvironmentVariable("SYSTEM_NAME") ?? "Healink"
        };
    }

    /// <summary>
    /// Get allowed hosts
    /// </summary>
    public static string GetAllowedHosts()
    {
        return Environment.GetEnvironmentVariable("ALLOWED_HOSTS") ?? "*";
    }

    /// <summary>
    /// Get log level configuration
    /// </summary>
    public static LogLevelSettings GetLogLevelConfiguration()
    {
        return new LogLevelSettings
        {
            Default = Environment.GetEnvironmentVariable("LOG_LEVEL_DEFAULT") ?? "Information",
            AspNetCore = Environment.GetEnvironmentVariable("LOG_LEVEL_ASPNETCORE") ?? "Warning"
        };
    }

    /// <summary>
    /// Parse file extensions from comma-separated string
    /// </summary>
    private static string[] ParseExtensions(string? extensionsString)
    {
        if (string.IsNullOrEmpty(extensionsString))
        {
            return new[] { ".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx", ".epub" };
        }

        return extensionsString.Split(',', StringSplitOptions.RemoveEmptyEntries)
                              .Select(ext => ext.Trim())
                              .ToArray();
    }
}

/// <summary>
/// Admin account settings
/// </summary>
public class AdminAccountSettings
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
}

/// <summary>
/// CORS settings
/// </summary>
public class CorsSettings
{
    public string FrontendUrl { get; set; } = string.Empty;
    public bool AllowAnyOrigin { get; set; } = false;
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
}

/// <summary>
/// Security settings
/// </summary>
public class SecuritySettings
{
    public bool RequireHttps { get; set; } = false;
}

/// <summary>
/// System settings
/// </summary>
public class SystemSettings
{
    public string Name { get; set; } = "Healink";
}

/// <summary>
/// Log level settings
/// </summary>
public class LogLevelSettings
{
    public string Default { get; set; } = "Information";
    public string AspNetCore { get; set; } = "Warning";
} 