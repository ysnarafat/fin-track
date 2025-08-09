using FinTrack.Core.Entities;
using FinTrack.Core.Enums;
using System.Linq.Expressions;

namespace FinTrack.Core.Interfaces;

/// <summary>
/// Generic repository interface for basic CRUD operations
/// </summary>
/// <typeparam name="T">Entity type that inherits from BaseEntity</typeparam>
public interface IRepository<T> where T : BaseEntity
{
    // Basic CRUD operations
    
    /// <summary>
    /// Gets an entity by its ID
    /// </summary>
    /// <param name="id">Entity ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Entity if found, null otherwise</returns>
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets an entity by its sync ID
    /// </summary>
    /// <param name="syncId">Sync ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Entity if found, null otherwise</returns>
    Task<T?> GetBySyncIdAsync(string syncId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all entities (excluding soft deleted)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of entities</returns>
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets entities that match the specified predicate
    /// </summary>
    /// <param name="predicate">Filter predicate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of matching entities</returns>
    Task<IEnumerable<T>> GetWhereAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a single entity that matches the specified predicate
    /// </summary>
    /// <param name="predicate">Filter predicate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Entity if found, null otherwise</returns>
    Task<T?> GetSingleAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Adds a new entity
    /// </summary>
    /// <param name="entity">Entity to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Added entity with generated ID</returns>
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Adds multiple entities
    /// </summary>
    /// <param name="entities">Entities to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Added entities with generated IDs</returns>
    Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates an existing entity
    /// </summary>
    /// <param name="entity">Entity to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated entity</returns>
    Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates multiple entities
    /// </summary>
    /// <param name="entities">Entities to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated entities</returns>
    Task<IEnumerable<T>> UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Soft deletes an entity by ID
    /// </summary>
    /// <param name="id">Entity ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted, false if not found</returns>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Soft deletes an entity
    /// </summary>
    /// <param name="entity">Entity to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted, false otherwise</returns>
    Task<bool> DeleteAsync(T entity, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Hard deletes an entity by ID (permanently removes from database)
    /// </summary>
    /// <param name="id">Entity ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted, false if not found</returns>
    Task<bool> HardDeleteAsync(int id, CancellationToken cancellationToken = default);
    
    // Query operations
    
    /// <summary>
    /// Counts entities that match the specified predicate
    /// </summary>
    /// <param name="predicate">Filter predicate (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Count of matching entities</returns>
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if any entity matches the specified predicate
    /// </summary>
    /// <param name="predicate">Filter predicate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if any entity matches, false otherwise</returns>
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets entities with pagination
    /// </summary>
    /// <param name="skip">Number of entities to skip</param>
    /// <param name="take">Number of entities to take</param>
    /// <param name="predicate">Filter predicate (optional)</param>
    /// <param name="orderBy">Order by expression (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated collection of entities</returns>
    Task<IEnumerable<T>> GetPagedAsync(int skip, int take, 
        Expression<Func<T, bool>>? predicate = null,
        Expression<Func<T, object>>? orderBy = null,
        CancellationToken cancellationToken = default);
    
    // Sync-related operations
    
    /// <summary>
    /// Gets entities that need to be synchronized
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of entities pending sync</returns>
    Task<IEnumerable<T>> GetPendingSyncAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets entities with specific sync status
    /// </summary>
    /// <param name="syncStatus">Sync status to filter by</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of entities with the specified sync status</returns>
    Task<IEnumerable<T>> GetBySyncStatusAsync(SyncStatus syncStatus, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets entities modified after a specific timestamp
    /// </summary>
    /// <param name="timestamp">Timestamp to compare against</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of entities modified after the timestamp</returns>
    Task<IEnumerable<T>> GetModifiedAfterAsync(DateTime timestamp, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Marks entities as synced
    /// </summary>
    /// <param name="syncIds">Sync IDs of entities to mark as synced</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of entities updated</returns>
    Task<int> MarkAsSyncedAsync(IEnumerable<string> syncIds, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Marks entities as having sync conflicts
    /// </summary>
    /// <param name="syncIds">Sync IDs of entities to mark as conflicted</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of entities updated</returns>
    Task<int> MarkAsConflictedAsync(IEnumerable<string> syncIds, CancellationToken cancellationToken = default);
}