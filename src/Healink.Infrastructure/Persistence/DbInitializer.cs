using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Healink.Domain.Entities;
using Healink.Domain.Entities.Identity;
using Healink.Domain.Enums;

namespace Healink.Infrastructure.Persistence;

public class SeedTracking
{
    public Guid Id { get; set; }
    public bool IsSeeded { get; set; }
    public DateTime SeededAt { get; set; }
}

public static class DbInitializer
{
    private static async Task<bool> ExecuteSafelyAsync(Func<Task> operation, string operationName)
    {
        try
        {
            await operation();
            Console.WriteLine($"Successfully completed: {operationName}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in {operationName}: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
            return false;
        }
    }
    
    private static async Task SeedRolesAsync(RoleManager<AppRole> roleManager)
    {
        try
        {
            Console.WriteLine("Checking for new roles to add...");
            
            var existingRoles = await roleManager.Roles.Select(r => r.Name).ToListAsync();
            
            var allRoles = new List<AppRole>
            {
                new AppRole { Name = "Admin", NormalizedName = "ADMIN", Description = "System Administrator", IsSystemRole = true },
                new AppRole { Name = "Staff", NormalizedName = "STAFF", Description = "Healthcare Staff Member", IsSystemRole = true },
                new AppRole { Name = "Doctor", NormalizedName = "DOCTOR", Description = "Medical Doctor", IsSystemRole = true },
                new AppRole { Name = "Nurse", NormalizedName = "NURSE", Description = "Registered Nurse", IsSystemRole = true },
                new AppRole { Name = "Patient", NormalizedName = "PATIENT", Description = "Patient", IsSystemRole = true },
            };

            int addedRoles = 0;
            foreach (var role in allRoles)
            {
                if (!existingRoles.Contains(role.Name))
                {
                    var result = await roleManager.CreateAsync(role);
                    if (result.Succeeded)
                    {
                        Console.WriteLine($"Added new role: {role.Name}");
                        addedRoles++;
                    }
                    else
                    {
                        Console.WriteLine($"Failed to add role {role.Name}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
            }
            
            Console.WriteLine($"Role seeding completed. Added {addedRoles} new roles.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error seeding roles: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
        }
    }
    
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            var dbContext = services.GetRequiredService<HealinkDbContext>();
            
            bool shouldSeed = await ShouldPerformSeedingAsync(dbContext);
            
            await SeedRolesAsync(services.GetRequiredService<RoleManager<AppRole>>());
            
            if (!shouldSeed)
            {
                Console.WriteLine("Database is already seeded. Skipping core seed operation.");
                return;
            }
            
            Console.WriteLine("Starting database initialization...");
            
            Console.WriteLine("Testing database connection...");
            if (!await dbContext.Database.CanConnectAsync())
            {
                Console.WriteLine("Cannot connect to database. Please check your connection string and ensure the database server is running.");
                return;
            }
            Console.WriteLine("Database connection successful.");

            #region Seed Admin Users
            try
            {
                var userManager = services.GetRequiredService<UserManager<AppUser>>();
                
                var adminEmail = "admin@healink.com";
                var adminExists = await userManager.FindByNameAsync(adminEmail) != null;

                if (!adminExists)
                {
                    Console.WriteLine("Creating Admin user...");
                    var adminUser = new AppUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        NormalizedEmail = adminEmail.ToUpper(),
                        NormalizedUserName = adminEmail.ToUpper(),
                        EmailConfirmed = true,
                        PhoneNumber = "0901234567",
                        PhoneNumberConfirmed = true,
                        EntityId = Guid.NewGuid()
                    };

                    var result = await userManager.CreateAsync(adminUser, "Admin@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                        
                        // Create admin staff profile
                        var adminProfile = new StaffProfile
                        {
                            Id = adminUser.EntityId.Value,
                            IdentityUserId = adminUser.Id,
                            FirstName = "Admin",
                            LastName = "Healink",
                            FullName = "Admin Healink",
                            Email = adminEmail,
                            PhoneNumber = "0901234567",
                            Gender = Gender.Other,
                            JoinDate = DateTime.UtcNow,
                            Status = EntityStatus.Active,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = adminUser.EntityId.Value
                        };
                        
                        await dbContext.StaffProfiles.AddAsync(adminProfile);
                        await dbContext.SaveChangesAsync();
                        
                        Console.WriteLine("Admin user and profile created successfully");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to create Admin: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }

                // Create sample doctor
                var doctorEmail = "doctor@healink.com";
                var doctorExists = await userManager.FindByNameAsync(doctorEmail) != null;

                if (!doctorExists)
                {
                    Console.WriteLine("Creating Doctor user...");
                    var doctorUser = new AppUser
                    {
                        UserName = doctorEmail,
                        Email = doctorEmail,
                        NormalizedEmail = doctorEmail.ToUpper(),
                        NormalizedUserName = doctorEmail.ToUpper(),
                        EmailConfirmed = true,
                        PhoneNumber = "0901234568",
                        PhoneNumberConfirmed = true,
                        EntityId = Guid.NewGuid()
                    };

                    var result = await userManager.CreateAsync(doctorUser, "Doctor@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(doctorUser, "Doctor");
                        
                        // Create doctor staff profile
                        var doctorProfile = new StaffProfile
                        {
                            Id = doctorUser.EntityId.Value,
                            IdentityUserId = doctorUser.Id,
                            FirstName = "John",
                            LastName = "Smith",
                            FullName = "Dr. John Smith",
                            Email = doctorEmail,
                            PhoneNumber = "0901234568",
                            Gender = Gender.Male,
                            JoinDate = DateTime.UtcNow,
                            Status = EntityStatus.Active,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = doctorUser.EntityId.Value
                        };
                        
                        await dbContext.StaffProfiles.AddAsync(doctorProfile);
                        await dbContext.SaveChangesAsync();
                        
                        Console.WriteLine("Doctor user and profile created successfully");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating admin users: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
            }
            #endregion
            
            await MarkDatabaseAsSeededAsync(dbContext);
            
            Console.WriteLine("Database initialization completed successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Critical error in database initialization: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
            throw;
        }
    }
    
    private static async Task<bool> ShouldPerformSeedingAsync(HealinkDbContext dbContext)
    {
        try
        {
            // Check if we have any seed tracking record
            var seedTrackingExists = await dbContext.Database
                .SqlQueryRaw<int>("SELECT COUNT(*) FROM [__SeedTracking] WHERE IsSeeded = 1")
                .FirstOrDefaultAsync();
                
            if (seedTrackingExists > 0)
            {
                Console.WriteLine("Found existing seed tracking record. Skipping seeding.");
                return false;
            }
            
            // Check if we have any existing data (like admin users)
            var hasStaffProfiles = await dbContext.StaffProfiles.AnyAsync();
            if (hasStaffProfiles)
            {
                Console.WriteLine("Found existing staff profiles. Skipping seeding.");
                await MarkDatabaseAsSeededAsync(dbContext);
                return false;
            }
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking seeding status: {ex.Message}");
            // If we can't check, assume we should seed
            return true;
        }
    }
    
    private static async Task MarkDatabaseAsSeededAsync(HealinkDbContext dbContext)
    {
        try
        {
            // Create table if it doesn't exist
            await dbContext.Database.ExecuteSqlRawAsync(@"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='__SeedTracking' AND xtype='U')
                CREATE TABLE [__SeedTracking] (
                    [Id] uniqueidentifier NOT NULL PRIMARY KEY,
                    [IsSeeded] bit NOT NULL,
                    [SeededAt] datetime2 NOT NULL
                )");
            
            // Insert or update seed tracking
            var trackingId = Guid.NewGuid();
            await dbContext.Database.ExecuteSqlRawAsync(@"
                IF NOT EXISTS (SELECT * FROM [__SeedTracking] WHERE IsSeeded = 1)
                INSERT INTO [__SeedTracking] ([Id], [IsSeeded], [SeededAt]) 
                VALUES ({0}, 1, {1})", trackingId, DateTime.UtcNow);
                
            Console.WriteLine("Marked database as seeded.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error marking database as seeded: {ex.Message}");
        }
    }
} 