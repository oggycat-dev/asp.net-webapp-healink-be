using MediatR;
using Healink.Application.Common.DTOs.Staff;
using Healink.Application.Common.Models;
using Healink.Domain.Enums;

namespace Healink.Application.Features.Staff.Commands.UpdateProfile;

/// <summary>
/// Command to update staff profile
/// </summary>
public record UpdateProfileCommand : IRequest<Result<StaffProfileDto>>
{
    /// <summary>
    /// ID of the staff profile to update
    /// </summary>
    public Guid StaffId { get; init; }
    
    /// <summary>
    /// First name of the staff member
    /// </summary>
    public string? FirstName { get; init; }
    
    /// <summary>
    /// Last name of the staff member
    /// </summary>
    public string? LastName { get; init; }
    
    /// <summary>
    /// Phone number of the staff member
    /// </summary>
    public string? PhoneNumber { get; init; }
    
    /// <summary>
    /// Date of birth of the staff member
    /// </summary>
    public DateTime? DateOfBirth { get; init; }
    
    /// <summary>
    /// Gender of the staff member
    /// </summary>
    public Gender? Gender { get; init; }
    
    /// <summary>
    /// Physical address of the staff member
    /// </summary>
    public string? Address { get; init; }
    
    /// <summary>
    /// Department where the staff works
    /// </summary>
    public string? Department { get; init; }
    
    /// <summary>
    /// Job position/title of the staff member
    /// </summary>
    public string? Position { get; init; }
}
