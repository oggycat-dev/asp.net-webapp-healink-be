using Healink.Domain.Commons;
using Healink.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Healink.Domain.Entities;

/// <summary>
/// Track user's podcast listening history
/// </summary>
public class PodcastPlayHistory : BaseEntity
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
    /// Reference to podcast
    /// </summary>
    [Required]
    public Guid PodcastId { get; set; }
    
    /// <summary>
    /// Navigation property to podcast
    /// </summary>
    public virtual Podcast Podcast { get; set; } = null!;
    
    /// <summary>
    /// When the user started playing this podcast
    /// </summary>
    [Required]
    public DateTime PlayedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// How many seconds the user listened
    /// </summary>
    public int ListenedSeconds { get; set; } = 0;
    
    /// <summary>
    /// Completion percentage (0-100)
    /// </summary>
    public int CompletionPercentage { get; set; } = 0;
    
    /// <summary>
    /// User's mood when they played this podcast
    /// </summary>
    public MoodType? MoodAtPlay { get; set; }
    
    /// <summary>
    /// User's mood after listening (if recorded)
    /// </summary>
    public MoodType? MoodAfterPlay { get; set; }
    
    /// <summary>
    /// User's rating for this podcast (1-5 stars)
    /// </summary>
    public int? Rating { get; set; }
    
    /// <summary>
    /// Whether user liked this podcast
    /// </summary>
    public bool? IsLiked { get; set; }
    
    /// <summary>
    /// Whether user completed listening
    /// </summary>
    public bool IsCompleted { get; set; } = false;
    
    /// <summary>
    /// How the user discovered this podcast
    /// </summary>
    [MaxLength(50)]
    public string? DiscoverySource { get; set; } // "recommendation", "search", "featured", "category", "qr_code"
    
    /// <summary>
    /// Device type used for playing
    /// </summary>
    [MaxLength(50)]
    public string? DeviceType { get; set; } // "mobile", "desktop", "tablet"
    
    /// <summary>
    /// User's feedback comment (optional)
    /// </summary>
    [MaxLength(500)]
    public string? FeedbackComment { get; set; }
    
    /// <summary>
    /// Whether this was played from a QR code scan
    /// </summary>
    public bool IsFromQrCode { get; set; } = false;
    
    /// <summary>
    /// QR Bracelet ID if played from QR scan
    /// </summary>
    public Guid? QrBraceletId { get; set; }
}
