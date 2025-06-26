using System.ComponentModel.DataAnnotations;
using Healink.Domain.Enums;

namespace Healink.Domain.Commons;

/// <summary>
/// Base entity for all entities
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Primary key
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Entity status
    /// </summary>
    public EntityStatus Status { get; set; } = EntityStatus.Active;

    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Created by user ID
    /// </summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>
    /// Updated by user ID
    /// </summary>
    public Guid? UpdatedBy { get; set; }

    /// <summary>
    /// Soft delete flag
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Deletion timestamp
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Deleted by user ID
    /// </summary>
    public Guid? DeletedBy { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    protected BaseEntity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        Status = EntityStatus.Active;
        IsDeleted = false;
    }
} 