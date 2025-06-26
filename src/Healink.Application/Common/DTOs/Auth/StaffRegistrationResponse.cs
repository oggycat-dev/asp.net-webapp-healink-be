using System.Text.Json.Serialization;

namespace Healink.Application.Common.DTOs.Auth;

/// <summary>
/// Staff registration response DTO
/// </summary>
public class StaffRegistrationResponse
{
    /// <summary>
    /// Unique identifier for the created staff user
    /// </summary>
    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Staff profile identifier
    /// </summary>
    [JsonPropertyName("staff_id")]
    public Guid StaffId { get; set; }
    
    /// <summary>
    /// Staff profile identifier (alias for backward compatibility)
    /// </summary>
    [JsonPropertyName("entity_id")]
    public Guid EntityId 
    { 
        get => StaffId; 
        set => StaffId = value; 
    }
    
    /// <summary>
    /// Username of the created staff account
    /// </summary>
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// Email address of the created staff account
    /// </summary>
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Full name of the staff member
    /// </summary>
    [JsonPropertyName("full_name")]
    public string FullName { get; set; } = string.Empty;
    
    /// <summary>
    /// Department of the staff member
    /// </summary>
    [JsonPropertyName("department")]
    public string? Department { get; set; }
    
    /// <summary>
    /// Timestamp when the staff account was created
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Indicates if the staff registration was successful
    /// </summary>
    [JsonPropertyName("is_success")]
    public bool IsSuccess { get; set; }
    
    /// <summary>
    /// Registration success or error message
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
} 