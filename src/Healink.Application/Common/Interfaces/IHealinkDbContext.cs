using Microsoft.EntityFrameworkCore;
using Healink.Domain.Entities;

namespace Healink.Application.Common.Interfaces;

/// <summary>
/// Healink business database context interface
/// </summary>
public interface IHealinkDbContext
{
    /// <summary>
    /// Staff profiles DbSet
    /// </summary>
    DbSet<StaffProfile> StaffProfiles { get; set; }
    
    /// <summary>
    /// Save changes asynchronously
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
} 