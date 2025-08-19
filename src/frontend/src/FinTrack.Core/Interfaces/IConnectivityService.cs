namespace FinTrack.Core.Interfaces;

/// <summary>
/// Service for detecting network connectivity status
/// </summary>
public interface IConnectivityService
{
    /// <summary>
    /// Gets the current connectivity status
    /// </summary>
    bool IsConnected { get; }
    
    /// <summary>
    /// Event raised when connectivity status changes
    /// </summary>
    event EventHandler<bool> ConnectivityChanged;
    
    /// <summary>
    /// Starts monitoring connectivity changes
    /// </summary>
    void StartMonitoring();
    
    /// <summary>
    /// Stops monitoring connectivity changes
    /// </summary>
    void StopMonitoring();
}