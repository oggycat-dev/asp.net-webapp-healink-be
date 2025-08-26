using Healink.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Healink.Application.Common.DTOs.Podcast;

/// <summary>
/// Request to record podcast play
/// </summary>
public class PodcastPlayRequest
{
    [Required]
    public Guid PodcastId { get; set; }
    
    public MoodType? MoodAtPlay { get; set; }
    
    [MaxLength(50)]
    public string? DiscoverySource { get; set; }
    
    [MaxLength(50)]
    public string? DeviceType { get; set; }
    
    public bool IsFromQrCode { get; set; } = false;
    
    public Guid? QrBraceletId { get; set; }
    
    public int StartPosition { get; set; } = 0;
}
