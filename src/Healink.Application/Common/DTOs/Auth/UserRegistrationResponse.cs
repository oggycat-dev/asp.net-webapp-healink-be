using System.Text.Json.Serialization;

namespace Healink.Application.Common.DTOs.Auth;

/// <summary>
/// User registration response DTO
/// </summary>
public class UserRegistrationResponse
{
    /// <summary>
    /// Unique identifier for the created user
    /// </summary>
    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Username of the created account
    /// </summary>
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Email address of the created account
    /// </summary>
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Phone number
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Timestamp when the account was created
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Is email confirmed
    /// </summary>
    public bool EmailConfirmed { get; set; }

    /// <summary>
    /// Indicates if the registration was successful
    /// </summary>
    [JsonPropertyName("is_success")]
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Registration success or error message
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
} 