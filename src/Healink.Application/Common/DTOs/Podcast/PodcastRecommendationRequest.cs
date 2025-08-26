using Healink.Domain.Enums;

namespace Healink.Application.Common.DTOs.Podcast;

/// <summary>
/// Request for mood-based podcast recommendations
/// </summary>
public class PodcastRecommendationRequest
{
    public MoodType? CurrentMood { get; set; }
    
    public List<MoodType> PreferredMoods { get; set; } = new();
    
    public List<ContentType> PreferredContentTypes { get; set; } = new();
    
    public int? MaxDurationMinutes { get; set; }
    
    public int? MinDurationMinutes { get; set; }
    
    public int Count { get; set; } = 10;
    
    public bool ExcludeRecentlyPlayed { get; set; } = true;
    
    public int ExcludeRecentDays { get; set; } = 7;
    
    public bool IncludeFeatured { get; set; } = true;
    
    public string? Language { get; set; } = "vi-VN";
}
