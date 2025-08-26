using Healink.Domain.Commons;
using Healink.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Healink.Domain.Entities;

/// <summary>
/// User's mood preferences for personalized recommendations
/// </summary>
public class UserMoodPreference : BaseEntity
{
    /// <summary>
    /// Reference to user profile
    /// </summary>
    [Required]
    public Guid UserProfileId { get; set; }
    
    /// <summary>
    /// Navigation property to user profile
    /// </summary>
    public virtual UserProfile UserProfile { get; set; } = null!;
    
    /// <summary>
    /// Mood type this preference applies to
    /// </summary>
    [Required]
    public MoodType MoodType { get; set; }
    
    /// <summary>
    /// Preferred content types for this mood (JSON array)
    /// </summary>
    [MaxLength(200)]
    public string? PreferredContentTypes { get; set; }
    
    /// <summary>
    /// Preferred podcast duration in minutes for this mood
    /// </summary>
    public int? PreferredDurationMinutes { get; set; }
    
    /// <summary>
    /// User's preference weight for this mood (1-10)
    /// </summary>
    public int PreferenceWeight { get; set; } = 5;
    
    /// <summary>
    /// Whether user wants this mood to be included in recommendations
    /// </summary>
    public bool IsEnabled { get; set; } = true;
    
    /// <summary>
    /// Time of day when user typically experiences this mood (24-hour format)
    /// </summary>
    [MaxLength(5)]
    public string? TypicalTimeOfDay { get; set; }
    
    /// <summary>
    /// How often user experiences this mood
    /// </summary>
    [MaxLength(20)]
    public string? Frequency { get; set; } // "daily", "weekly", "monthly", "rarely"
    
    /// <summary>
    /// User's notes about this mood preference
    /// </summary>
    [MaxLength(500)]
    public string? Notes { get; set; }
    
    /// <summary>
    /// Last time this preference was used for recommendation
    /// </summary>
    public DateTime? LastUsedAt { get; set; }
    
    /// <summary>
    /// Number of times this preference has been used
    /// </summary>
    public int UsageCount { get; set; } = 0;
}
