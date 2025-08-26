using System.Data;
using Healink.Application.Common.Interfaces.Repositories;

namespace Healink.Application.Common.Interfaces;

/// <summary>
/// Unit of Work pattern interface for coordinating multiple database contexts
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// The main business context
    /// </summary>
    IHealinkDbContext BusinessContext { get; }

    /// <summary>
    /// The identity/application context
    /// </summary>
    IApplicationDbContext ApplicationContext { get; }

    /// <summary>
    /// The staff profile repository
    /// </summary>
    IStaffProfileRepository StaffProfileRepository { get; }

    /// <summary>
    /// The podcast repository
    /// </summary>
    IPodcastRepository PodcastRepository { get; }

    /// <summary>
    /// The podcast category repository
    /// </summary>
    IPodcastCategoryRepository PodcastCategoryRepository { get; }

    /// <summary>
    /// The user profile repository
    /// </summary>
    IUserProfileRepository UserProfileRepository { get; }

    /// <summary>
    /// The podcast play history repository
    /// </summary>
    IPodcastPlayHistoryRepository PodcastPlayHistoryRepository { get; }

    /// <summary>
    /// Begin a transaction
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Begin a transaction with specified isolation level
    /// </summary>
    Task BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Commit the current transaction
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Rollback the current transaction
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Save changes on business context
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Save changes on application context (Identity)
    /// </summary>
    Task<int> SaveApplicationChangesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Save changes on both contexts with transaction coordination
    /// </summary>
    Task<int> SaveAllChangesAsync(CancellationToken cancellationToken = default);
} 