using Healink.Domain.Enums;

namespace Healink.Application.Common.DTOs.Podcast;

/// <summary>
/// Podcast category data transfer object
/// </summary>
public class PodcastCategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Slug { get; set; } = string.Empty;
    public ContentType ContentType { get; set; }
    public string? IconUrl { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? ColorTheme { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsFeatured { get; set; }
    public MoodType? PrimaryMood { get; set; }
    public List<MoodType> SecondaryMoods { get; set; } = new();
    public int PodcastCount { get; set; }
    public int TotalPlays { get; set; }
    public DateTime CreatedAt { get; set; }
}
