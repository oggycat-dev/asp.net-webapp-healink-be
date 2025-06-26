using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using Healink.Application.Common.Models;

namespace Healink.API.Middlewares;

/// <summary>
/// Base class for role-based access filters
/// </summary>
public abstract class BaseRoleAccessFilter : IActionFilter
{
    protected abstract string[] AllowedRoles { get; }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var user = context.HttpContext.User;
        
        if (!user.Identity?.IsAuthenticated == true)
        {
            context.Result = new UnauthorizedObjectResult(
                Result.Failure("Authentication required", ErrorCode.Unauthorized));
            return;
        }

        var userRoles = user.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        if (!userRoles.Any(role => AllowedRoles.Contains(role, StringComparer.OrdinalIgnoreCase)))
        {
            context.Result = new ForbidResult();
            return;
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // No implementation needed
    }
}

/// <summary>
/// Filter for User role access
/// </summary>
public class UserRoleAccessFilter : BaseRoleAccessFilter
{
    protected override string[] AllowedRoles => new[] { "User", "Staff", "Admin" };
}

/// <summary>
/// Filter for Staff role access
/// </summary>
public class StaffRoleAccessFilter : BaseRoleAccessFilter
{
    protected override string[] AllowedRoles => new[] { "Staff", "Admin" };
}

/// <summary>
/// Filter for Admin role access
/// </summary>
public class AdminRoleAccessFilter : BaseRoleAccessFilter
{
    protected override string[] AllowedRoles => new[] { "Admin" };
}

/// <summary>
/// Attribute for User role authorization
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireUserRoleAttribute : ServiceFilterAttribute
{
    public RequireUserRoleAttribute() : base(typeof(UserRoleAccessFilter))
    {
    }
}

/// <summary>
/// Attribute for Staff role authorization
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireStaffRoleAttribute : ServiceFilterAttribute
{
    public RequireStaffRoleAttribute() : base(typeof(StaffRoleAccessFilter))
    {
    }
}

/// <summary>
/// Attribute for Admin role authorization
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireAdminRoleAttribute : ServiceFilterAttribute
{
    public RequireAdminRoleAttribute() : base(typeof(AdminRoleAccessFilter))
    {
    }
} 