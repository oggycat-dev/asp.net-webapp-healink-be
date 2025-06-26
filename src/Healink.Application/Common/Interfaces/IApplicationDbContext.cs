using Microsoft.EntityFrameworkCore;
using Healink.Domain.Entities;

namespace Healink.Application.Common.Interfaces;

/// <summary>
/// Application database context interface
/// </summary>
public interface IApplicationDbContext
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