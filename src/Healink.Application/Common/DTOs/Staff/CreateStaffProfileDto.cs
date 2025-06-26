using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Healink.Domain.Enums;

namespace Healink.Application.Common.DTOs.Staff;

/// <summary>
/// DTO for creating a new staff profile
/// </summary>
public class CreateStaffProfileDto
{
    /// <summary>
    /// First name of the staff member
    /// </summary>
    [Required(ErrorMessage = "First name is required")]
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    [JsonPropertyName("first_name")]
    public string FirstName { get; set; } = null!;
    
    /// <summary>
    /// Last name of the staff member
    /// </summary>
    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    [JsonPropertyName("last_name")]
    public string LastName { get; set; } = null!;
    
    /// <summary>
    /// Email address of the staff member
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [JsonPropertyName("email")]
    public string Email { get; set; } = null!;
    
    /// <summary>
    /// Phone number of the staff member
    /// </summary>
    [Phone(ErrorMessage = "Invalid phone number format")]
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
    public Gender? Gender { get; set; }
    
    /// <summary>
    /// Physical address of the staff member
    /// </summary>
    [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
    [JsonPropertyName("address")]
    public string? Address { get; set; }
    
    /// <summary>
    /// Department where the staff works
    /// </summary>
    [StringLength(100, ErrorMessage = "Department cannot exceed 100 characters")]
    [JsonPropertyName("department")]
    public string? Department { get; set; }
    
    /// <summary>
    /// Job position/title of the staff member
    /// </summary>
    [StringLength(100, ErrorMessage = "Position cannot exceed 100 characters")]
    [JsonPropertyName("position")]
    public string? Position { get; set; }
    
    /// <summary>
    /// Employee ID of the staff member
    /// </summary>
    [StringLength(20, ErrorMessage = "Employee ID cannot exceed 20 characters")]
    public string? EmployeeId { get; set; }
    
    /// <summary>
    /// Hire date of the staff member
    /// </summary>
    [JsonPropertyName("hire_date")]
    public DateTime? HireDate { get; set; }
    
    /// <summary>
    /// Salary of the staff member
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Salary must be non-negative")]
    [JsonPropertyName("salary")]
    public decimal? Salary { get; set; }
    
    /// <summary>
    /// Password for the staff account
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
    [JsonPropertyName("password")]
    public string Password { get; set; } = null!;
} 