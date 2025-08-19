using FinTrack.Core.Enums;

namespace FinTrack.Core.Interfaces;

/// <summary>
/// Service for managing data synchronization
/// </summary>
public interface ISyncService
{
    /// <summary>
    /// Gets the current sync status
    /// </summary>
    SyncState CurrentState { get; }
    
    /// <summary>
    /// Gets the number of pending changes
    /// </summary>
    int PendingChangesCount { get; }
    
    /// <summary>
    /// Gets the last sync timestamp
    /// </summary>
    DateTime? LastSyncTime { get; }
    
    /// <summary>
    /// Event raised when sync state changes
    /// </summary>
    event EventHandler<SyncStateChangedEventArgs> SyncStateChanged;
    
    /// <summary>
    /// Event raised when pending changes count changes
    /// </summary>
    event EventHandler<int> PendingChangesCountChanged;
    
    /// <summary>
    /// Starts automatic synchronization
    /// </summary>
    Task StartAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Stops automatic synchronization
    /// </summary>
    Task StopAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Performs a manual sync
    /// </summary>
    Task SyncAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all pending changes
    /// </summary>
    Task<IEnumerable<PendingChange>> GetPendingChangesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets conflicts that need resolution
    /// </summary>
    Task<IEnumerable<SyncConflict>> GetConflictsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Resolves a sync conflict
    /// </summary>
    Task ResolveConflictAsync(string conflictId, ConflictResolution resolution, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents the current state of synchronization
/// </summary>
public enum SyncState
{
    Idle,
    Syncing,
    Error,
    Offline
}

/// <summary>
/// Event arguments for sync state changes
/// </summary>
public class SyncStateChangedEventArgs : EventArgs
{
    public SyncState PreviousState { get; set; }
    public SyncState CurrentState { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Represents a pending change that needs to be synchronized
/// </summary>
public class PendingChange
{
    public string Id { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public SyncOperation Operation { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Data { get; set; }
}

/// <summary>
/// Represents a synchronization conflict
/// </summary>
public class SyncConflict
{
    public string Id { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string LocalData { get; set; } = string.Empty;
    public string RemoteData { get; set; } = string.Empty;
    public DateTime ConflictDetectedAt { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// Conflict resolution options
/// </summary>
public enum ConflictResolution
{
    UseLocal,
    UseRemote,
    Merge
}