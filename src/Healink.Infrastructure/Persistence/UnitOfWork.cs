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
    private IDbContextTransaction? _transaction;
    private readonly IStaffProfileRepository _staffProfileRepository;

    public UnitOfWork(
        HealinkDbContext businessContext,
        IStaffProfileRepository staffProfileRepository)
    {
        _businessContext = businessContext;
        _staffProfileRepository = staffProfileRepository;
    }

    public IHealinkDbContext BusinessContext => _businessContext;

    public IStaffProfileRepository StaffProfileRepository => _staffProfileRepository;

    /// <summary>
    /// Begin a transaction
    /// </summary>
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _businessContext.Database.BeginTransactionAsync(cancellationToken);
    }
    
    /// <summary>
    /// Begin a transaction with specified isolation level
    /// </summary>
    public async Task BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default)
    {
        _transaction = await _businessContext.Database.BeginTransactionAsync(isolationLevel, cancellationToken);
    }
    
    /// <summary>
    /// Commit the current transaction
    /// </summary>
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SaveChangesAsync(cancellationToken);
            
            if (_transaction != null)
            {
                await _transaction.CommitAsync(cancellationToken);
            }
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }
    
    /// <summary>
    /// Rollback the current transaction
    /// </summary>
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
    
    /// <summary>
    /// Save changes to business context
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _businessContext.SaveChangesAsync(cancellationToken);
    }
    
    /// <summary>
    /// Dispose the unit of work and transaction
    /// </summary>
    public void Dispose()
    {
        _transaction?.Dispose();
        GC.SuppressFinalize(this);
    }
} 