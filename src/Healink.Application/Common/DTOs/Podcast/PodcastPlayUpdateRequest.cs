using Healink.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Healink.Application.Common.DTOs.Podcast;

/// <summary>
/// Request to update podcast play progress
/// </summary>
public class PodcastPlayUpdateRequest
{
    [Required]
    public Guid PlayHistoryId { get; set; }
    
    [Required]
    public int ListenedSeconds { get; set; }
    
    [Required]
    public int CompletionPercentage { get; set; }
    
    public MoodType? MoodAfterPlay { get; set; }
    
    [Range(1, 5)]
    public int? Rating { get; set; }
    
    public bool? IsLiked { get; set; }
    
    public bool IsCompleted { get; set; } = false;
    
    [MaxLength(500)]
    public string? FeedbackComment { get; set; }
}
