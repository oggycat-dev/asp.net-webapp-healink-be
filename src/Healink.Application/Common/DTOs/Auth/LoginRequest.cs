using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Healink.Application.Common.DTOs.Auth;

/// <summary>
/// Login request model
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Username or email
    /// </summary>
    [Required(ErrorMessage = "Username is required")]
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// User password
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Grant type (default: password)
    /// </summary>
    [JsonPropertyName("grant_type")]
    public string? GrantType { get; set; }
    
    /// <summary>
    /// Remember me option for extended session
    /// </summary>
    [JsonPropertyName("remember_me")]
    public bool RememberMe { get; set; } = false;
} 