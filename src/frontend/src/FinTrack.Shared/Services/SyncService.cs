using FinTrack.Core.Interfaces;
using FinTrack.Core.Enums;
using Microsoft.Extensions.Logging;

namespace FinTrack.Shared.Services;

/// <summary>
/// Implementation of sync service for managing data synchronization
/// </summary>
public class SyncService : ISyncService
{
    private readonly IConnectivityService _connectivityService;
    private readonly IFeatureFlagService _featureFlagService;
    private readonly ILogger<SyncService> _logger;
    private readonly Timer _syncTimer;
    private readonly List<PendingChange> _pendingChanges = new();
    private readonly List<SyncConflict> _conflicts = new();
    
    private SyncState _currentState = SyncState.Idle;
    private DateTime? _lastSyncTime;
    private bool _isStarted;

    public SyncService(IConnectivityService connectivityService, IFeatureFlagService featureFlagService, ILogger<SyncService> logger)
    {
        _connectivityService = connectivityService;
        _featureFlagService = featureFlagService;
        _logger = logger;
        
        // Create timer for periodic sync (every 5 minutes)
        _syncTimer = new Timer(OnTimerElapsed, null, Timeout.Infinite, Timeout.Infinite);
        
        // Subscribe to connectivity changes
        _connectivityService.ConnectivityChanged += OnConnectivityChanged;
    }

    public SyncState CurrentState
    {
        get => _currentState;
        private set
        {
            if (_currentState != value)
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
    }

    public int PendingChangesCount => _pendingChanges.Count;

    public DateTime? LastSyncTime => _lastSyncTime;

    public event EventHandler<SyncStateChangedEventArgs>? SyncStateChanged;
    public event EventHandler<int>? PendingChangesCountChanged;

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_isStarted)
            return Task.CompletedTask;

        // Check if offline sync feature is enabled
        if (!_featureFlagService.IsFeatureEnabled(FeatureFlags.OfflineSync))
        {
            _logger.LogInformation("Offline sync feature is disabled, sync service will not start");
            return Task.CompletedTask;
        }

        _isStarted = true;
        _connectivityService.StartMonitoring();
        
        // Start periodic sync timer only if automatic sync is enabled
        if (_featureFlagService.IsFeatureEnabled(FeatureFlags.AutomaticSync))
        {
            _syncTimer.Change(TimeSpan.Zero, TimeSpan.FromMinutes(5));
        }
        
        _logger.LogInformation("Sync service started");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (!_isStarted)
            return Task.CompletedTask;

        _isStarted = false;
        _connectivityService.StopMonitoring();
        _syncTimer.Change(Timeout.Infinite, Timeout.Infinite);
        
        CurrentState = SyncState.Idle;
        _logger.LogInformation("Sync service stopped");
        return Task.CompletedTask;
    }

    public async Task SyncAsync(CancellationToken cancellationToken = default)
    {
        // Check if offline sync feature is enabled
        if (!_featureFlagService.IsFeatureEnabled(FeatureFlags.OfflineSync))
        {
            _logger.LogDebug("Offline sync feature is disabled, skipping sync");
            return;
        }

        if (CurrentState == SyncState.Syncing)
        {
            _logger.LogDebug("Sync already in progress, skipping");
            return;
        }

        if (!_connectivityService.IsConnected)
        {
            CurrentState = SyncState.Offline;
            _logger.LogDebug("No internet connection, sync skipped");
            return;
        }

        try
        {
            CurrentState = SyncState.Syncing;
            _logger.LogInformation("Starting sync operation");

            // Simulate sync operation
            await Task.Delay(2000, cancellationToken);

            // TODO: Implement actual sync logic with repositories
            // - Get pending changes from repositories
            // - Send changes to server
            // - Handle conflicts
            // - Update local data with server changes

            _lastSyncTime = DateTime.UtcNow;
            CurrentState = SyncState.Idle;
            
            _logger.LogInformation("Sync completed successfully");
        }
        catch (OperationCanceledException)
        {
            CurrentState = SyncState.Idle;
            _logger.LogInformation("Sync operation was cancelled");
        }
        catch (Exception ex)
        {
            CurrentState = SyncState.Error;
            _logger.LogError(ex, "Sync operation failed");
        }
    }

    public Task<IEnumerable<PendingChange>> GetPendingChangesAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<PendingChange>>(_pendingChanges.ToList());
    }

    public Task<IEnumerable<SyncConflict>> GetConflictsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<SyncConflict>>(_conflicts.ToList());
    }

    public async Task ResolveConflictAsync(string conflictId, ConflictResolution resolution, CancellationToken cancellationToken = default)
    {
        var conflict = _conflicts.FirstOrDefault(c => c.Id == conflictId);
        if (conflict == null)
        {
            _logger.LogWarning("Conflict with ID {ConflictId} not found", conflictId);
            return;
        }

        _logger.LogInformation("Resolving conflict {ConflictId} with resolution {Resolution}", conflictId, resolution);

        // TODO: Implement actual conflict resolution logic
        switch (resolution)
        {
            case ConflictResolution.UseLocal:
                // Keep local version, mark as resolved
                break;
            case ConflictResolution.UseRemote:
                // Use remote version, update local data
                break;
            case ConflictResolution.Merge:
                // Implement merge logic
                break;
        }

        _conflicts.Remove(conflict);
        await Task.CompletedTask;
    }

    private async void OnTimerElapsed(object? state)
    {
        if (_isStarted && _connectivityService.IsConnected)
        {
            try
            {
                await SyncAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Periodic sync failed");
            }
        }
    }

    private void OnConnectivityChanged(object? sender, bool isConnected)
    {
        _logger.LogInformation("Connectivity changed: {IsConnected}", isConnected);
        
        if (isConnected && _isStarted)
        {
            // Trigger sync when connection is restored
            _ = Task.Run(async () =>
            {
                await Task.Delay(1000); // Small delay to ensure connection is stable
                await SyncAsync();
            });
        }
        else if (!isConnected)
        {
            CurrentState = SyncState.Offline;
        }
    }

    public void Dispose()
    {
        _syncTimer?.Dispose();
        _connectivityService.ConnectivityChanged -= OnConnectivityChanged;
    }
}