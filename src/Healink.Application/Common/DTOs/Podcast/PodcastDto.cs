using Healink.Domain.Enums;

namespace Healink.Application.Common.DTOs.Podcast;

/// <summary>
/// Podcast data transfer object
/// </summary>
public class PodcastDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string AudioUrl { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public int DurationSeconds { get; set; }
    public string FormattedDuration { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string? AudioFormat { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public PodcastStatus Status { get; set; }
    public MoodType PrimaryMood { get; set; }
    public string? AuthorName { get; set; }
    public string? NarratorName { get; set; }
    public string Language { get; set; } = string.Empty;
    public string? Tags { get; set; }
    public int PlayCount { get; set; }
    public int LikeCount { get; set; }
    public decimal AverageRating { get; set; }
    public int RatingCount { get; set; }
    public bool IsFeatured { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Additional properties for API responses
    public List<MoodType> MoodTags { get; set; } = new();
    public bool IsLikedByCurrentUser { get; set; }
    public int? CurrentUserRating { get; set; }
    public bool HasBeenPlayedByCurrentUser { get; set; }
    public int? LastPlayedPosition { get; set; }
}
