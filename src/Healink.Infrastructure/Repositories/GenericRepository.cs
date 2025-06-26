using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Healink.Application.Common.Interfaces.Repositories;
using Healink.Domain.Commons;
using Healink.Infrastructure.Persistence;

namespace Healink.Infrastructure.Repositories;

/// <summary>
/// Generic repository implementation
/// </summary>
public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : BaseEntity
{
    private readonly HealinkDbContext _context;
    private readonly DbSet<TEntity> _dbSet;

    public GenericRepository(HealinkDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<TEntity>();
    }

    // Interface method implementations required by IGenericRepository<T>
    
    /// <summary>
    /// Get entity by ID
    /// </summary>
    public async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbSet.FindAsync(new object[] { id }, cancellationToken);
        return entity;
    }

    /// <summary>
    /// Get all entities
    /// </summary>
    public async Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Find entities by condition
    /// </summary>
    public async Task<List<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get entities with pagination
    /// </summary>
    public async Task<List<TEntity>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Add entity
    /// </summary>
    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    /// <summary>
    /// Add range of entities
    /// </summary>
    public async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddRangeAsync(entities, cancellationToken);
    }

    /// <summary>
    /// Update entity
    /// </summary>
    public void Update(TEntity entity)
    {
        _dbSet.Update(entity);
    }

    /// <summary>
    /// Update range of entities
    /// </summary>
    public void UpdateRange(IEnumerable<TEntity> entities)
    {
        _dbSet.UpdateRange(entities);
    }

    /// <summary>
    /// Remove entity (soft delete)
    /// </summary>
    public void Remove(TEntity entity)
    {
        _dbSet.Remove(entity);
    }

    /// <summary>
    /// Remove range of entities (soft delete)
    /// </summary>
    public void RemoveRange(IEnumerable<TEntity> entities)
    {
        _dbSet.RemoveRange(entities);
    }

    /// <summary>
    /// Check if entity exists
    /// </summary>
    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(e => e.Id == id, cancellationToken);
    }

    /// <summary>
    /// Count entities
    /// </summary>
    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(cancellationToken);
    }

    /// <summary>
    /// Count entities by condition
    /// </summary>
    public async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(predicate, cancellationToken);
    }

    // Additional helper methods for backwards compatibility
    
    protected virtual object GetEntityId(TEntity entity)
    {
        var keyProperty = _context.Model.FindEntityType(typeof(TEntity))
            ?.FindPrimaryKey()?.Properties[0];

        if (keyProperty == null)
            throw new InvalidOperationException("Entity does not have a primary key");

        return keyProperty.GetGetter().GetClrValue(entity);
    }

    public virtual async Task<TEntity> AddAsync(TEntity entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var entry = await _dbSet.AddAsync(entity);
        return entry.Entity;
    }

    public virtual async Task DeleteAsync(TEntity entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var entityState = _context.Entry(entity).State;
        if (entityState == EntityState.Detached)
        {
            var id = GetEntityId(entity);
            var trackedEntity = await _dbSet.FindAsync(id);
            if (trackedEntity == null)
                throw new KeyNotFoundException($"Entity with id {id} not found");

            _dbSet.Remove(trackedEntity);
        }
        else
        {
            _dbSet.Remove(entity);
        }
    }

    public virtual IQueryable<TEntity> FindByCondition(Expression<Func<TEntity, bool>> expression)
    {
        return _dbSet.AsNoTracking().Where(expression);
    }

    public virtual IQueryable<TEntity> FindByCondition(
        Expression<Func<TEntity, bool>> expression,
        params Expression<Func<TEntity, object>>[] includes)
    {
        var query = _dbSet.AsNoTracking().Where(expression);

        if (includes?.Any() == true)
        {
            query = includes.Aggregate(query,
                (current, include) => current.Include(include));
        }

        return query;
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        return await _dbSet.AsNoTracking().ToListAsync();
    }

    public virtual async Task<TEntity?> GetByIdAsync(Guid id, params Expression<Func<TEntity, object>>[] includes)
    {
        return await GetByIdAsync((object)id, includes);
    }

    public virtual async Task<TEntity?> GetByIdAsync(object id, params Expression<Func<TEntity, object>>[] includes)
    {
        // If no includes, use FindAsync
        if (includes == null || !includes.Any())
        {
            return await _dbSet.FindAsync(id);
        }

        // If includes exist, use FirstOrDefaultAsync with includes
        var keyProperty = _context.Model.FindEntityType(typeof(TEntity))
            ?.FindPrimaryKey()?.Properties[0];

        if (keyProperty == null)
            throw new InvalidOperationException("Entity does not have a primary key");

        var parameter = Expression.Parameter(typeof(TEntity), "e");
        var property = Expression.Property(parameter, keyProperty.Name);
        var constant = Expression.Constant(id);
        var equality = Expression.Equal(property, Expression.Convert(constant, keyProperty.ClrType));
        var lambda = Expression.Lambda<Func<TEntity, bool>>(equality, parameter);

        return await GetFirstOrDefaultAsync(lambda, includes);
    }

    public virtual async Task<TEntity?> GetFirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        params Expression<Func<TEntity, object>>[] includes)
    {
        var query = _dbSet.AsQueryable();

        if (includes?.Any() == true)
        {
            query = includes.Aggregate(query,
                (current, include) => current.Include(include));
        }
        return await query.FirstOrDefaultAsync(predicate);
    }

    public virtual async Task<TEntity> UpdateAsync(TEntity entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var id = GetEntityId(entity);
        var existingEntity = await _dbSet.FindAsync(id);

        if (existingEntity == null)
            throw new KeyNotFoundException($"Entity with id {id} not found");

        _context.Entry(existingEntity).CurrentValues.SetValues(entity);
        return existingEntity;
    }

    public virtual async Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities)
    {
        await _dbSet.AddRangeAsync(entities);
        return entities;
    }

    public virtual async Task UpdateRangeAsync(IEnumerable<TEntity> entities)
    {
        foreach (var entity in entities)
        {
            await UpdateAsync(entity);
        }
    }

    public virtual async Task DeleteRangeAsync(IEnumerable<TEntity> entities)
    {
        foreach (var entity in entities)
        {
            await DeleteAsync(entity);
        }
    }

    public virtual async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }

    public virtual IQueryable<TEntity> FindByCondition<TKey>(
        Expression<Func<TEntity, bool>> expression,
        Expression<Func<TEntity, TKey>> orderBy,
        bool ascending = true,
        params Expression<Func<TEntity, object>>[] includes)
    {
        var query = _dbSet.AsNoTracking().Where(expression);

        if (includes?.Any() == true)
        {
            query = includes.Aggregate(query,
                (current, include) => current.Include(include));
        }

        return ascending
            ? query.OrderBy(orderBy)
            : query.OrderByDescending(orderBy);
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync<TKey>(Expression<Func<TEntity, TKey>> orderBy, bool ascending = true)
    {
        return ascending
            ? await _dbSet.AsNoTracking().OrderBy(orderBy).ToListAsync()
            : await _dbSet.AsNoTracking().OrderByDescending(orderBy).ToListAsync();
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(params Expression<Func<TEntity, object>>[] includes)
    {
        var query = _dbSet.AsNoTracking();

        if (includes?.Any() == true)
        {
            query = includes.Aggregate(query,
                (current, include) => current.Include(include));
        }

        return await query.ToListAsync();
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync<TKey>(
        Expression<Func<TEntity, TKey>> orderBy,
        bool ascending = true,
        params Expression<Func<TEntity, object>>[] includes)
    {
        var query = _dbSet.AsNoTracking();

        if (includes?.Any() == true)
        {
            query = includes.Aggregate(query,
                (current, include) => current.Include(include));
        }

        return ascending
            ? await query.OrderBy(orderBy).ToListAsync()
            : await query.OrderByDescending(orderBy).ToListAsync();
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllIncludeDeletedAsync()
    {
        return await _dbSet.IgnoreQueryFilters().ToListAsync();
    }

    public virtual async Task<TEntity?> GetByIdIncludeDeletedAsync(Guid id, params Expression<Func<TEntity, object>>[] includes)
    {
        if (includes == null || !includes.Any())
        {
            return await _dbSet.IgnoreQueryFilters().FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id);
        }

        var query = _dbSet.IgnoreQueryFilters().AsQueryable();
        query = includes.Aggregate(query, (current, include) => current.Include(include));

        return await query.FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id);
    }

    public virtual async Task<TEntity> SoftDeleteAsync(TEntity entity, string userId)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        if (entity is BaseEntity baseEntity)
        {
            baseEntity.IsDeleted = true;
            baseEntity.DeletedAt = DateTime.UtcNow;
            baseEntity.DeletedBy = Guid.Parse(userId);
            _context.Entry(entity).State = EntityState.Modified;
        }

        return entity;
    }

    public virtual async Task<TEntity> RestoreAsync(TEntity entity, string userId)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        if (entity is BaseEntity baseEntity)
        {
            baseEntity.IsDeleted = false;
            baseEntity.DeletedAt = null;
            baseEntity.DeletedBy = null;
            baseEntity.UpdatedAt = DateTime.UtcNow;
            baseEntity.UpdatedBy = Guid.Parse(userId);
            _context.Entry(entity).State = EntityState.Modified;
        }

        return entity;
    }

    public virtual async Task<IEnumerable<TEntity>> SoftDeleteRangeAsync(IEnumerable<TEntity> entities, string userId)
    {
        if (entities == null)
            throw new ArgumentNullException(nameof(entities));

        var result = new List<TEntity>();
        foreach (var entity in entities)
        {
            var softDeleted = await SoftDeleteAsync(entity, userId);
            result.Add(softDeleted);
        }
        return result;
    }

    public virtual async Task<int> SoftDeleteByConditionAsync(Expression<Func<TEntity, bool>> predicate, string userId)
    {
        var entities = await _dbSet.Where(predicate).ToListAsync();
        if (!entities.Any())
            return 0;

        foreach (var entity in entities)
        {
            if (entity is BaseEntity baseEntity)
            {
                baseEntity.IsDeleted = true;
                baseEntity.DeletedAt = DateTime.UtcNow;
                baseEntity.DeletedBy = Guid.Parse(userId);
                _context.Entry(entity).State = EntityState.Modified;
            }
        }

        return entities.Count;
    }

    public virtual async Task<IEnumerable<TEntity>> RestoreRangeAsync(IEnumerable<TEntity> entities, string userId)
    {
        if (entities == null)
            throw new ArgumentNullException(nameof(entities));

        var result = new List<TEntity>();
        foreach (var entity in entities)
        {
            var restored = await RestoreAsync(entity, userId);
            result.Add(restored);
        }
        return result;
    }

    public virtual async Task<IEnumerable<TEntity>> FindAsyncForUpdate(
        Expression<Func<TEntity, bool>> predicate,
        params Expression<Func<TEntity, object>>[] includes)
    {
        var query = _dbSet.AsQueryable().Where(predicate);

        if (includes?.Any() == true)
        {
            query = includes.Aggregate(query,
                (current, include) => current.Include(include));
        }

        return await query.ToListAsync();
    }

    public virtual IQueryable<TEntity> FindByConditionIncludeDeleted(Expression<Func<TEntity, bool>> expression)
    {
        return _dbSet.IgnoreQueryFilters().Where(expression);
    }

    public virtual async Task<(List<TEntity> Items, int TotalCount)> GetPagedAsync<TKey>(
        Expression<Func<TEntity, bool>>? predicate,
        Expression<Func<TEntity, TKey>> orderBy,
        bool ascending,
        int pageNumber,
        int pageSize,
        params Expression<Func<TEntity, object>>[] includes)
    {
        var query = _dbSet.AsNoTracking();

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        if (includes?.Any() == true)
        {
            query = includes.Aggregate(query,
                (current, include) => current.Include(include));
        }

        var totalCount = await query.CountAsync();

        query = ascending
            ? query.OrderBy(orderBy)
            : query.OrderByDescending(orderBy);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
} 