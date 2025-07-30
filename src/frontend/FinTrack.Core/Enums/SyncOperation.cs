namespace FinTrack.Core.Enums;

/// <summary>
/// Represents the type of synchronization operation
/// </summary>
public enum SyncOperation
{
    /// <summary>
    /// Create operation - entity needs to be created on the server
    /// </summary>
    Create = 0,
    
    /// <summary>
    /// Update operation - entity needs to be updated on the server
    /// </summary>
    Update = 1,
    
    /// <summary>
    /// Delete operation - entity needs to be deleted on the server
    /// </summary>
    Delete = 2,
    
    /// <summary>
    /// No operation - entity is already synced
    /// </summary>
    None = 3
}