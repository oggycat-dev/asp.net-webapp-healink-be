using Microsoft.EntityFrameworkCore;
using Healink.Infrastructure.Persistence;
using Healink.Infrastructure.Data;

namespace Healink.API.Extensions;

public static class MigrationExtensions
{
    public static async Task ApplyMigrations(this IApplicationBuilder app, ILogger logger)
    {
        try
        {
            using var scope = app.ApplicationServices.CreateScope();

            // Get both DbContexts
            using var identityDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            using var businessDbContext = scope.ServiceProvider.GetRequiredService<HealinkDbContext>();

            logger.LogInformation("Starting database migrations...");
            
            // Check database connections with retry logic
            try
            {
                await RetryDatabaseConnection(identityDbContext, "Identity", logger);
                await RetryDatabaseConnection(businessDbContext, "Business", logger);
                logger.LogInformation("Successfully connected to all databases.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to connect to one or more databases after multiple attempts!");
                throw;
            }

            // Apply pending migrations for Identity
            try
            {
                var pendingIdentityMigrations = identityDbContext.Database.GetPendingMigrations();
                var appliedIdentityMigrations = identityDbContext.Database.GetAppliedMigrations();

                logger.LogInformation(
                    "Identity DB: Found {PendingCount} pending migrations and {AppliedCount} previously applied migrations",
                    pendingIdentityMigrations.Count(),
                    appliedIdentityMigrations.Count());

                if (pendingIdentityMigrations.Any())
                {
                    logger.LogInformation("Applying pending Identity migrations...");
                    identityDbContext.Database.Migrate();
                    logger.LogInformation("Successfully applied all pending Identity migrations.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while applying Identity migrations!");
                throw;
            }

            // Apply pending migrations for Business
            try
            {
                var pendingBusinessMigrations = businessDbContext.Database.GetPendingMigrations();
                var appliedBusinessMigrations = businessDbContext.Database.GetAppliedMigrations();

                logger.LogInformation(
                    "Business DB: Found {PendingCount} pending migrations and {AppliedCount} previously applied migrations",
                    pendingBusinessMigrations.Count(),
                    appliedBusinessMigrations.Count());

                if (pendingBusinessMigrations.Any())
                {
                    logger.LogInformation("Applying pending Business migrations...");
                    businessDbContext.Database.Migrate();
                    logger.LogInformation("Successfully applied all pending Business migrations.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while applying Business migrations!");
                throw;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "A problem occurred during database migrations!");
            throw;
        }
    }

    private static async Task RetryDatabaseConnection(DbContext context, string contextName, ILogger logger, int maxRetries = 3, int delaySeconds = 5)
    {
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                logger.LogInformation("Attempting to connect to {ContextName} database (attempt {Attempt}/{MaxRetries})...", contextName, attempt, maxRetries);
                
                // Test connection
                await context.Database.CanConnectAsync();
                logger.LogInformation("Successfully connected to {ContextName} database on attempt {Attempt}", contextName, attempt);
                return;
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                logger.LogWarning(ex, "Failed to connect to {ContextName} database on attempt {Attempt}. Retrying in {DelaySeconds} seconds...", contextName, attempt, delaySeconds);
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to connect to {ContextName} database after {MaxRetries} attempts", contextName, maxRetries);
                throw;
            }
        }
    }

    public static void EnsureDatabaseCreated(this IApplicationBuilder app, ILogger logger)
    {
        try
        {
            using var scope = app.ApplicationServices.CreateScope();
            using var identityDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            using var businessDbContext = scope.ServiceProvider.GetRequiredService<HealinkDbContext>();

            logger.LogInformation("Checking if databases exist...");
            
            if (identityDbContext.Database.EnsureCreated())
            {
                logger.LogInformation("Identity database was created successfully.");
            }
            else
            {
                logger.LogInformation("Identity database already exists.");
            }

            if (businessDbContext.Database.EnsureCreated())
            {
                logger.LogInformation("Business database was created successfully.");
            }
            else
            {
                logger.LogInformation("Business database already exists.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while ensuring databases exist!");
            throw;
        }
    }

    public static async Task SeedDataAsync(this IApplicationBuilder app, ILogger logger)
    {
        try
        {
            using var scope = app.ApplicationServices.CreateScope();
            var services = scope.ServiceProvider;

            logger.LogInformation("Starting data seeding...");

            await DataSeeder.SeedAsync(services);

            logger.LogInformation("Data seeding completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during data seeding!");
            throw;
        }
    }
} 