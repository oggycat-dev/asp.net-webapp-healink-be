using Healink.Application.Common.DTOs.Staff;
using Healink.Application.Common.Models;
using MediatR;

namespace Healink.Application.Features.Staff.Commands.CreateStaffProfile;

/// <summary>
/// Command to create a new staff profile
/// </summary>
public record CreateStaffProfileCommand(CreateStaffProfileDto Request) : IRequest<Result<StaffProfileDto>>; 