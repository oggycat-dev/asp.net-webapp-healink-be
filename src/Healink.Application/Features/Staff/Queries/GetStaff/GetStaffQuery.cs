using MediatR;
using Healink.Application.Common.DTOs.Staff;
using Healink.Application.Common.Models;

namespace Healink.Application.Features.Staff.Queries.GetStaff;

/// <summary>
/// Query to get staff member by ID
/// </summary>
public record GetStaffQuery : IRequest<Result<StaffResponse>>
{
    /// <summary>
    /// Staff ID to retrieve
    /// </summary>
    public Guid StaffId { get; init; }
    
    public GetStaffQuery(Guid staffId)
    {
        StaffId = staffId;
    }
} 