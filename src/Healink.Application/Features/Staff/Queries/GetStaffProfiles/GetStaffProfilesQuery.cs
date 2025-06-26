using Healink.Application.Common.DTOs.Staff;
using Healink.Application.Common.Models;
using Healink.Domain.Enums;
using MediatR;

namespace Healink.Application.Features.Staff.Queries.GetStaffProfiles;

/// <summary>
/// Query to get staff profiles with filtering and pagination
/// </summary>
public class GetStaffProfilesQuery : IRequest<Result<List<StaffProfileDto>>>
{
    public string? SearchTerm { get; set; }
    public string? Department { get; set; }
    public string? Position { get; set; }
    public EntityStatus? Status { get; set; }
    public string? SortBy { get; set; }
    public bool IsAscending { get; set; } = true;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
} 