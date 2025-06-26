using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Healink.Domain.Enums;

namespace Healink.Application.Common.DTOs.Staff;

/// <summary>
/// DTO for updating a staff profile
/// </summary>
public class UpdateStaffProfileDto
{
    /// <summary>
    /// First name of the staff member
    /// </summary>
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    [JsonPropertyName("first_name")]
    public string? FirstName { get; set; }

    /// <summary>
    /// Last name of the staff member
    /// </summary>
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    [JsonPropertyName("last_name")]
    public string? LastName { get; set; }

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
    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
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

    
    [StringLength(20, ErrorMessage = "Employee ID cannot exceed 20 characters")]
    public string? EmployeeId { get; set; }
    
    public DateTime? HireDate { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Salary must be non-negative")]
    public decimal? Salary { get; set; }
    
    public EntityStatus? Status { get; set; }
} 