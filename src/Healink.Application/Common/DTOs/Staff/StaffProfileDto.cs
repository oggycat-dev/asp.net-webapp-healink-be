using System.Text.Json.Serialization;
using Healink.Domain.Enums;

namespace Healink.Application.Common.DTOs.Staff;

/// <summary>
/// Staff profile data transfer object
/// </summary>
public class StaffProfileDto
{
    /// <summary>
    /// Unique identifier for the staff profile
    /// </summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

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
    /// Status of the staff member
    /// </summary>
    [JsonPropertyName("status")]
    public EntityStatus Status { get; set; }

    /// <summary>
    /// Timestamp when the staff profile was created
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public Gender? Gender { get; set; }
    public string? Address { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? EmployeeId { get; set; }
    public DateTime? HireDate { get; set; }
    public decimal? Salary { get; set; }
    public DateTime? ModifiedAt { get; set; }
} 