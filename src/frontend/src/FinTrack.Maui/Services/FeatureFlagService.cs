using FinTrack.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace FinTrack.Maui.Services;

/// <summary>
/// MAUI implementation of feature flag service with persistent storage
/// </summary>
public class FeatureFlagService : IFeatureFlagService
{
    private readonly ILogger<FeatureFlagService> _logger;
    private readonly Dictionary<string, bool> _featureFlags;
    private readonly string _settingsKey = "FeatureFlags";

    public FeatureFlagService(ILogger<FeatureFlagService> logger)
    {
        _logger = logger;
        _featureFlags = new Dictionary<string, bool>();
        
        // Initialize default feature flags
        InitializeDefaultFlags();
        
        // Load persisted flags
        LoadPersistedFlags();
    }

    public bool IsFeatureEnabled(string flagName)
    {
        if (_featureFlags.TryGetValue(flagName, out bool value))
        {
            _logger.LogDebug("Feature flag {FlagName} is {Status}", flagName, value ? "enabled" : "disabled");
            return value;
        }

        _logger.LogWarning("Feature flag {FlagName} not found, defaulting to false", flagName);
        return false;
    }

    public void SetFeatureFlag(string flagName, bool enabled)
    {
        _featureFlags[flagName] = enabled;
        _logger.LogInformation("Feature flag {FlagName} set to {Status}", flagName, enabled ? "enabled" : "disabled");
        
        // Persist the change
        PersistFlags();
    }

    public Dictionary<string, bool> GetAllFeatureFlags()
    {
        return new Dictionary<string, bool>(_featureFlags);
    }

    private void InitializeDefaultFlags()
    {
        // Default feature flag values
        _featureFlags[FeatureFlags.OfflineSync] = true;
        _featureFlags[FeatureFlags.SyncStatusIndicators] = true;
        _featureFlags[FeatureFlags.AutomaticSync] = true;
        _featureFlags[FeatureFlags.ConflictResolution] = true;
        
        _logger.LogInformation("Initialized default feature flags");
    }

    private void LoadPersistedFlags()
    {
        try
        {
            // Try to load from preferences
            var flagsJson = Preferences.Get(_settingsKey, string.Empty);
            if (!string.IsNullOrEmpty(flagsJson))
            {
                var persistedFlags = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, bool>>(flagsJson);
                if (persistedFlags != null)
                {
                    foreach (var flag in persistedFlags)
                    {
                        _featureFlags[flag.Key] = flag.Value;
                    }
                    _logger.LogInformation("Loaded {Count} persisted feature flags", persistedFlags.Count);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load persisted feature flags, using defaults");
        }
    }

    private void PersistFlags()
    {
        try
        {
            var flagsJson = System.Text.Json.JsonSerializer.Serialize(_featureFlags);
            Preferences.Set(_settingsKey, flagsJson);
            _logger.LogDebug("Persisted feature flags to preferences");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist feature flags");
        }
    }
}