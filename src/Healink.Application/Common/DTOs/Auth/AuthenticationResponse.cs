using System.Text.Json.Serialization;

namespace Healink.Application.Common.DTOs.Auth;

/// <summary>
/// Authentication response model containing user information and tokens
/// </summary>
public class AuthenticationResponse
{
    /// <summary>
    /// Unique identifier for the user
    /// </summary>
    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Entity ID (Staff Profile ID, Patient ID, etc.)
    /// </summary>
    [JsonPropertyName("entity_id")]
    public Guid? EntityId { get; set; }

    /// <summary>
    /// Username for the authenticated user
    /// </summary>
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Email address of the authenticated user
    /// </summary>
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// First name
    /// </summary>
    [JsonPropertyName("first_name")]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Last name
    /// </summary>
    [JsonPropertyName("last_name")]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Full name of the authenticated user
    /// </summary>
    [JsonPropertyName("full_name")]
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// User roles
    /// </summary>
    [JsonPropertyName("roles")]
    public List<string> Roles { get; set; } = new();

    /// <summary>
    /// Access token for API authentication
    /// </summary>
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Alias for AccessToken to maintain compatibility (settable)
    /// </summary>
    [JsonPropertyName("token")]
    public string Token 
    { 
        get => AccessToken; 
        set => AccessToken = value; 
    }

    /// <summary>
    /// Refresh token for obtaining new access tokens
    /// </summary>
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Token expiration time in minutes
    /// </summary>
    [JsonPropertyName("expires_in_minutes")]
    public int ExpiresInMinutes { get; set; }

    /// <summary>
    /// Token expiration timestamp
    /// </summary>
    [JsonPropertyName("expires_at")]
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Indicates if the user account is active
    /// </summary>
    [JsonPropertyName("is_active")]
    public bool IsActive { get; set; } = true;
} 