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

    /// <summary>
    /// User profiles DbSet for wellness platform
    /// </summary>
    public DbSet<UserProfile> UserProfiles { get; set; } = null!;

    /// <summary>
    /// Podcast categories DbSet
    /// </summary>
    public DbSet<PodcastCategory> PodcastCategories { get; set; } = null!;

    /// <summary>
    /// Podcasts DbSet
    /// </summary>
    public DbSet<Podcast> Podcasts { get; set; } = null!;

    /// <summary>
    /// Podcast mood tags DbSet
    /// </summary>
    public DbSet<PodcastMoodTag> PodcastMoodTags { get; set; } = null!;

    /// <summary>
    /// Podcast play history DbSet
    /// </summary>
    public DbSet<PodcastPlayHistory> PodcastPlayHistory { get; set; } = null!;

    /// <summary>
    /// User mood preferences DbSet
    /// </summary>
    public DbSet<UserMoodPreference> UserMoodPreferences { get; set; } = null!;

    // Identity references (navigation only, not for migrations)
    // These are commented out to avoid table creation conflicts
    // public DbSet<AppUser> IdentityUsers { get; set; } = null!;
    // public DbSet<AppRole> IdentityRoles { get; set; } = null!;

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

            // Configure AppUser reference without relationship (to avoid conflicts)
            entity.Property(s => s.AppUserId).IsRequired().HasMaxLength(450);

            // Configure indexes
            entity.HasIndex(s => s.Email).IsUnique();
            entity.HasIndex(s => s.LicenseNumber).IsUnique().HasFilter("[LicenseNumber] IS NOT NULL");
            entity.HasIndex(s => s.AppUserId).IsUnique();
            entity.HasIndex(s => s.Department);
            entity.HasIndex(s => s.Status);
        });

        // Configure UserProfile
        builder.Entity<UserProfile>(entity =>
        {
            entity.ToTable("UserProfiles");
            
            entity.Property(u => u.AppUserId).IsRequired().HasMaxLength(450);
            entity.Property(u => u.DisplayName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.FirstName).HasMaxLength(50);
            entity.Property(u => u.LastName).HasMaxLength(50);
            entity.Property(u => u.TimeZone).HasMaxLength(100);
            entity.Property(u => u.Language).HasMaxLength(10).HasDefaultValue("vi-VN");
            entity.Property(u => u.Bio).HasMaxLength(500);
            entity.Property(u => u.ProfilePictureUrl).HasMaxLength(500);
            entity.Property(u => u.DailyReminderTime).HasMaxLength(5).HasDefaultValue("09:00");

            // Configure indexes
            entity.HasIndex(u => u.AppUserId).IsUnique();
            entity.HasIndex(u => u.DisplayName);
            entity.HasIndex(u => u.CurrentMood);
        });

        // Configure PodcastCategory
        builder.Entity<PodcastCategory>(entity =>
        {
            entity.ToTable("PodcastCategories");
            
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
            entity.Property(c => c.Description).HasMaxLength(500);
            entity.Property(c => c.Slug).IsRequired().HasMaxLength(100);
            entity.Property(c => c.IconUrl).HasMaxLength(500);
            entity.Property(c => c.CoverImageUrl).HasMaxLength(500);
            entity.Property(c => c.ColorTheme).HasMaxLength(7);
            entity.Property(c => c.SecondaryMoods).HasMaxLength(200);

            // Configure indexes
            entity.HasIndex(c => c.Slug).IsUnique();
            entity.HasIndex(c => c.ContentType);
            entity.HasIndex(c => c.PrimaryMood);
            entity.HasIndex(c => c.IsFeatured);
            entity.HasIndex(c => c.DisplayOrder);
        });

        // Configure Podcast
        builder.Entity<Podcast>(entity =>
        {
            entity.ToTable("Podcasts");
            
            entity.Property(p => p.Title).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Description).HasMaxLength(1000);
            entity.Property(p => p.AudioUrl).IsRequired().HasMaxLength(500);
            entity.Property(p => p.CoverImageUrl).HasMaxLength(500);
            entity.Property(p => p.AudioFormat).HasMaxLength(10);
            entity.Property(p => p.AuthorName).HasMaxLength(100);
            entity.Property(p => p.NarratorName).HasMaxLength(100);
            entity.Property(p => p.Language).HasMaxLength(10).HasDefaultValue("vi-VN");
            entity.Property(p => p.Tags).HasMaxLength(500);
            entity.Property(p => p.UploadedByUserId).HasMaxLength(450);
            entity.Property(p => p.AverageRating).HasPrecision(3, 2);

            // Configure relationships
            entity.HasOne(p => p.Category)
                .WithMany(c => c.Podcasts)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure indexes
            entity.HasIndex(p => p.Title);
            entity.HasIndex(p => p.Status);
            entity.HasIndex(p => p.PrimaryMood);
            entity.HasIndex(p => p.CategoryId);
            entity.HasIndex(p => p.IsFeatured);
            entity.HasIndex(p => p.PublishedAt);
            entity.HasIndex(p => p.PlayCount);
        });

        // Configure PodcastMoodTag
        builder.Entity<PodcastMoodTag>(entity =>
        {
            entity.ToTable("PodcastMoodTags");
            
            entity.Property(mt => mt.TaggedBy).HasMaxLength(50);

            // Configure relationships
            entity.HasOne(mt => mt.Podcast)
                .WithMany(p => p.MoodTags)
                .HasForeignKey(mt => mt.PodcastId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure indexes
            entity.HasIndex(mt => new { mt.PodcastId, mt.MoodType }).IsUnique();
            entity.HasIndex(mt => mt.MoodType);
            entity.HasIndex(mt => mt.IsPrimary);
        });

        // Configure PodcastPlayHistory
        builder.Entity<PodcastPlayHistory>(entity =>
        {
            entity.ToTable("PodcastPlayHistory");
            
            entity.Property(ph => ph.DiscoverySource).HasMaxLength(50);
            entity.Property(ph => ph.DeviceType).HasMaxLength(50);
            entity.Property(ph => ph.FeedbackComment).HasMaxLength(500);

            // Configure relationships
            entity.HasOne(ph => ph.UserProfile)
                .WithMany(u => u.PlayHistory)
                .HasForeignKey(ph => ph.UserProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ph => ph.Podcast)
                .WithMany(p => p.PlayHistory)
                .HasForeignKey(ph => ph.PodcastId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure indexes
            entity.HasIndex(ph => ph.UserProfileId);
            entity.HasIndex(ph => ph.PodcastId);
            entity.HasIndex(ph => ph.PlayedAt);
            entity.HasIndex(ph => ph.MoodAtPlay);
            entity.HasIndex(ph => ph.IsCompleted);
            entity.HasIndex(ph => ph.IsFromQrCode);
        });

        // Configure UserMoodPreference
        builder.Entity<UserMoodPreference>(entity =>
        {
            entity.ToTable("UserMoodPreferences");
            
            entity.Property(mp => mp.PreferredContentTypes).HasMaxLength(200);
            entity.Property(mp => mp.TypicalTimeOfDay).HasMaxLength(5);
            entity.Property(mp => mp.Frequency).HasMaxLength(20);
            entity.Property(mp => mp.Notes).HasMaxLength(500);

            // Configure relationships
            entity.HasOne(mp => mp.UserProfile)
                .WithMany(u => u.MoodPreferences)
                .HasForeignKey(mp => mp.UserProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure indexes
            entity.HasIndex(mp => new { mp.UserProfileId, mp.MoodType }).IsUnique();
            entity.HasIndex(mp => mp.MoodType);
            entity.HasIndex(mp => mp.IsEnabled);
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