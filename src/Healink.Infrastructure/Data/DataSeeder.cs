using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Healink.Domain.Entities;
using Healink.Domain.Entities.Identity;
using Healink.Domain.Enums;
using Healink.Infrastructure.Persistence;

namespace Healink.Infrastructure.Data;

/// <summary>
/// Data seeder for initial data
/// </summary>
public static class DataSeeder
{
    /// <summary>
    /// Seed initial data
    /// </summary>
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var identityContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var businessContext = scope.ServiceProvider.GetRequiredService<HealinkDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        try
        {
            Console.WriteLine("Starting data seeding...");

            // Seed roles first
            await SeedRolesAsync(roleManager);

            // Check if we should perform seeding
            bool shouldSeed = await ShouldPerformSeedingAsync(businessContext);
            
            if (!shouldSeed)
            {
                Console.WriteLine("Database is already seeded. Skipping core seed operation.");
                return;
            }

            // Seed admin user and profile
            await SeedAdminUserAsync(userManager, businessContext);

            // Seed sample doctor
            await SeedSampleDoctorAsync(userManager, businessContext);

            await MarkDatabaseAsSeededAsync(businessContext);
            
            Console.WriteLine("Data seeding completed successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during data seeding: {ex.Message}");
            throw;
        }
    }

    private static async Task<bool> ShouldPerformSeedingAsync(HealinkDbContext dbContext)
    {
        try
        {
            // Check if we have any existing staff profiles
            var hasStaffProfiles = await dbContext.StaffProfiles.AnyAsync();
            if (hasStaffProfiles)
            {
                Console.WriteLine("Found existing staff profiles. Skipping seeding.");
                return false;
            }
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking seeding status: {ex.Message}");
            return true;
        }
    }

    private static async Task SeedRolesAsync(RoleManager<AppRole> roleManager)
    {
        Console.WriteLine("Seeding roles...");
        
        var roles = new[]
        {
            new AppRole 
            { 
                Name = "Admin", 
                NormalizedName = "ADMIN",
                Description = "System Administrator",
                IsSystemRole = true
            },
            new AppRole 
            { 
                Name = "Staff", 
                NormalizedName = "STAFF",
                Description = "Healthcare Staff Member",
                IsSystemRole = true
            },
            new AppRole 
            { 
                Name = "Doctor", 
                NormalizedName = "DOCTOR",
                Description = "Medical Doctor",
                IsSystemRole = true
            },
            new AppRole 
            { 
                Name = "Nurse", 
                NormalizedName = "NURSE",
                Description = "Registered Nurse",
                IsSystemRole = true
            },
            new AppRole 
            { 
                Name = "Patient", 
                NormalizedName = "PATIENT",
                Description = "Patient",
                IsSystemRole = true
            }
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role.Name!))
            {
                var result = await roleManager.CreateAsync(role);
                if (result.Succeeded)
                {
                    Console.WriteLine($"Created role: {role.Name}");
                }
                else
                {
                    Console.WriteLine($"Failed to create role {role.Name}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }
    }

    private static async Task SeedAdminUserAsync(UserManager<AppUser> userManager, HealinkDbContext businessContext)
    {
        Console.WriteLine("Seeding admin user...");
        
        var adminEmail = "admin@healink.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            var entityId = Guid.NewGuid();
            adminUser = new AppUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                NormalizedEmail = adminEmail.ToUpper(),
                NormalizedUserName = adminEmail.ToUpper(),
                EmailConfirmed = true,
                PhoneNumber = "0901234567",
                PhoneNumberConfirmed = true,
                IsActive = true,
                EntityId = entityId
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                
                // Create admin staff profile
                var adminProfile = new StaffProfile
                {
                    Id = entityId,
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
                    CreatedBy = entityId
                };
                
                await businessContext.StaffProfiles.AddAsync(adminProfile);
                await businessContext.SaveChangesAsync();
                
                Console.WriteLine("Admin user and profile created successfully");
            }
            else
            {
                Console.WriteLine($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
    }

    private static async Task SeedSampleDoctorAsync(UserManager<AppUser> userManager, HealinkDbContext businessContext)
    {
        Console.WriteLine("Seeding sample doctor...");
        
        var doctorEmail = "doctor@healink.com";
        var doctorUser = await userManager.FindByEmailAsync(doctorEmail);

        if (doctorUser == null)
        {
            var entityId = Guid.NewGuid();
            doctorUser = new AppUser
            {
                UserName = doctorEmail,
                Email = doctorEmail,
                NormalizedEmail = doctorEmail.ToUpper(),
                NormalizedUserName = doctorEmail.ToUpper(),
                EmailConfirmed = true,
                PhoneNumber = "0901234568",
                PhoneNumberConfirmed = true,
                IsActive = true,
                EntityId = entityId
            };

            var result = await userManager.CreateAsync(doctorUser, "Doctor123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(doctorUser, "Doctor");
                
                // Create doctor staff profile
                var doctorProfile = new StaffProfile
                {
                    Id = entityId,
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
                    CreatedBy = entityId
                };
                
                await businessContext.StaffProfiles.AddAsync(doctorProfile);
                await businessContext.SaveChangesAsync();
                
                Console.WriteLine("Doctor user and profile created successfully");
            }
            else
            {
                Console.WriteLine($"Failed to create doctor user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
    }

    private static async Task MarkDatabaseAsSeededAsync(HealinkDbContext dbContext)
    {
        try
        {
            // You can add seed tracking logic here if needed
            Console.WriteLine("Marked database as seeded.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error marking database as seeded: {ex.Message}");
        }
    }
} 