using FinTrack.Core.Enums;

namespace FinTrack.Core.Entities.ValueObjects;

/// <summary>
/// Value object containing synchronization metadata for entities
/// </summary>
public record SyncMetadata
{
    /// <summary>
    /// Current synchronization status
    /// </summary>
    public SyncStatus Status { get; init; }
    
    /// <summary>
    /// Unique identifier for synchronization across devices
    /// </summary>
    public string SyncId { get; init; }
    
    /// <summary>
    /// Timestamp of the last successful synchronization
    /// </summary>
    public DateTime? LastSyncAt { get; init; }
    
    /// <summary>
    /// Version number for optimistic concurrency control
    /// </summary>
    public long Version { get; init; }
    
    /// <summary>
    /// Device identifier where the entity was last modified
    /// </summary>
    public string? LastModifiedBy { get; init; }
    
    /// <summary>
    /// Number of sync retry attempts
    /// </summary>
    public int RetryCount { get; init; }
    
    /// <summary>
    /// Timestamp of the last sync attempt
    /// </summary>
    public DateTime? LastSyncAttempt { get; init; }
    
    /// <summary>
    /// Error message from the last failed sync attempt
    /// </summary>
    public string? LastSyncError { get; init; }
    
    /// <summary>
    /// Constructor for SyncMetadata
    /// </summary>
    public SyncMetadata(
        SyncStatus status = SyncStatus.PendingCreate,
        string? syncId = null,
        DateTime? lastSyncAt = null,
        long version = 1,
        string? lastModifiedBy = null,
        int retryCount = 0,
        DateTime? lastSyncAttempt = null,
        string? lastSyncError = null)
    {
        Status = status;
        SyncId = syncId ?? Guid.NewGuid().ToString();
        LastSyncAt = lastSyncAt;
        Version = version;
        LastModifiedBy = lastModifiedBy;
        RetryCount = retryCount;
        LastSyncAttempt = lastSyncAttempt;
        LastSyncError = lastSyncError;
    }
    
    /// <summary>
    /// Creates initial sync metadata for a new entity
    /// </summary>
    public static SyncMetadata CreateNew(string? deviceId = null)
    {
        return new SyncMetadata(
            status: SyncStatus.PendingCreate,
            syncId: Guid.NewGuid().ToString(),
            version: 1,
            lastModifiedBy: deviceId
        );
    }
    
    /// <summary>
    /// Creates sync metadata for a synced entity
    /// </summary>
    public static SyncMetadata CreateSynced(string syncId, long version, string? deviceId = null)
    {
        return new SyncMetadata(
            status: SyncStatus.Synced,
            syncId: syncId,
            lastSyncAt: DateTime.UtcNow,
            version: version,
            lastModifiedBy: deviceId
        );
    }
    
    /// <summary>
    /// Marks the metadata as modified
    /// </summary>
    public SyncMetadata MarkAsModified(string? deviceId = null)
    {
        var newStatus = Status == SyncStatus.Synced ? SyncStatus.PendingUpdate : Status;
        
        return this with
        {
            Status = newStatus,
            Version = Version + 1,
            LastModifiedBy = deviceId
        };
    }
    
    /// <summary>
    /// Marks the metadata as deleted
    /// </summary>
    public SyncMetadata MarkAsDeleted(string? deviceId = null)
    {
        return this with
        {
            Status = SyncStatus.PendingDelete,
            Version = Version + 1,
            LastModifiedBy = deviceId
        };
    }
    
    /// <summary>
    /// Marks the metadata as synced
    /// </summary>
    public SyncMetadata MarkAsSynced()
    {
        return this with
        {
            Status = SyncStatus.Synced,
            LastSyncAt = DateTime.UtcNow,
            RetryCount = 0,
            LastSyncError = null
        };
    }
    
    /// <summary>
    /// Marks the metadata as having a sync conflict
    /// </summary>
    public SyncMetadata MarkAsConflicted(string? error = null)
    {
        return this with
        {
            Status = SyncStatus.Conflict,
            LastSyncAttempt = DateTime.UtcNow,
            LastSyncError = error
        };
    }
    
    /// <summary>
    /// Marks the metadata as having failed to sync
    /// </summary>
    public SyncMetadata MarkAsSyncFailed(string? error = null)
    {
        return this with
        {
            Status = SyncStatus.SyncFailed,
            RetryCount = RetryCount + 1,
            LastSyncAttempt = DateTime.UtcNow,
            LastSyncError = error
        };
    }
    
    /// <summary>
    /// Checks if the entity needs to be synced
    /// </summary>
    public bool NeedsSync => Status is SyncStatus.PendingCreate or SyncStatus.PendingUpdate or SyncStatus.PendingDelete;
    
    /// <summary>
    /// Checks if the entity has sync issues
    /// </summary>
    public bool HasSyncIssues => Status is SyncStatus.SyncFailed or SyncStatus.Conflict;
    
    /// <summary>
    /// Checks if the entity is fully synced
    /// </summary>
    public bool IsSynced => Status == SyncStatus.Synced;
}