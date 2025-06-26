using Microsoft.AspNetCore.Identity;

namespace Healink.Domain.Entities.Identity;

/// <summary>
/// Application user for identity management
/// </summary>
public class AppUser : IdentityUser
{
    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Last modification timestamp
    /// </summary>
    public DateTime? ModifiedAt { get; set; }
    
    /// <summary>
    /// Whether the user is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Reference to the entity ID in the business context
    /// </summary>
    public Guid? EntityId { get; set; }
    
    /// <summary>
    /// User's refresh token for JWT authentication
    /// </summary>
    public string? RefreshToken { get; set; }
    
    /// <summary>
    /// Expiry time of the refresh token
    /// </summary>
    public DateTime? RefreshTokenExpiryTime { get; set; }
    
    /// <summary>
    /// Navigation property to staff profile (for staff members)
    /// </summary>
    public virtual StaffProfile? StaffProfile { get; set; }
} 