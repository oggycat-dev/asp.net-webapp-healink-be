using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Healink.Domain.Enums;

namespace Healink.Application.Common.DTOs.Staff;

/// <summary>
/// Request DTO for creating a new staff member
/// </summary>
public class CreateStaffRequest
{
    /// <summary>
    /// Username for the staff account
    /// </summary>
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// Email address of the staff member
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Phone number of the staff member
    /// </summary>
    [Phone(ErrorMessage = "Invalid phone number format")]
    [JsonPropertyName("phone_number")]
    public string? PhoneNumber { get; set; }
    
    /// <summary>
    /// Password for the staff account
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
    
    /// <summary>
    /// First name of the staff member
    /// </summary>
    [Required(ErrorMessage = "First name is required")]
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    [JsonPropertyName("first_name")]
    public string FirstName { get; set; } = string.Empty;
    
    /// <summary>
    /// Last name of the staff member
    /// </summary>
    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    [JsonPropertyName("last_name")]
    public string LastName { get; set; } = string.Empty;
    
    /// <summary>
    /// Date of birth of the staff member
    /// </summary>
    [JsonPropertyName("date_of_birth")]
    public DateTime? DateOfBirth { get; set; }
    
    /// <summary>
    /// Gender of the staff member
    /// </summary>
    [JsonPropertyName("gender")]
    public Gender? Gender { get; set; }
    
    /// <summary>
    /// Physical address of the staff member
    /// </summary>
    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    [JsonPropertyName("address")]
    public string? Address { get; set; }
    
    /// <summary>
    /// Role/position in the organization
    /// </summary>
    [Required(ErrorMessage = "Role is required")]
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;
    
    /// <summary>
    /// Department where the staff works
    /// </summary>
    [JsonPropertyName("department")]
    public string? Department { get; set; }
    
    /// <summary>
    /// Professional license number (for doctors, nurses, etc.)
    /// </summary>
    [JsonPropertyName("license_number")]
    public string? LicenseNumber { get; set; }
    
    /// <summary>
    /// Emergency contact information
    /// </summary>
    [StringLength(200, ErrorMessage = "Emergency contact cannot exceed 200 characters")]
    [JsonPropertyName("emergency_contact")]
    public string? EmergencyContact { get; set; }
} 