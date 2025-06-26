using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Healink.Application.Common.Interfaces;
using Healink.Application.Common.Models;
using Healink.Application.Common.DTOs.Staff;
using Healink.Application.Features.Staff.Commands.CreateStaffProfile;
using Healink.Application.Features.Staff.Queries.GetStaffProfiles;
using Swashbuckle.AspNetCore.Annotations;
using Healink.Domain.Enums;

namespace Healink.API.Controllers.Admin;

/// <summary>
/// Admin controller for staff management
/// </summary>
[ApiController]
[Route("api/admin/staff")]
[ApiExplorerSettings(GroupName = "admin")]
[Authorize(Roles = "Admin")]
[SwaggerTag("Admin - Quản lý nhân viên")]
public class StaffController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;

    public StaffController(IMediator mediator, IUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Get all staff members with filtering and pagination
    /// </summary>
    /// <remarks>
    /// This API supports filtering, sorting, and pagination for staff members.
    /// 
    /// Filter parameters:
    /// - searchTerm: Search by name, email, or employee ID (partial match)
    /// - department: Filter by department (partial match)
    /// - position: Filter by position (partial match)
    /// - status: Filter by status (0: Inactive, 1: Active, 2: Suspended)
    /// 
    /// Sort parameters:
    /// - sortBy: Field to sort by (firstname, lastname, email, department, position, hiredate, createdat)
    /// - isAscending: Sort ascending (true) or descending (false)
    /// 
    /// Pagination parameters:
    /// - pageNumber: Page number (default: 1)
    /// - pageSize: Number of records per page (default: 10)
    /// </remarks>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Get staff members",
        Description = "Get list of staff members with filtering, sorting, and pagination. Admin access required.",
        OperationId = "Admin_GetStaff"
    )]
    [SwaggerResponse(200, "Staff list retrieved successfully")]
    [SwaggerResponse(400, "Invalid request parameters")]
    [SwaggerResponse(401, "Unauthorized")]
    [SwaggerResponse(403, "Insufficient permissions (Admin required)")]
    public async Task<IActionResult> GetStaff(
        [FromQuery] string? searchTerm,
        [FromQuery] string? department,
        [FromQuery] string? position,
        [FromQuery] EntityStatus? status,
        [FromQuery] string? sortBy,
        [FromQuery] bool isAscending = true,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetStaffProfilesQuery
        {
            SearchTerm = searchTerm,
            Department = department,
            Position = position,
            Status = status,
            SortBy = sortBy,
            IsAscending = isAscending,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);
        
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get staff member by ID
    /// </summary>
    /// <param name="id">Staff member ID</param>
    [HttpGet("{id}")]
    [SwaggerOperation(
        Summary = "Get staff member by ID",
        Description = "Get detailed information of a specific staff member. Admin access required.",
        OperationId = "Admin_GetStaffById"
    )]
    [SwaggerResponse(200, "Staff member found")]
    [SwaggerResponse(404, "Staff member not found")]
    [SwaggerResponse(401, "Unauthorized")]
    [SwaggerResponse(403, "Insufficient permissions (Admin required)")]
    public async Task<IActionResult> GetStaffById(Guid id)
    {
        // Implementation would require a GetStaffByIdQuery
        // For now, return a placeholder
        return Ok(new { message = $"Get staff with ID: {id}" });
    }

    /// <summary>
    /// Create new staff member
    /// </summary>
    /// <remarks>
    /// Create a new staff member account with user credentials.
    /// 
    /// Sample request:
    /// 
    ///     POST /api/admin/staff
    ///     {
    ///        "firstName": "John",
    ///        "lastName": "Doe",
    ///        "email": "john.doe@healink.com",
    ///        "phoneNumber": "+1234567890",
    ///        "department": "IT",
    ///        "position": "Software Developer",
    ///        "employeeId": "EMP001",
    ///        "salary": 50000,
    ///        "password": "SecurePass123!"
    ///     }
    /// </remarks>
    /// <param name="request">Staff creation data</param>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Create new staff member",
        Description = "Create a new staff member with user account. Admin access required.",
        OperationId = "Admin_CreateStaff"
    )]
    [SwaggerResponse(201, "Staff member created successfully")]
    [SwaggerResponse(400, "Invalid data or validation errors")]
    [SwaggerResponse(409, "Email or Employee ID already exists")]
    [SwaggerResponse(401, "Unauthorized")]
    [SwaggerResponse(403, "Insufficient permissions (Admin required)")]
    public async Task<IActionResult> CreateStaff([FromBody] CreateStaffProfileDto request)
    {
        var command = new CreateStaffProfileCommand(request);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetStaffById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Update staff member information
    /// </summary>
    /// <param name="id">Staff member ID</param>
    /// <param name="request">Updated staff data</param>
    [HttpPut("{id}")]
    [SwaggerOperation(
        Summary = "Update staff member",
        Description = "Update staff member information. Admin access required.",
        OperationId = "Admin_UpdateStaff"
    )]
    [SwaggerResponse(200, "Staff member updated successfully")]
    [SwaggerResponse(400, "Invalid data or validation errors")]
    [SwaggerResponse(404, "Staff member not found")]
    [SwaggerResponse(401, "Unauthorized")]
    [SwaggerResponse(403, "Insufficient permissions (Admin required)")]
    public async Task<IActionResult> UpdateStaff(Guid id, [FromBody] UpdateStaffProfileDto request)
    {
        // Implementation would require an UpdateStaffCommand
        // For now, return a placeholder
        return Ok(new { message = $"Update staff with ID: {id}", data = request });
    }

    /// <summary>
    /// Delete (deactivate) staff member
    /// </summary>
    /// <param name="id">Staff member ID</param>
    [HttpDelete("{id}")]
    [SwaggerOperation(
        Summary = "Delete staff member",
        Description = "Soft delete (deactivate) a staff member. Admin access required.",
        OperationId = "Admin_DeleteStaff"
    )]
    [SwaggerResponse(200, "Staff member deleted successfully")]
    [SwaggerResponse(404, "Staff member not found")]
    [SwaggerResponse(401, "Unauthorized")]
    [SwaggerResponse(403, "Insufficient permissions (Admin required)")]
    public async Task<IActionResult> DeleteStaff(Guid id)
    {
        // Implementation would require a DeleteStaffCommand
        // For now, return a placeholder
        return Ok(new { message = $"Delete staff with ID: {id}" });
    }

    /// <summary>
    /// Search staff members
    /// </summary>
    /// <param name="query">Search query</param>
    [HttpGet("search")]
    [SwaggerOperation(
        Summary = "Search staff members",
        Description = "Search staff members by name, email, or employee ID. Admin access required.",
        OperationId = "Admin_SearchStaff"
    )]
    [SwaggerResponse(200, "Search results")]
    [SwaggerResponse(400, "Invalid search query")]
    [SwaggerResponse(401, "Unauthorized")]
    [SwaggerResponse(403, "Insufficient permissions (Admin required)")]
    public async Task<IActionResult> SearchStaff([FromQuery] string query)
    {
        var searchQuery = new GetStaffProfilesQuery
        {
            SearchTerm = query,
            PageSize = 50 // Return more results for search
        };

        var result = await _mediator.Send(searchQuery);
        
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
} 