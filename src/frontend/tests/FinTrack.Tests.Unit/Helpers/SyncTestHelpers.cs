using System;

namespace FinTrack.Tests.Unit.Helpers;

/// <summary>
/// Test helper utilities for sync functionality
/// </summary>
public static class SyncTestHelpers
{
    /// <summary>
    /// Creates a test sync state changed event args
    /// </summary>
    public static FinTrack.Core.Interfaces.SyncStateChangedEventArgs CreateSyncStateChangedEventArgs(
        FinTrack.Core.Interfaces.SyncState previousState, 
        FinTrack.Core.Interfaces.SyncState currentState,
        string? errorMessage = null)
    {
        return new FinTrack.Core.Interfaces.SyncStateChangedEventArgs
        {
            PreviousState = previousState,
            CurrentState = currentState,
            ErrorMessage = errorMessage
        };
    }

    /// <summary>
    /// Creates a test sync conflict
    /// </summary>
    public static FinTrack.Core.Interfaces.SyncConflict CreateSyncConflict(
        string id,
        string entityType,
        string entityId,
        string localData,
        string remoteData)
    {
        return new FinTrack.Core.Interfaces.SyncConflict
        {
            Id = id,
            EntityType = entityType,
            EntityId = entityId,
            LocalData = localData,
            RemoteData = remoteData,
            ConflictDetectedAt = DateTime.UtcNow
        };
    }
}
