using FinTrack.Core.Entities;
using FinTrack.Core.Enums;
using FinTrack.Core.Exceptions;
using FinTrack.Core.Interfaces;
using FinTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace FinTrack.Infrastructure.Repositories;

/// <summary>
/// Base repository implementation providing generic CRUD operations for all entities
/// </summary>
/// <typeparam name="T">Entity type that inherits from BaseEntity</typeparam>
public class BaseRepository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly FinTrackDbContext _context;
    protected readonly DbSet<T> _dbSet;
    protected readonly ILogger<BaseRepository<T>> _logger;

    /// <summary>
    /// Constructor for BaseRepository
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="logger">Logger instance</param>
    public BaseRepository(FinTrackDbContext context, ILogger<BaseRepository<T>> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dbSet = _context.Set<T>();
    }

    #region Basic CRUD Operations

    /// <summary>
    /// Gets an entity by its ID
    /// </summary>
    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting {EntityType} with ID {Id}", typeof(T).Name, id);
            
            var entity = await _dbSet
                .Where(e => e.Id == id && !e.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);
                
            if (entity == null)
            {
                _logger.LogDebug("{EntityType} with ID {Id} not found", typeof(T).Name, id);
            }
            
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting {EntityType} with ID {Id}", typeof(T).Name, id);
            throw;
        }
    }

    /// <summary>
    /// Gets an entity by its sync ID
    /// </summary>
    public virtual async Task<T?> GetBySyncIdAsync(string syncId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting {EntityType} with Sync ID {SyncId}", typeof(T).Name, syncId);
            
            var entity = await _dbSet
                .Where(e => e.SyncId == syncId && !e.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);
                
            if (entity == null)
            {
                _logger.LogDebug("{EntityType} with Sync ID {SyncId} not found", typeof(T).Name, syncId);
            }
            
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting {EntityType} with Sync ID {SyncId}", typeof(T).Name, syncId);
            throw;
        }
    }

    /// <summary>
    /// Gets all entities (excluding soft deleted)
    /// </summary>
    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting all {EntityType} entities", typeof(T).Name);
            
            var entities = await _dbSet
                .Where(e => !e.IsDeleted)
                .ToListAsync(cancellationToken);
                
            _logger.LogDebug("Retrieved {Count} {EntityType} entities", entities.Count, typeof(T).Name);
            return entities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all {EntityType} entities", typeof(T).Name);
            throw;
        }
    }

    /// <summary>
    /// Gets entities that match the specified predicate
    /// </summary>
    public virtual async Task<IEnumerable<T>> GetWhereAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting {EntityType} entities with predicate", typeof(T).Name);
            
            var entities = await _dbSet
                .Where(e => !e.IsDeleted)
                .Where(predicate)
                .ToListAsync(cancellationToken);
                
            _logger.LogDebug("Retrieved {Count} {EntityType} entities matching predicate", entities.Count, typeof(T).Name);
            return entities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting {EntityType} entities with predicate", typeof(T).Name);
            throw;
        }
    }

    /// <summary>
    /// Gets a single entity that matches the specified predicate
    /// </summary>
    public virtual async Task<T?> GetSingleAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting single {EntityType} entity with predicate", typeof(T).Name);
            
            var entity = await _dbSet
                .Where(e => !e.IsDeleted)
                .Where(predicate)
                .FirstOrDefaultAsync(cancellationToken);
                
            if (entity == null)
            {
                _logger.LogDebug("No {EntityType} entity found matching predicate", typeof(T).Name);
            }
            
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting single {EntityType} entity with predicate", typeof(T).Name);
            throw;
        }
    }

    /// <summary>
    /// Adds a new entity
    /// </summary>
    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        try
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _logger.LogDebug("Adding new {EntityType} entity", typeof(T).Name);
            
            // Ensure sync ID is set
            if (string.IsNullOrEmpty(entity.SyncId))
            {
                entity.SyncId = Guid.NewGuid().ToString();
            }
            
            // Set initial sync status
            entity.SyncStatus = SyncStatus.PendingCreate;
            
            var addedEntity = _dbSet.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogDebug("Added {EntityType} entity with ID {Id}", typeof(T).Name, addedEntity.Entity.Id);
            return addedEntity.Entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding {EntityType} entity", typeof(T).Name);
            throw;
        }
    }

    /// <summary>
    /// Adds multiple entities
    /// </summary>
    public virtual async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        try
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            var entityList = entities.ToList();
            if (!entityList.Any())
                return entityList;

            _logger.LogDebug("Adding {Count} {EntityType} entities", entityList.Count, typeof(T).Name);
            
            // Ensure sync IDs are set and sync status is correct
            foreach (var entity in entityList)
            {
                if (string.IsNullOrEmpty(entity.SyncId))
                {
                    entity.SyncId = Guid.NewGuid().ToString();
                }
                entity.SyncStatus = SyncStatus.PendingCreate;
            }
            
            _dbSet.AddRange(entityList);
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogDebug("Added {Count} {EntityType} entities", entityList.Count, typeof(T).Name);
            return entityList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding {Count} {EntityType} entities", entities?.Count() ?? 0, typeof(T).Name);
            throw;
        }
    }

    /// <summary>
    /// Updates an existing entity
    /// </summary>
    public virtual async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        try
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _logger.LogDebug("Updating {EntityType} entity with ID {Id}", typeof(T).Name, entity.Id);
            
            // Check if entity exists
            var existingEntity = await _dbSet
                .Where(e => e.Id == entity.Id && !e.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);
                
            if (existingEntity == null)
            {
                throw new EntityNotFoundException(typeof(T).Name, entity.Id);
            }
            
            // Check for concurrency conflicts
            if (existingEntity.Version != entity.Version)
            {
                throw new ConcurrencyException(typeof(T).Name, entity.Id, 
                    BitConverter.GetBytes(entity.Version), BitConverter.GetBytes(existingEntity.Version));
            }
            
            // Update the entity
            _context.Entry(existingEntity).CurrentValues.SetValues(entity);
            
            // Mark the entity as modified
            _context.Entry(existingEntity).State = EntityState.Modified;
            
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogDebug("Updated {EntityType} entity with ID {Id}", typeof(T).Name, entity.Id);
            return existingEntity;
        }
        catch (Exception ex) when (!(ex is EntityNotFoundException || ex is ConcurrencyException))
        {
            _logger.LogError(ex, "Error updating {EntityType} entity with ID {Id}", typeof(T).Name, entity.Id);
            throw;
        }
    }

    /// <summary>
    /// Updates multiple entities
    /// </summary>
    public virtual async Task<IEnumerable<T>> UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        try
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            var entityList = entities.ToList();
            if (!entityList.Any())
                return entityList;

            _logger.LogDebug("Updating {Count} {EntityType} entities", entityList.Count, typeof(T).Name);
            
            var updatedEntities = new List<T>();
            
            foreach (var entity in entityList)
            {
                var existingEntity = await _dbSet
                    .Where(e => e.Id == entity.Id && !e.IsDeleted)
                    .FirstOrDefaultAsync(cancellationToken);
                    
                if (existingEntity == null)
                {
                    throw new EntityNotFoundException(typeof(T).Name, entity.Id);
                }
                
                // Check for concurrency conflicts
                if (existingEntity.Version != entity.Version)
                {
                    throw new ConcurrencyException(typeof(T).Name, entity.Id, 
                        BitConverter.GetBytes(entity.Version), BitConverter.GetBytes(existingEntity.Version));
                }
                
                _context.Entry(existingEntity).CurrentValues.SetValues(entity);
                
                // Don't call MarkAsModified here - let UpdateAuditFields handle it
                _context.Entry(existingEntity).State = EntityState.Modified;
                updatedEntities.Add(existingEntity);
            }
            
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogDebug("Updated {Count} {EntityType} entities", entityList.Count, typeof(T).Name);
            return updatedEntities;
        }
        catch (Exception ex) when (!(ex is EntityNotFoundException || ex is ConcurrencyException))
        {
            _logger.LogError(ex, "Error updating {Count} {EntityType} entities", entities?.Count() ?? 0, typeof(T).Name);
            throw;
        }
    }

    /// <summary>
    /// Soft deletes an entity by ID
    /// </summary>
    public virtual async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Soft deleting {EntityType} entity with ID {Id}", typeof(T).Name, id);
            
            var entity = await _dbSet
                .Where(e => e.Id == id && !e.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);
                
            if (entity == null)
            {
                _logger.LogDebug("{EntityType} entity with ID {Id} not found for deletion", typeof(T).Name, id);
                return false;
            }
            
            entity.MarkAsDeleted();
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogDebug("Soft deleted {EntityType} entity with ID {Id}", typeof(T).Name, id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error soft deleting {EntityType} entity with ID {Id}", typeof(T).Name, id);
            throw;
        }
    }

    /// <summary>
    /// Soft deletes an entity
    /// </summary>
    public virtual async Task<bool> DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        try
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return await DeleteAsync(entity.Id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error soft deleting {EntityType} entity", typeof(T).Name);
            throw;
        }
    }

    /// <summary>
    /// Hard deletes an entity by ID (permanently removes from database)
    /// </summary>
    public virtual async Task<bool> HardDeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Hard deleting {EntityType} entity with ID {Id}", typeof(T).Name, id);
            
            var entity = await _dbSet
                .IgnoreQueryFilters()
                .Where(e => e.Id == id)
                .FirstOrDefaultAsync(cancellationToken);
                
            if (entity == null)
            {
                _logger.LogDebug("{EntityType} entity with ID {Id} not found for hard deletion", typeof(T).Name, id);
                return false;
            }
            
            // Mark entity for hard delete to bypass soft delete logic
            entity.SyncStatus = SyncStatus.HardDelete;
            _context.Entry(entity).Property(nameof(BaseEntity.SyncStatus)).IsModified = true;
            
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogDebug("Hard deleted {EntityType} entity with ID {Id}", typeof(T).Name, id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error hard deleting {EntityType} entity with ID {Id}", typeof(T).Name, id);
            throw;
        }
    }

    #endregion

    #region Query Operations

    /// <summary>
    /// Counts entities that match the specified predicate
    /// </summary>
    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Counting {EntityType} entities", typeof(T).Name);
            
            var query = _dbSet.Where(e => !e.IsDeleted);
            
            if (predicate != null)
            {
                query = query.Where(predicate);
            }
            
            var count = await query.CountAsync(cancellationToken);
            
            _logger.LogDebug("Counted {Count} {EntityType} entities", count, typeof(T).Name);
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error counting {EntityType} entities", typeof(T).Name);
            throw;
        }
    }

    /// <summary>
    /// Checks if any entity matches the specified predicate
    /// </summary>
    public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Checking if any {EntityType} entities match predicate", typeof(T).Name);
            
            var exists = await _dbSet
                .Where(e => !e.IsDeleted)
                .Where(predicate)
                .AnyAsync(cancellationToken);
                
            _logger.LogDebug("Any {EntityType} entities match predicate: {Exists}", typeof(T).Name, exists);
            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if any {EntityType} entities match predicate", typeof(T).Name);
            throw;
        }
    }

    /// <summary>
    /// Gets entities with pagination
    /// </summary>
    public virtual async Task<IEnumerable<T>> GetPagedAsync(int skip, int take, 
        Expression<Func<T, bool>>? predicate = null,
        Expression<Func<T, object>>? orderBy = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (skip < 0)
                throw new ArgumentException("Skip must be non-negative", nameof(skip));
            if (take <= 0)
                throw new ArgumentException("Take must be positive", nameof(take));

            _logger.LogDebug("Getting paged {EntityType} entities (skip: {Skip}, take: {Take})", typeof(T).Name, skip, take);
            
            var query = _dbSet.Where(e => !e.IsDeleted);
            
            if (predicate != null)
            {
                query = query.Where(predicate);
            }
            
            if (orderBy != null)
            {
                query = query.OrderBy(orderBy);
            }
            else
            {
                // Default ordering by ID
                query = query.OrderBy(e => e.Id);
            }
            
            var entities = await query
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
                
            _logger.LogDebug("Retrieved {Count} paged {EntityType} entities", entities.Count, typeof(T).Name);
            return entities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paged {EntityType} entities", typeof(T).Name);
            throw;
        }
    }

    #endregion

    #region Sync-related Operations

    /// <summary>
    /// Gets entities that need to be synchronized
    /// </summary>
    public virtual async Task<IEnumerable<T>> GetPendingSyncAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting {EntityType} entities pending sync", typeof(T).Name);
            
            var entities = await _dbSet
                .IgnoreQueryFilters() // Include soft deleted entities for sync operations
                .Where(e => e.SyncStatus == SyncStatus.PendingCreate ||
                           e.SyncStatus == SyncStatus.PendingUpdate ||
                           e.SyncStatus == SyncStatus.PendingDelete)
                .ToListAsync(cancellationToken);
                
            _logger.LogDebug("Retrieved {Count} {EntityType} entities pending sync", entities.Count, typeof(T).Name);
            return entities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting {EntityType} entities pending sync", typeof(T).Name);
            throw;
        }
    }

    /// <summary>
    /// Gets entities with specific sync status
    /// </summary>
    public virtual async Task<IEnumerable<T>> GetBySyncStatusAsync(SyncStatus syncStatus, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting {EntityType} entities with sync status {SyncStatus}", typeof(T).Name, syncStatus);
            
            var entities = await _dbSet
                .IgnoreQueryFilters() // Include soft deleted entities for sync operations
                .Where(e => e.SyncStatus == syncStatus)
                .ToListAsync(cancellationToken);
                
            _logger.LogDebug("Retrieved {Count} {EntityType} entities with sync status {SyncStatus}", entities.Count, typeof(T).Name, syncStatus);
            return entities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting {EntityType} entities with sync status {SyncStatus}", typeof(T).Name, syncStatus);
            throw;
        }
    }

    /// <summary>
    /// Gets entities modified after a specific timestamp
    /// </summary>
    public virtual async Task<IEnumerable<T>> GetModifiedAfterAsync(DateTime timestamp, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting {EntityType} entities modified after {Timestamp}", typeof(T).Name, timestamp);
            
            var entities = await _dbSet
                .IgnoreQueryFilters() // Include soft deleted entities for sync operations
                .Where(e => e.UpdatedAt > timestamp)
                .ToListAsync(cancellationToken);
                
            _logger.LogDebug("Retrieved {Count} {EntityType} entities modified after {Timestamp}", entities.Count, typeof(T).Name, timestamp);
            return entities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting {EntityType} entities modified after {Timestamp}", typeof(T).Name, timestamp);
            throw;
        }
    }

    /// <summary>
    /// Marks entities as synced
    /// </summary>
    public virtual async Task<int> MarkAsSyncedAsync(IEnumerable<string> syncIds, CancellationToken cancellationToken = default)
    {
        try
        {
            if (syncIds == null)
                throw new ArgumentNullException(nameof(syncIds));

            var syncIdList = syncIds.ToList();
            if (!syncIdList.Any())
                return 0;

            _logger.LogDebug("Marking {Count} {EntityType} entities as synced", syncIdList.Count, typeof(T).Name);
            
            var entities = await _dbSet
                .IgnoreQueryFilters() // Include soft deleted entities for sync operations
                .Where(e => syncIdList.Contains(e.SyncId))
                .ToListAsync(cancellationToken);
                
            foreach (var entity in entities)
            {
                entity.MarkAsSynced();
                // Mark sync status as modified to prevent override in UpdateAuditFields
                _context.Entry(entity).Property(nameof(BaseEntity.SyncStatus)).IsModified = true;
            }
            
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogDebug("Marked {Count} {EntityType} entities as synced", entities.Count, typeof(T).Name);
            return entities.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking {EntityType} entities as synced", typeof(T).Name);
            throw;
        }
    }

    /// <summary>
    /// Marks entities as having sync conflicts
    /// </summary>
    public virtual async Task<int> MarkAsConflictedAsync(IEnumerable<string> syncIds, CancellationToken cancellationToken = default)
    {
        try
        {
            if (syncIds == null)
                throw new ArgumentNullException(nameof(syncIds));

            var syncIdList = syncIds.ToList();
            if (!syncIdList.Any())
                return 0;

            _logger.LogDebug("Marking {Count} {EntityType} entities as conflicted", syncIdList.Count, typeof(T).Name);
            
            var entities = await _dbSet
                .IgnoreQueryFilters() // Include soft deleted entities for sync operations
                .Where(e => syncIdList.Contains(e.SyncId))
                .ToListAsync(cancellationToken);
                
            foreach (var entity in entities)
            {
                entity.MarkAsConflicted();
                // Mark sync status as modified to prevent override in UpdateAuditFields
                _context.Entry(entity).Property(nameof(BaseEntity.SyncStatus)).IsModified = true;
            }
            
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogDebug("Marked {Count} {EntityType} entities as conflicted", entities.Count, typeof(T).Name);
            return entities.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking {EntityType} entities as conflicted", typeof(T).Name);
            throw;
        }
    }

    #endregion

    #region Save Operations

    /// <summary>
    /// Saves all changes to the database
    /// </summary>
    public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Saving changes to database");
            
            var result = await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogDebug("Saved {Count} changes to database", result);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving changes to database");
            throw;
        }
    }

    #endregion
}