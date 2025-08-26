using Healink.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Healink.Application.Common.DTOs.Podcast;

/// <summary>
/// Request to update an existing podcast
/// </summary>
public class UpdatePodcastRequest
{
    [MaxLength(200)]
    public string? Title { get; set; }
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    public Guid? CategoryId { get; set; }
    
    public MoodType? PrimaryMood { get; set; }
    
    [MaxLength(100)]
    public string? AuthorName { get; set; }
    
    [MaxLength(100)]
    public string? NarratorName { get; set; }
    
    [MaxLength(10)]
    public string? Language { get; set; }
    
    public string? Transcript { get; set; }
    
    [MaxLength(500)]
    public string? Tags { get; set; }
    
    public List<MoodType>? AdditionalMoods { get; set; }
    
    public bool? IsFeatured { get; set; }
    
    public PodcastStatus? Status { get; set; }
}
