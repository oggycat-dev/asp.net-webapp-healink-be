using Healink.Domain.Commons;
using Healink.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Healink.Domain.Entities;

/// <summary>
/// Category for organizing podcasts
/// </summary>
public class PodcastCategory : BaseEntity
{
    /// <summary>
    /// Category name
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Category description
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Category slug for URLs
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Slug { get; set; } = string.Empty;
    
    /// <summary>
    /// Content type associated with this category
    /// </summary>
    [Required]
    public ContentType ContentType { get; set; }
    
    /// <summary>
    /// Category icon URL (S3 path)
    /// </summary>
    [MaxLength(500)]
    public string? IconUrl { get; set; }
    
    /// <summary>
    /// Category cover image URL (S3 path)
    /// </summary>
    [MaxLength(500)]
    public string? CoverImageUrl { get; set; }
    
    /// <summary>
    /// Category color theme (hex code)
    /// </summary>
    [MaxLength(7)]
    public string? ColorTheme { get; set; }
    
    /// <summary>
    /// Display order for sorting
    /// </summary>
    public int DisplayOrder { get; set; } = 0;
    
    /// <summary>
    /// Whether this category is featured
    /// </summary>
    public bool IsFeatured { get; set; } = false;
    
    /// <summary>
    /// Primary mood associated with this category
    /// </summary>
    public MoodType? PrimaryMood { get; set; }
    
    /// <summary>
    /// Secondary moods associated with this category
    /// </summary>
    [MaxLength(200)]
    public string? SecondaryMoods { get; set; } // JSON array of MoodType
    
    /// <summary>
    /// Number of podcasts in this category
    /// </summary>
    public int PodcastCount { get; set; } = 0;
    
    /// <summary>
    /// Total plays for podcasts in this category
    /// </summary>
    public int TotalPlays { get; set; } = 0;
    
    /// <summary>
    /// Podcasts in this category
    /// </summary>
    public virtual ICollection<Podcast> Podcasts { get; set; } = new List<Podcast>();
}
