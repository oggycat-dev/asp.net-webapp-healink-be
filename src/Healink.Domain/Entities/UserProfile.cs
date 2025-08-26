using Healink.Domain.Commons;
using Healink.Domain.Entities.Identity;
using Healink.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Healink.Domain.Entities;

/// <summary>
/// User profile for wellness platform
/// </summary>
public class UserProfile : BaseEntity
{
    /// <summary>
    /// Reference to AppUser identity
    /// </summary>
    [Required]
    public string AppUserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Navigation property to AppUser
    /// </summary>
    public virtual AppUser AppUser { get; set; } = null!;
    
    /// <summary>
    /// Display name for the platform
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string DisplayName { get; set; } = string.Empty;
    
    /// <summary>
    /// User's first name
    /// </summary>
    [MaxLength(50)]
    public string? FirstName { get; set; }
    
    /// <summary>
    /// User's last name
    /// </summary>
    [MaxLength(50)]
    public string? LastName { get; set; }
    
    /// <summary>
    /// Date of birth
    /// </summary>
    public DateTime? DateOfBirth { get; set; }
    
    /// <summary>
    /// User's gender
    /// </summary>
    public Gender? Gender { get; set; }
    
    /// <summary>
    /// User's timezone
    /// </summary>
    [MaxLength(100)]
    public string? TimeZone { get; set; }
    
    /// <summary>
    /// User's preferred language
    /// </summary>
    [MaxLength(10)]
    public string Language { get; set; } = "vi-VN";
    
    /// <summary>
    /// User's current mood (last recorded)
    /// </summary>
    public MoodType? CurrentMood { get; set; }
    
    /// <summary>
    /// When mood was last updated
    /// </summary>
    public DateTime? MoodUpdatedAt { get; set; }
    
    /// <summary>
    /// User's bio or description
    /// </summary>
    [MaxLength(500)]
    public string? Bio { get; set; }
    
    /// <summary>
    /// Profile picture URL (S3 path)
    /// </summary>
    [MaxLength(500)]
    public string? ProfilePictureUrl { get; set; }
    
    /// <summary>
    /// Whether user allows mood-based recommendations
    /// </summary>
    public bool EnableMoodRecommendations { get; set; } = true;
    
    /// <summary>
    /// Whether user wants daily wellness reminders
    /// </summary>
    public bool EnableDailyReminders { get; set; } = true;
    
    /// <summary>
    /// Preferred time for daily reminders (24-hour format)
    /// </summary>
    [MaxLength(5)]
    public string? DailyReminderTime { get; set; } = "09:00";
    
    /// <summary>
    /// Total listening time in minutes
    /// </summary>
    public int TotalListeningMinutes { get; set; } = 0;
    
    /// <summary>
    /// Number of diary entries written
    /// </summary>
    public int DiaryEntriesCount { get; set; } = 0;
    
    /// <summary>
    /// User's mood preferences for recommendations
    /// </summary>
    public virtual ICollection<UserMoodPreference> MoodPreferences { get; set; } = new List<UserMoodPreference>();
    
    /// <summary>
    /// User's podcast play history
    /// </summary>
    public virtual ICollection<PodcastPlayHistory> PlayHistory { get; set; } = new List<PodcastPlayHistory>();
}
