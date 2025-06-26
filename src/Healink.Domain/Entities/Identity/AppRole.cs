using Microsoft.AspNetCore.Identity;

namespace Healink.Domain.Entities.Identity;

/// <summary>
/// Application role entity
/// </summary>
public class AppRole : IdentityRole
{
    /// <summary>
    /// Role description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Indicates if this is a system role that cannot be deleted
    /// </summary>
    public bool IsSystemRole { get; set; }

    /// <summary>
    /// Role display name
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Role permissions (JSON string)
    /// </summary>
    public string? Permissions { get; set; }

    /// <summary>
    /// Role creation date
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Role last update date
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Role created by user ID
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Role updated by user ID
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public AppRole()
    {
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Constructor with role name
    /// </summary>
    public AppRole(string roleName) : base(roleName)
    {
        CreatedAt = DateTime.UtcNow;
    }
} 