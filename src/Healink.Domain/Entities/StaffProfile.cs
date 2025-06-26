using Healink.Domain.Commons;
using Healink.Domain.Entities.Identity;
using Healink.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Healink.Domain.Entities;

/// <summary>
/// Staff profile entity for healthcare workers
/// </summary>
public class StaffProfile : BaseEntity
{
    /// <summary>
    /// First name of the staff member
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Last name of the staff member
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Full name (computed property)
    /// </summary>
    public string FullName 
    { 
        get => $"{FirstName} {LastName}".Trim();
        set { /* Allow setting for mapping purposes, but ignore */ }
    }

    /// <summary>
    /// Email address
    /// </summary>
    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Phone number
    /// </summary>
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Date of birth
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// Gender of the staff member
    /// </summary>
    public Gender Gender { get; set; }

    /// <summary>
    /// Physical address
    /// </summary>
    [MaxLength(500)]
    public string? Address { get; set; }

    /// <summary>
    /// Department where the staff works
    /// </summary>
    [MaxLength(100)]
    public string? Department { get; set; }

    /// <summary>
    /// Job position/title
    /// </summary>
    [MaxLength(100)]
    public string? Position { get; set; }

    /// <summary>
    /// Professional license number (for doctors, nurses, etc.)
    /// </summary>
    [MaxLength(50)]
    public string? LicenseNumber { get; set; }

    /// <summary>
    /// Emergency contact information
    /// </summary>
    [MaxLength(200)]
    public string? EmergencyContact { get; set; }

    /// <summary>
    /// Date when the staff joined
    /// </summary>
    public DateTime JoinDate { get; set; }

    /// <summary>
    /// Related AppUser ID for authentication
    /// </summary>
    [Required]
    public string AppUserId { get; set; } = string.Empty;

    /// <summary>
    /// Alias for AppUserId to maintain compatibility
    /// </summary>
    public string IdentityUserId 
    { 
        get => AppUserId; 
        set => AppUserId = value; 
    }

    /// <summary>
    /// Navigation property to AppUser
    /// </summary>
    public virtual AppUser? AppUser { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public StaffProfile()
    {
        JoinDate = DateTime.UtcNow;
    }
} 