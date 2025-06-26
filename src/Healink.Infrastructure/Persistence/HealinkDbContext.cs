using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Healink.Application.Common.Interfaces;
using Healink.Domain.Entities;
using Healink.Domain.Entities.Identity;
using Healink.Domain.Commons;
using System.Linq.Expressions;

namespace Healink.Infrastructure.Persistence;

/// <summary>
/// Healink business database context
/// </summary>
public class HealinkDbContext : DbContext, IHealinkDbContext
{
    public HealinkDbContext(DbContextOptions<HealinkDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        optionsBuilder.ConfigureWarnings(warnings =>
            warnings.Ignore(CoreEventId.FirstWithoutOrderByAndFilterWarning));
    }

    /// <summary>
    /// Staff profiles DbSet
    /// </summary>
    public DbSet<StaffProfile> StaffProfiles { get; set; } = null!;

    // Identity references (navigation only, not for migrations)
    public DbSet<AppUser> IdentityUsers { get; set; } = null!;
    public DbSet<AppRole> IdentityRoles { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply soft delete filter to all entities inheriting from BaseEntity
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                var falseConstant = Expression.Constant(false);
                var condition = Expression.Equal(property, falseConstant);
                var lambda = Expression.Lambda(condition, parameter);
                
                builder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }

        // Configure StaffProfile
        builder.Entity<StaffProfile>(entity =>
        {
            entity.ToTable("StaffProfiles");
            
            // Required fields
            entity.Property(s => s.FirstName).IsRequired().HasMaxLength(50);
            entity.Property(s => s.LastName).IsRequired().HasMaxLength(50);
            entity.Property(s => s.Email).IsRequired().HasMaxLength(100);
            entity.Property(s => s.PhoneNumber).HasMaxLength(20);
            entity.Property(s => s.Department).HasMaxLength(100);
            entity.Property(s => s.Position).HasMaxLength(100);
            entity.Property(s => s.LicenseNumber).HasMaxLength(50);
            entity.Property(s => s.Address).HasMaxLength(500);
            entity.Property(s => s.EmergencyContact).HasMaxLength(200);

            // Configure relationship with AppUser (external reference)
            entity.HasOne<AppUser>()
                .WithOne()
                .HasForeignKey<StaffProfile>(s => s.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure indexes
            entity.HasIndex(s => s.Email).IsUnique();
            entity.HasIndex(s => s.LicenseNumber).IsUnique().HasFilter("[LicenseNumber] IS NOT NULL");
            entity.HasIndex(s => s.AppUserId).IsUnique();
            entity.HasIndex(s => s.Department);
            entity.HasIndex(s => s.Status);
        });

        // Configure Identity entities (read-only references)
        builder.Entity<AppUser>(entity =>
        {
            entity.ToTable("AspNetUsers");
            entity.HasNoKey(); // This is just for navigation, actual table managed by Identity
        });

        builder.Entity<AppRole>(entity =>
        {
            entity.ToTable("AspNetRoles");
            entity.HasNoKey(); // This is just for navigation, actual table managed by Identity
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Update audit fields
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
} 