namespace FinTrack.Core.Enums;

/// <summary>
/// Represents the synchronization status of an entity
/// </summary>
public enum SyncStatus
{
    /// <summary>
    /// Entity is synchronized with the remote server
    /// </summary>
    Synced = 0,
    
    /// <summary>
    /// Entity has been created locally and needs to be synced to the server
    /// </summary>
    PendingCreate = 1,
    
    /// <summary>
    /// Entity has been updated locally and needs to be synced to the server
    /// </summary>
    PendingUpdate = 2,
    
    /// <summary>
    /// Entity has been deleted locally and needs to be synced to the server
    /// </summary>
    PendingDelete = 3,
    
    /// <summary>
    /// Entity sync failed and needs to be retried
    /// </summary>
    SyncFailed = 4,
    
    /// <summary>
    /// Entity has a sync conflict that needs to be resolved
    /// </summary>
    Conflict = 5,
    
    /// <summary>
    /// Entity is marked for hard delete (internal use only)
    /// </summary>
    HardDelete = 6
}