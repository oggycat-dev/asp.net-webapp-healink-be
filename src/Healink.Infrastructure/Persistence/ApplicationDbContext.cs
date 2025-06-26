using Healink.Application.Common.Interfaces;
using Healink.Domain.Entities;
using Healink.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Healink.Infrastructure.Persistence;

/// <summary>
/// Application database context
/// </summary>
public class ApplicationDbContext : IdentityDbContext<AppUser, AppRole, string>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Staff profiles DbSet
    /// </summary>
    public DbSet<StaffProfile> StaffProfiles { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure StaffProfile entity
        builder.Entity<StaffProfile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Department).HasMaxLength(100);
            entity.Property(e => e.Position).HasMaxLength(100);
            entity.Property(e => e.LicenseNumber).HasMaxLength(50);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.EmergencyContact).HasMaxLength(200);

            // Configure relationship with AppUser
            entity.HasOne(e => e.AppUser)
                .WithOne(u => u.StaffProfile)
                .HasForeignKey<StaffProfile>(e => e.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure indexes
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.LicenseNumber).IsUnique().HasFilter("[LicenseNumber] IS NOT NULL");
        });

        // Configure AppUser entity
        builder.Entity<AppUser>(entity =>
        {
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();
        });

        // Configure AppRole entity
        builder.Entity<AppRole>(entity =>
        {
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(200);
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
} 