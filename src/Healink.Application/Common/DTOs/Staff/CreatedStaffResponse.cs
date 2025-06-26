using System.Text.Json.Serialization;
using Healink.Domain.Enums;

namespace Healink.Application.Common.DTOs.Staff;

/// <summary>
/// Response DTO for successfully created staff member
/// </summary>
public class CreatedStaffResponse
{
    /// <summary>
    /// Unique identifier for the created staff member
    /// </summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
    
    /// <summary>
    /// Identity user ID for the created staff account
    /// </summary>
    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Username of the created staff account
    /// </summary>
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// Email address of the created staff member
    /// </summary>
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// First name of the created staff member
    /// </summary>
    [JsonPropertyName("first_name")]
    public string FirstName { get; set; } = string.Empty;
    
    /// <summary>
    /// Last name of the created staff member
    /// </summary>
    [JsonPropertyName("last_name")]
    public string LastName { get; set; } = string.Empty;
    
    /// <summary>
    /// Full name of the created staff member
    /// </summary>
    [JsonPropertyName("full_name")]
    public string FullName { get; set; } = string.Empty;
    
    /// <summary>
    /// Department of the created staff member
    /// </summary>
    [JsonPropertyName("department")]
    public string? Department { get; set; }
    
    /// <summary>
    /// Role/position assigned to the staff member
    /// </summary>
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;
    
    /// <summary>
    /// Status of the staff member
    /// </summary>
    [JsonPropertyName("status")]
    public EntityStatus Status { get; set; }
    
    /// <summary>
    /// Timestamp when the staff member was created
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Indicates if the staff creation was successful
    /// </summary>
    [JsonPropertyName("is_success")]
    public bool IsSuccess { get; set; }
    
    /// <summary>
    /// Success or error message
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}