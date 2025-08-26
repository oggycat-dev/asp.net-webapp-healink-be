using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Healink.Application.Common.Interfaces;
using Healink.Application.Common.Interfaces.Repositories;

namespace Healink.Infrastructure.Persistence;

/// <summary>
/// Implementation of the Unit of Work pattern for coordinating multiple database contexts
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly HealinkDbContext _businessContext;
    private readonly ApplicationDbContext _applicationContext;
    private IDbContextTransaction? _businessTransaction;
    private IDbContextTransaction? _applicationTransaction;
    private readonly IStaffProfileRepository _staffProfileRepository;
    private readonly IPodcastRepository _podcastRepository;
    private readonly IPodcastCategoryRepository _podcastCategoryRepository;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IPodcastPlayHistoryRepository _podcastPlayHistoryRepository;

    public UnitOfWork(
        HealinkDbContext businessContext,
        ApplicationDbContext applicationContext,
        IStaffProfileRepository staffProfileRepository,
        IPodcastRepository podcastRepository,
        IPodcastCategoryRepository podcastCategoryRepository,
        IUserProfileRepository userProfileRepository,
        IPodcastPlayHistoryRepository podcastPlayHistoryRepository)
    {
        _businessContext = businessContext;
        _applicationContext = applicationContext;
        _staffProfileRepository = staffProfileRepository;
        _podcastRepository = podcastRepository;
        _podcastCategoryRepository = podcastCategoryRepository;
        _userProfileRepository = userProfileRepository;
        _podcastPlayHistoryRepository = podcastPlayHistoryRepository;
    }

    public IHealinkDbContext BusinessContext => _businessContext;
    public IApplicationDbContext ApplicationContext => _applicationContext;
    public IStaffProfileRepository StaffProfileRepository => _staffProfileRepository;
    public IPodcastRepository PodcastRepository => _podcastRepository;
    public IPodcastCategoryRepository PodcastCategoryRepository => _podcastCategoryRepository;
    public IUserProfileRepository UserProfileRepository => _userProfileRepository;
    public IPodcastPlayHistoryRepository PodcastPlayHistoryRepository => _podcastPlayHistoryRepository;

    /// <summary>
    /// Begin a transaction on both contexts
    /// </summary>
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        // Start transactions on both contexts
        _businessTransaction = await _businessContext.Database.BeginTransactionAsync(cancellationToken);
        _applicationTransaction = await _applicationContext.Database.BeginTransactionAsync(cancellationToken);
    }
    
    /// <summary>
    /// Begin a transaction with specified isolation level on both contexts
    /// </summary>
    public async Task BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
    {
        // Start transactions on both contexts with specified isolation level
        _businessTransaction = await _businessContext.Database.BeginTransactionAsync(isolationLevel, cancellationToken);
        _applicationTransaction = await _applicationContext.Database.BeginTransactionAsync(isolationLevel, cancellationToken);
    }
    
    /// <summary>
    /// Commit transactions on both contexts
    /// </summary>
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Save changes on both contexts
            await SaveAllChangesAsync(cancellationToken);
            
            // Commit both transactions
            if (_applicationTransaction != null)
            {
                await _applicationTransaction.CommitAsync(cancellationToken);
            }
            
            if (_businessTransaction != null)
            {
                await _businessTransaction.CommitAsync(cancellationToken);
            }
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            await DisposeTransactionsAsync();
        }
    }
    
    /// <summary>
    /// Rollback transactions on both contexts
    /// </summary>
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_applicationTransaction != null)
            {
                await _applicationTransaction.RollbackAsync(cancellationToken);
            }
        }
        catch
        {
            // Log but don't throw to allow business transaction rollback
        }
        
        try
        {
            if (_businessTransaction != null)
            {
                await _businessTransaction.RollbackAsync(cancellationToken);
            }
        }
        catch
        {
            // Log but continue cleanup
        }
        
        await DisposeTransactionsAsync();
    }
    
    /// <summary>
    /// Save changes to business context
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _businessContext.SaveChangesAsync(cancellationToken);
    }
    
    /// <summary>
    /// Save changes to application context (Identity)
    /// </summary>
    public async Task<int> SaveApplicationChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _applicationContext.SaveChangesAsync(cancellationToken);
    }
    
    /// <summary>
    /// Save changes on both contexts with coordination
    /// </summary>
    public async Task<int> SaveAllChangesAsync(CancellationToken cancellationToken = default)
    {
        // Save application context first (Identity data)
        var applicationChanges = await _applicationContext.SaveChangesAsync(cancellationToken);
        
        // Then save business context
        var businessChanges = await _businessContext.SaveChangesAsync(cancellationToken);
        
        return applicationChanges + businessChanges;
    }
    
    /// <summary>
    /// Dispose transactions
    /// </summary>
    private async Task DisposeTransactionsAsync()
    {
        if (_applicationTransaction != null)
        {
            await _applicationTransaction.DisposeAsync();
            _applicationTransaction = null;
        }
        
        if (_businessTransaction != null)
        {
            await _businessTransaction.DisposeAsync();
            _businessTransaction = null;
        }
    }
    
    /// <summary>
    /// Dispose the unit of work and transactions
    /// </summary>
    public void Dispose()
    {
        _applicationTransaction?.Dispose();
        _businessTransaction?.Dispose();
        GC.SuppressFinalize(this);
    }
} 