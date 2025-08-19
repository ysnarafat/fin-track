using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FinTrack.Core.Interfaces;

namespace FinTrack.Tests.Unit.Mocks;

/// <summary>
/// Mock implementation of ISyncService for testing
/// </summary>
public class MockSyncService : ISyncService
{
    private SyncState _currentState = SyncState.Idle;
    private int _pendingChangesCount = 0;
    private DateTime? _lastSyncTime = null;
    private readonly List<PendingChange> _pendingChanges = new();
    private readonly List<SyncConflict> _conflicts = new();

    public SyncState CurrentState
    {
        get => _currentState;
        set
        {
            var previousState = _currentState;
            _currentState = value;
            SyncStateChanged?.Invoke(this, new SyncStateChangedEventArgs 
            { 
                PreviousState = previousState, 
                CurrentState = value 
            });
        }
    }

    public int PendingChangesCount
    {
        get => _pendingChangesCount;
        set
        {
            _pendingChangesCount = value;
            PendingChangesCountChanged?.Invoke(this, value);
        }
    }

    public DateTime? LastSyncTime
    {
        get => _lastSyncTime;
        set => _lastSyncTime = value;
    }

    public event EventHandler<SyncStateChangedEventArgs>? SyncStateChanged;
    public event EventHandler<int>? PendingChangesCountChanged;

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        CurrentState = SyncState.Syncing;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        CurrentState = SyncState.Idle;
        return Task.CompletedTask;
    }

    public Task SyncAsync(CancellationToken cancellationToken = default)
    {
        CurrentState = SyncState.Syncing;
        // Simulate sync
        CurrentState = SyncState.Idle;
        LastSyncTime = DateTime.UtcNow;
        PendingChangesCount = 0;
        return Task.CompletedTask;
    }

    public Task<IEnumerable<PendingChange>> GetPendingChangesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<PendingChange>>(_pendingChanges);
    }

    public Task<IEnumerable<SyncConflict>> GetConflictsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<SyncConflict>>(_conflicts);
    }

    public Task ResolveConflictAsync(string conflictId, ConflictResolution resolution, CancellationToken cancellationToken = default)
    {
        var conflict = _conflicts.FirstOrDefault(c => c.Id == conflictId);
        if (conflict != null)
        {
            _conflicts.Remove(conflict);
        }
        return Task.CompletedTask;
    }

    public void AddPendingChange(PendingChange change)
    {
        _pendingChanges.Add(change);
        PendingChangesCount = _pendingChanges.Count;
    }

    public void AddConflict(SyncConflict conflict)
    {
        _conflicts.Add(conflict);
    }
}
