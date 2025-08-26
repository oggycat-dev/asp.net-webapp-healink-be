using Healink.Domain.Commons;
using Healink.Domain.Entities.Identity;
using Healink.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Healink.Domain.Entities;

/// <summary>
/// Podcast entity for wellness content
/// </summary>
public class Podcast : BaseEntity
{
    /// <summary>
    /// Podcast title
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Podcast description
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Audio file URL in AWS S3
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string AudioUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Cover image URL in AWS S3
    /// </summary>
    [MaxLength(500)]
    public string? CoverImageUrl { get; set; }
    
    /// <summary>
    /// Podcast duration in seconds
    /// </summary>
    [Required]
    public int DurationSeconds { get; set; }
    
    /// <summary>
    /// Audio file size in bytes
    /// </summary>
    public long FileSizeBytes { get; set; } = 0;
    
    /// <summary>
    /// Audio file format (mp3, wav, etc.)
    /// </summary>
    [MaxLength(10)]
    public string? AudioFormat { get; set; }
    
    /// <summary>
    /// Podcast category
    /// </summary>
    [Required]
    public Guid CategoryId { get; set; }
    
    /// <summary>
    /// Navigation property to category
    /// </summary>
    public virtual PodcastCategory Category { get; set; } = null!;
    
    /// <summary>
    /// Podcast status
    /// </summary>
    [Required]
    public new PodcastStatus Status { get; set; } = PodcastStatus.Draft;
    
    /// <summary>
    /// Primary mood for this podcast
    /// </summary>
    [Required]
    public MoodType PrimaryMood { get; set; }
    
    /// <summary>
    /// Author/Creator name
    /// </summary>
    [MaxLength(100)]
    public string? AuthorName { get; set; }
    
    /// <summary>
    /// Narrator name (if different from author)
    /// </summary>
    [MaxLength(100)]
    public string? NarratorName { get; set; }
    
    /// <summary>
    /// Language of the podcast
    /// </summary>
    [MaxLength(10)]
    public string Language { get; set; } = "vi-VN";
    
    /// <summary>
    /// Transcript of the podcast (for accessibility)
    /// </summary>
    public string? Transcript { get; set; }
    
    /// <summary>
    /// Tags for searchability (comma-separated)
    /// </summary>
    [MaxLength(500)]
    public string? Tags { get; set; }
    
    /// <summary>
    /// Number of times this podcast has been played
    /// </summary>
    public int PlayCount { get; set; } = 0;
    
    /// <summary>
    /// Number of times this podcast has been liked
    /// </summary>
    public int LikeCount { get; set; } = 0;
    
    /// <summary>
    /// Average rating (1-5 stars)
    /// </summary>
    public decimal AverageRating { get; set; } = 0;
    
    /// <summary>
    /// Number of ratings
    /// </summary>
    public int RatingCount { get; set; } = 0;
    
    /// <summary>
    /// Whether this podcast is featured
    /// </summary>
    public bool IsFeatured { get; set; } = false;
    
    /// <summary>
    /// When this podcast was published
    /// </summary>
    public DateTime? PublishedAt { get; set; }
    
    /// <summary>
    /// User who uploaded this podcast (if user-generated)
    /// </summary>
    public string? UploadedByUserId { get; set; }
    
    /// <summary>
    /// Navigation property to uploader
    /// </summary>
    public virtual AppUser? UploadedByUser { get; set; }
    
    /// <summary>
    /// Mood tags associated with this podcast
    /// </summary>
    public virtual ICollection<PodcastMoodTag> MoodTags { get; set; } = new List<PodcastMoodTag>();
    
    /// <summary>
    /// Play history for this podcast
    /// </summary>
    public virtual ICollection<PodcastPlayHistory> PlayHistory { get; set; } = new List<PodcastPlayHistory>();
    
    /// <summary>
    /// Duration formatted as string (computed property)
    /// </summary>
    public string FormattedDuration
    {
        get
        {
            var timespan = TimeSpan.FromSeconds(DurationSeconds);
            if (timespan.Hours > 0)
                return timespan.ToString(@"h\:mm\:ss");
            return timespan.ToString(@"m\:ss");
        }
    }
}
