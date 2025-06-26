using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Healink.Domain.Enums;

namespace Healink.Application.Common.DTOs.Auth;

/// <summary>
/// Staff registration request DTO
/// </summary>
public class StaffRegistrationRequest
{
    /// <summary>
    /// Username for the new staff account
    /// </summary>
    [Required(ErrorMessage = "Username is required")]
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// Email address for the new staff account
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Phone number
    /// </summary>
    [Required(ErrorMessage = "Phone number is required")]
    public string PhoneNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// Password for the new staff account
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
    
    /// <summary>
    /// First name of the staff member
    /// </summary>
    [Required(ErrorMessage = "First name is required")]
    [JsonPropertyName("first_name")]
    public string FirstName { get; set; } = string.Empty;
    
    /// <summary>
    /// Last name of the staff member
    /// </summary>
    [Required(ErrorMessage = "Last name is required")]
    [JsonPropertyName("last_name")]
    public string LastName { get; set; } = string.Empty;
    
    /// <summary>
    /// Department of the staff member
    /// </summary>
    [JsonPropertyName("department")]
    public string? Department { get; set; }
    
    /// <summary>
    /// Role assignment
    /// </summary>
    [Required(ErrorMessage = "Role is required")]
    public string Role { get; set; } = "Staff";
} 