using MediatR;
using Healink.Application.Common.DTOs.Staff;
using Healink.Application.Common.Models;
using Healink.Domain.Enums;

namespace Healink.Application.Features.Staff.Commands.CreateStaff;

/// <summary>
/// Command to create a new staff member
/// </summary>
public record CreateStaffCommand : IRequest<Result<CreatedStaffResponse>>
{
    /// <summary>
    /// Username for login
    /// </summary>
    public string Username { get; init; } = string.Empty;
    
    /// <summary>
    /// Email address
    /// </summary>
    public string Email { get; init; } = string.Empty;
    
    /// <summary>
    /// Phone number
    /// </summary>
    public string PhoneNumber { get; init; } = string.Empty;
    
    /// <summary>
    /// Password for the account
    /// </summary>
    public string Password { get; init; } = string.Empty;
    
    /// <summary>
    /// First name
    /// </summary>
    public string FirstName { get; init; } = string.Empty;
    
    /// <summary>
    /// Last name
    /// </summary>
    public string LastName { get; init; } = string.Empty;
    
    /// <summary>
    /// Gender
    /// </summary>
    public Gender Gender { get; init; }
    
    /// <summary>
    /// Date of birth
    /// </summary>
    public DateTime? DateOfBirth { get; init; }
    
    /// <summary>
    /// Address
    /// </summary>
    public string? Address { get; init; }
    
    /// <summary>
    /// Role assignment (Doctor, Nurse, Staff, etc.)
    /// </summary>
    public string Role { get; init; } = "Staff";
    
    /// <summary>
    /// Department or specialization
    /// </summary>
    public string? Department { get; init; }
    
    /// <summary>
    /// License number (for doctors/nurses)
    /// </summary>
    public string? LicenseNumber { get; init; }
    
    /// <summary>
    /// Emergency contact information
    /// </summary>
    public string? EmergencyContact { get; init; }
} 