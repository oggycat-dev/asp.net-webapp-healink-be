using Healink.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Healink.Application.Common.DTOs.Podcast;

/// <summary>
/// Request to create a new podcast
/// </summary>
public class CreatePodcastRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    [Required]
    public Guid CategoryId { get; set; }
    
    [Required]
    public MoodType PrimaryMood { get; set; }
    
    [MaxLength(100)]
    public string? AuthorName { get; set; }
    
    [MaxLength(100)]
    public string? NarratorName { get; set; }
    
    [MaxLength(10)]
    public string Language { get; set; } = "vi-VN";
    
    public string? Transcript { get; set; }
    
    [MaxLength(500)]
    public string? Tags { get; set; }
    
    public List<MoodType> AdditionalMoods { get; set; } = new();
    
    public bool IsFeatured { get; set; } = false;
}
