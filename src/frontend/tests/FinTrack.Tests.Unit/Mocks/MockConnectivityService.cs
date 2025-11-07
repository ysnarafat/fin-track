using System;
using FinTrack.Core.Interfaces;

namespace FinTrack.Tests.Unit.Mocks;

/// <summary>
/// Mock implementation of IConnectivityService for testing
/// </summary>
public class MockConnectivityService : IConnectivityService
{
    private bool _isConnected = true;

    public bool IsConnected
    {
        get => _isConnected;
        set
        {
            if (_isConnected != value)
            {
                _isConnected = value;
                ConnectivityChanged?.Invoke(this, value);
            }
        }
    }

    public event EventHandler<bool>? ConnectivityChanged;

    public void StartMonitoring()
    {
        // Mock implementation - no-op
    }

    public void StopMonitoring()
    {
        // Mock implementation - no-op
    }

    public void SimulateConnectivityChange(bool isConnected)
    {
        IsConnected = isConnected;
    }
}
