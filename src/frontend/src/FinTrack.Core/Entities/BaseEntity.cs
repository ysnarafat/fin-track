using FinTrack.Core.Enums;

namespace FinTrack.Core.Entities;

/// <summary>
/// Base entity class that provides common properties for all domain entities
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Unique identifier for the entity
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Timestamp when the entity was created
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Timestamp when the entity was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }
    
    /// <summary>
    /// Indicates if the entity has been soft deleted
    /// </summary>
    public bool IsDeleted { get; set; }
    
    /// <summary>
    /// Current synchronization status of the entity
    /// </summary>
    public SyncStatus SyncStatus { get; set; }
    
    /// <summary>
    /// Unique identifier used for synchronization across devices
    /// </summary>
    public string SyncId { get; set; } = string.Empty;
    
    /// <summary>
    /// Timestamp of the last successful synchronization
    /// </summary>
    public DateTime? LastSyncAt { get; set; }
    
    /// <summary>
    /// Version number for optimistic concurrency control
    /// </summary>
    public long Version { get; set; }
    
    /// <summary>
    /// Device identifier where the entity was last modified
    /// </summary>
    public string? LastModifiedBy { get; set; }
    
    /// <summary>
    /// Constructor that initializes default values
    /// </summary>
    protected BaseEntity()
    {
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        SyncStatus = SyncStatus.PendingCreate;
        SyncId = Guid.NewGuid().ToString();
        Version = 1;
    }
    
    /// <summary>
    /// Updates the entity's timestamp and sync status
    /// </summary>
    public virtual void MarkAsModified()
    {
        UpdatedAt = DateTime.UtcNow;
        Version++;
        
        // Only change sync status if it's currently synced
        if (SyncStatus == SyncStatus.Synced)
        {
            SyncStatus = SyncStatus.PendingUpdate;
        }
    }
    
    /// <summary>
    /// Marks the entity as deleted (soft delete)
    /// </summary>
    public virtual void MarkAsDeleted()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        Version++;
        SyncStatus = SyncStatus.PendingDelete;
    }
    
    /// <summary>
    /// Marks the entity as successfully synced
    /// </summary>
    public virtual void MarkAsSynced()
    {
        SyncStatus = SyncStatus.Synced;
        LastSyncAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Marks the entity as having a sync conflict
    /// </summary>
    public virtual void MarkAsConflicted()
    {
        SyncStatus = SyncStatus.Conflict;
    }
    
    /// <summary>
    /// Marks the entity as having failed to sync
    /// </summary>
    public virtual void MarkAsSyncFailed()
    {
        SyncStatus = SyncStatus.SyncFailed;
    }
}