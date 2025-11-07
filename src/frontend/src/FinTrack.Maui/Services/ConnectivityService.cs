using FinTrack.Core.Interfaces;

namespace FinTrack.Maui.Services;

/// <summary>
/// MAUI implementation of connectivity service using Microsoft.Maui.Networking
/// </summary>
public class ConnectivityService : IConnectivityService
{
    private bool _isMonitoring;

    public bool IsConnected => Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

    public event EventHandler<bool>? ConnectivityChanged;

    public void StartMonitoring()
    {
        if (_isMonitoring)
            return;

        Connectivity.Current.ConnectivityChanged += OnConnectivityChanged;
        _isMonitoring = true;
    }

    public void StopMonitoring()
    {
        if (!_isMonitoring)
            return;

        Connectivity.Current.ConnectivityChanged -= OnConnectivityChanged;
        _isMonitoring = false;
    }

    private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {
        var isConnected = e.NetworkAccess == NetworkAccess.Internet;
        ConnectivityChanged?.Invoke(this, isConnected);
    }
}