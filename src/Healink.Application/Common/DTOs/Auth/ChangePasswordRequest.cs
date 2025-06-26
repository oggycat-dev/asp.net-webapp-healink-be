using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Healink.Application.Common.DTOs.Auth;

/// <summary>
/// Change password request DTO
/// </summary>
public class ChangePasswordRequest
{
    /// <summary>
    /// Current password
    /// </summary>
    [Required(ErrorMessage = "Current password is required")]
    [JsonPropertyName("current_password")]
    public string CurrentPassword { get; set; } = string.Empty;
    
    /// <summary>
    /// New password
    /// </summary>
    [Required(ErrorMessage = "New password is required")]
    [MinLength(8, ErrorMessage = "New password must be at least 8 characters")]
    [JsonPropertyName("new_password")]
    public string NewPassword { get; set; } = string.Empty;
} 