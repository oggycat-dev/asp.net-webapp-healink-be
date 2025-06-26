using System.Text.Json.Serialization;
using Healink.Domain.Enums;

namespace Healink.Application.Common.DTOs.Staff;

/// <summary>
/// Response DTO for staff information
/// </summary>
public class StaffResponse
{
    /// <summary>
    /// Unique identifier for the staff member
    /// </summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
    
    /// <summary>
    /// Identity user ID
    /// </summary>
    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// First name of the staff member
    /// </summary>
    [JsonPropertyName("first_name")]
    public string FirstName { get; set; } = string.Empty;
    
    /// <summary>
    /// Last name of the staff member
    /// </summary>
    [JsonPropertyName("last_name")]
    public string LastName { get; set; } = string.Empty;
    
    /// <summary>
    /// Full name of the staff member
    /// </summary>
    [JsonPropertyName("full_name")]
    public string FullName { get; set; } = string.Empty;
    
    /// <summary>
    /// Email address of the staff member
    /// </summary>
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Phone number of the staff member
    /// </summary>
    [JsonPropertyName("phone_number")]
    public string? PhoneNumber { get; set; }
    
    /// <summary>
    /// Date of birth of the staff member
    /// </summary>
    [JsonPropertyName("date_of_birth")]
    public DateTime? DateOfBirth { get; set; }
    
    /// <summary>
    /// Gender of the staff member
    /// </summary>
    [JsonPropertyName("gender")]
    public Gender Gender { get; set; }
    
    /// <summary>
    /// Physical address of the staff member
    /// </summary>
    [JsonPropertyName("address")]
    public string? Address { get; set; }
    
    /// <summary>
    /// Department where the staff works
    /// </summary>
    [JsonPropertyName("department")]
    public string? Department { get; set; }
    
    /// <summary>
    /// Job position/title of the staff member
    /// </summary>
    [JsonPropertyName("position")]
    public string? Position { get; set; }
    
    /// <summary>
    /// Professional license number
    /// </summary>
    [JsonPropertyName("license_number")]
    public string? LicenseNumber { get; set; }
    
    /// <summary>
    /// Emergency contact information
    /// </summary>
    [JsonPropertyName("emergency_contact")]
    public string? EmergencyContact { get; set; }
    
    /// <summary>
    /// Date when the staff joined
    /// </summary>
    [JsonPropertyName("join_date")]
    public DateTime JoinDate { get; set; }
    
    /// <summary>
    /// Status of the staff member
    /// </summary>
    [JsonPropertyName("status")]
    public EntityStatus Status { get; set; }
    
    /// <summary>
    /// Timestamp when the staff record was created
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Timestamp when the staff record was last updated
    /// </summary>
    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt { get; set; }
} 