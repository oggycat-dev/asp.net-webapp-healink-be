using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Healink.Application.Common.DTOs.Auth;

/// <summary>
/// Refresh token request DTO
/// </summary>
public class RefreshTokenRequest
{
    /// <summary>
    /// The refresh token
    /// </summary>
    [Required(ErrorMessage = "Refresh token is required")]
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;
    
    /// <summary>
    /// User ID to refresh token for
    /// </summary>
    [Required(ErrorMessage = "User ID is required")]
    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = string.Empty;
} 