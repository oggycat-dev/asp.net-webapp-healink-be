using DotNetEnv;

namespace Healink.API.Configurations;

public static class EnvironmentConfiguration
{
    public static WebApplicationBuilder AddEnvironmentConfiguration(this WebApplicationBuilder builder)
    {
        try
        {
            // Load .env file
            var envFilePath = Path.Combine(builder.Environment.ContentRootPath, ".env");
            if (File.Exists(envFilePath))
            {
                Env.Load(envFilePath);
                Console.WriteLine("✓ .env file loaded successfully");
            }
            else
            {
                Console.WriteLine("⚠ .env file not found, using system environment variables");
            }

            // Database Connection
            var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
            if (!string.IsNullOrEmpty(connectionString))
            {
                builder.Configuration["ConnectionStrings:DefaultConnection"] = connectionString;
            }

            // JWT Settings
            var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET");
            if (!string.IsNullOrEmpty(jwtSecret))
            {
                builder.Configuration["JwtSettings:Secret"] = jwtSecret;
            }

            var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
            if (!string.IsNullOrEmpty(jwtIssuer))
            {
                builder.Configuration["JwtSettings:Issuer"] = jwtIssuer;
            }

            var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
            if (!string.IsNullOrEmpty(jwtAudience))
            {
                builder.Configuration["JwtSettings:Audience"] = jwtAudience;
            }

            var jwtExpiryHours = Environment.GetEnvironmentVariable("JWT_EXPIRY_HOURS");
            if (!string.IsNullOrEmpty(jwtExpiryHours))
            {
                builder.Configuration["JwtSettings:ExpiryHours"] = jwtExpiryHours;
            }

            var jwtRefreshExpiryDays = Environment.GetEnvironmentVariable("JWT_REFRESH_EXPIRY_DAYS");
            if (!string.IsNullOrEmpty(jwtRefreshExpiryDays))
            {
                builder.Configuration["JwtSettings:RefreshExpiryDays"] = jwtRefreshExpiryDays;
            }

            // Admin Account Settings
            var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
            if (!string.IsNullOrEmpty(adminEmail))
            {
                builder.Configuration["AdminAccount:Email"] = adminEmail;
            }

            var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");
            if (!string.IsNullOrEmpty(adminPassword))
            {
                builder.Configuration["AdminAccount:Password"] = adminPassword;
            }

            var adminFirstName = Environment.GetEnvironmentVariable("ADMIN_FIRSTNAME");
            if (!string.IsNullOrEmpty(adminFirstName))
            {
                builder.Configuration["AdminAccount:FirstName"] = adminFirstName;
            }

            var adminLastName = Environment.GetEnvironmentVariable("ADMIN_LASTNAME");
            if (!string.IsNullOrEmpty(adminLastName))
            {
                builder.Configuration["AdminAccount:LastName"] = adminLastName;
            }

            // CORS Settings
            var corsOrigins = Environment.GetEnvironmentVariable("CORS_ORIGINS");
            if (!string.IsNullOrEmpty(corsOrigins))
            {
                builder.Configuration["CorsSettings:Origins"] = corsOrigins;
            }

            // Security Settings
            var requireHttps = Environment.GetEnvironmentVariable("SECURITY_REQUIRE_HTTPS");
            if (!string.IsNullOrEmpty(requireHttps))
            {
                builder.Configuration["Security:RequireHttps"] = requireHttps;
            }

            // Storage Settings
            var storageType = Environment.GetEnvironmentVariable("STORAGE_TYPE");
            if (!string.IsNullOrEmpty(storageType))
            {
                builder.Configuration["StorageSettings:Type"] = storageType;
            }

            var storagePath = Environment.GetEnvironmentVariable("STORAGE_PATH");
            if (!string.IsNullOrEmpty(storagePath))
            {
                builder.Configuration["StorageSettings:Path"] = storagePath;
            }

            var storageMaxFileSize = Environment.GetEnvironmentVariable("STORAGE_MAX_FILE_SIZE");
            if (!string.IsNullOrEmpty(storageMaxFileSize))
            {
                builder.Configuration["StorageSettings:MaxFileSize"] = storageMaxFileSize;
            }

            // Logging Settings
            var logLevel = Environment.GetEnvironmentVariable("LOG_LEVEL");
            if (!string.IsNullOrEmpty(logLevel))
            {
                builder.Configuration["Logging:LogLevel:Default"] = logLevel;
            }

            var logFilePath = Environment.GetEnvironmentVariable("LOG_FILE_PATH");
            if (!string.IsNullOrEmpty(logFilePath))
            {
                builder.Configuration["Logging:File:Path"] = logFilePath;
            }

            Console.WriteLine("Environment configuration loaded successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Error loading environment configuration: {ex.Message}");
            // Continue without environment configuration
        }

        return builder;
    }
} 