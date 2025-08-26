using Healink.Domain.Commons;
using Healink.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Healink.Domain.Entities;

/// <summary>
/// Many-to-many relationship between Podcast and MoodType
/// </summary>
public class PodcastMoodTag : BaseEntity
{
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
    /// Mood type associated with this podcast
    /// </summary>
    [Required]
    public MoodType MoodType { get; set; }
    
    /// <summary>
    /// Weight/Strength of association (1-10)
    /// Higher weight means stronger association
    /// </summary>
    public int Weight { get; set; } = 5;
    
    /// <summary>
    /// Whether this is the primary mood for the podcast
    /// </summary>
    public bool IsPrimary { get; set; } = false;
    
    /// <summary>
    /// Number of users who agreed with this mood association
    /// </summary>
    public int UserAgreementCount { get; set; } = 0;
    
    /// <summary>
    /// Who added this mood tag (user ID or system)
    /// </summary>
    [MaxLength(50)]
    public string? TaggedBy { get; set; }
}
