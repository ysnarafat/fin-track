namespace FinTrack.Core.Interfaces;

/// <summary>
/// Service for managing feature flags
/// </summary>
public interface IFeatureFlagService
{
    /// <summary>
    /// Gets the value of a feature flag
    /// </summary>
    /// <param name="flagName">Name of the feature flag</param>
    /// <returns>True if the feature is enabled, false otherwise</returns>
    bool IsFeatureEnabled(string flagName);
    
    /// <summary>
    /// Sets the value of a feature flag
    /// </summary>
    /// <param name="flagName">Name of the feature flag</param>
    /// <param name="enabled">Whether the feature should be enabled</param>
    void SetFeatureFlag(string flagName, bool enabled);
    
    /// <summary>
    /// Gets all feature flags
    /// </summary>
    /// <returns>Dictionary of feature flag names and their values</returns>
    Dictionary<string, bool> GetAllFeatureFlags();
}

/// <summary>
/// Constants for feature flag names
/// </summary>
public static class FeatureFlags
{
    /// <summary>
    /// Feature flag for offline sync functionality
    /// </summary>
    public const string OfflineSync = "OfflineSync";
    
    /// <summary>
    /// Feature flag for sync status indicators in UI
    /// </summary>
    public const string SyncStatusIndicators = "SyncStatusIndicators";
    
    /// <summary>
    /// Feature flag for automatic sync
    /// </summary>
    public const string AutomaticSync = "AutomaticSync";
    
    /// <summary>
    /// Feature flag for conflict resolution dialogs
    /// </summary>
    public const string ConflictResolution = "ConflictResolution";
}