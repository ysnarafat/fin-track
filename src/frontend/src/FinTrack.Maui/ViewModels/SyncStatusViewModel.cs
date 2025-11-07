using System.ComponentModel;
using System.Runtime.CompilerServices;
using FinTrack.Core.Interfaces;

namespace FinTrack.Maui.ViewModels;

/// <summary>
/// ViewModel for sync status display in the UI
/// </summary>
public class SyncStatusViewModel : INotifyPropertyChanged
{
    private readonly ISyncService _syncService;
    private readonly IConnectivityService _connectivityService;
    private readonly IFeatureFlagService _featureFlagService;
    
    private bool _isOnline;
    private SyncState _syncState;
    private int _pendingChangesCount;
    private DateTime? _lastSyncTime;
    private string _statusText = "Ready";
    private string _statusIcon = "wifi";
    private Color _statusColor = Colors.Green;

    public SyncStatusViewModel(ISyncService syncService, IConnectivityService connectivityService, IFeatureFlagService featureFlagService)
    {
        _syncService = syncService;
        _connectivityService = connectivityService;
        _featureFlagService = featureFlagService;
        
        // Initialize state
        _isOnline = _connectivityService.IsConnected;
        _syncState = _syncService.CurrentState;
        _pendingChangesCount = _syncService.PendingChangesCount;
        _lastSyncTime = _syncService.LastSyncTime;
        
        // Subscribe to events
        _connectivityService.ConnectivityChanged += OnConnectivityChanged;
        _syncService.SyncStateChanged += OnSyncStateChanged;
        _syncService.PendingChangesCountChanged += OnPendingChangesCountChanged;
        
        UpdateStatus();
    }

    public bool IsOnline
    {
        get => _isOnline;
        private set => SetProperty(ref _isOnline, value);
    }

    public SyncState SyncState
    {
        get => _syncState;
        private set => SetProperty(ref _syncState, value);
    }

    public int PendingChangesCount
    {
        get => _pendingChangesCount;
        private set => SetProperty(ref _pendingChangesCount, value);
    }

    public DateTime? LastSyncTime
    {
        get => _lastSyncTime;
        private set => SetProperty(ref _lastSyncTime, value);
    }

    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    public string StatusIcon
    {
        get => _statusIcon;
        private set => SetProperty(ref _statusIcon, value);
    }

    public Color StatusColor
    {
        get => _statusColor;
        private set => SetProperty(ref _statusColor, value);
    }

    public string LastSyncText
    {
        get
        {
            if (LastSyncTime == null)
                return "Never synced";
            
            var timeSpan = DateTime.UtcNow - LastSyncTime.Value;
            if (timeSpan.TotalMinutes < 1)
                return "Just now";
            if (timeSpan.TotalHours < 1)
                return $"{(int)timeSpan.TotalMinutes}m ago";
            if (timeSpan.TotalDays < 1)
                return $"{(int)timeSpan.TotalHours}h ago";
            
            return LastSyncTime.Value.ToString("MMM dd, HH:mm");
        }
    }

    public bool HasPendingChanges => PendingChangesCount > 0;

    public string PendingChangesText => PendingChangesCount switch
    {
        0 => "All synced",
        1 => "1 pending change",
        _ => $"{PendingChangesCount} pending changes"
    };

    public bool IsSyncFeatureEnabled => _featureFlagService.IsFeatureEnabled(FeatureFlags.OfflineSync);
    
    public bool AreSyncIndicatorsEnabled => _featureFlagService.IsFeatureEnabled(FeatureFlags.SyncStatusIndicators);
    
    public bool IsConflictResolutionEnabled => _featureFlagService.IsFeatureEnabled(FeatureFlags.ConflictResolution);

    private void OnConnectivityChanged(object? sender, bool isConnected)
    {
        IsOnline = isConnected;
        UpdateStatus();
    }

    private void OnSyncStateChanged(object? sender, SyncStateChangedEventArgs e)
    {
        SyncState = e.CurrentState;
        if (e.CurrentState == SyncState.Idle && e.PreviousState == SyncState.Syncing)
        {
            LastSyncTime = DateTime.UtcNow;
        }
        UpdateStatus();
    }

    private void OnPendingChangesCountChanged(object? sender, int count)
    {
        PendingChangesCount = count;
        UpdateStatus();
    }

    private void UpdateStatus()
    {
        if (!IsOnline)
        {
            StatusText = "Offline";
            StatusIcon = "wifi_off";
            StatusColor = Colors.Orange;
        }
        else
        {
            switch (SyncState)
            {
                case SyncState.Idle:
                    StatusText = HasPendingChanges ? "Pending sync" : "Synced";
                    StatusIcon = HasPendingChanges ? "sync_problem" : "sync";
                    StatusColor = HasPendingChanges ? Colors.Orange : Colors.Green;
                    break;
                
                case SyncState.Syncing:
                    StatusText = "Syncing...";
                    StatusIcon = "sync";
                    StatusColor = Colors.Blue;
                    break;
                
                case SyncState.Error:
                    StatusText = "Sync error";
                    StatusIcon = "sync_problem";
                    StatusColor = Colors.Red;
                    break;
                
                case SyncState.Offline:
                    StatusText = "Offline";
                    StatusIcon = "wifi_off";
                    StatusColor = Colors.Orange;
                    break;
            }
        }
        
        OnPropertyChanged(nameof(LastSyncText));
        OnPropertyChanged(nameof(HasPendingChanges));
        OnPropertyChanged(nameof(PendingChangesText));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value))
            return false;

        backingStore = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}