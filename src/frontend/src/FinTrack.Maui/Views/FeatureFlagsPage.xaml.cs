using FinTrack.Core.Interfaces;

namespace FinTrack.Maui.Views;

public partial class FeatureFlagsPage : ContentPage
{
    private readonly IFeatureFlagService _featureFlagService;
    private readonly ISyncService _syncService;

    public FeatureFlagsPage(IFeatureFlagService featureFlagService, ISyncService syncService)
    {
        InitializeComponent();
        _featureFlagService = featureFlagService;
        _syncService = syncService;
        
        LoadCurrentSettings();
    }

    private void LoadCurrentSettings()
    {
        OfflineSyncSwitch.IsToggled = _featureFlagService.IsFeatureEnabled(FeatureFlags.OfflineSync);
        SyncIndicatorsSwitch.IsToggled = _featureFlagService.IsFeatureEnabled(FeatureFlags.SyncStatusIndicators);
        AutoSyncSwitch.IsToggled = _featureFlagService.IsFeatureEnabled(FeatureFlags.AutomaticSync);
        ConflictResolutionSwitch.IsToggled = _featureFlagService.IsFeatureEnabled(FeatureFlags.ConflictResolution);
    }

    private async void OnOfflineSyncToggled(object sender, ToggledEventArgs e)
    {
        _featureFlagService.SetFeatureFlag(FeatureFlags.OfflineSync, e.Value);
        
        if (!e.Value)
        {
            // Stop sync service if offline sync is disabled
            await _syncService.StopAsync();
            await DisplayAlert("Sync Disabled", "Offline sync has been disabled. The sync service has been stopped.", "OK");
        }
        else
        {
            // Start sync service if offline sync is enabled
            await _syncService.StartAsync();
            await DisplayAlert("Sync Enabled", "Offline sync has been enabled. The sync service has been started.", "OK");
        }
    }

    private void OnSyncIndicatorsToggled(object sender, ToggledEventArgs e)
    {
        _featureFlagService.SetFeatureFlag(FeatureFlags.SyncStatusIndicators, e.Value);
        
        if (!e.Value)
        {
            DisplayAlert("Indicators Disabled", "Sync status indicators have been disabled. You may need to restart the app for changes to take full effect.", "OK");
        }
    }

    private async void OnAutoSyncToggled(object sender, ToggledEventArgs e)
    {
        _featureFlagService.SetFeatureFlag(FeatureFlags.AutomaticSync, e.Value);
        
        if (e.Value && _featureFlagService.IsFeatureEnabled(FeatureFlags.OfflineSync))
        {
            // Restart sync service to enable automatic sync
            await _syncService.StopAsync();
            await _syncService.StartAsync();
        }
    }

    private void OnConflictResolutionToggled(object sender, ToggledEventArgs e)
    {
        _featureFlagService.SetFeatureFlag(FeatureFlags.ConflictResolution, e.Value);
    }

    private async void OnResetToDefaultsClicked(object sender, EventArgs e)
    {
        var confirm = await DisplayAlert(
            "Reset to Defaults", 
            "This will reset all feature flags to their default values. Continue?", 
            "Yes", 
            "No");
        
        if (confirm)
        {
            _featureFlagService.SetFeatureFlag(FeatureFlags.OfflineSync, true);
            _featureFlagService.SetFeatureFlag(FeatureFlags.SyncStatusIndicators, true);
            _featureFlagService.SetFeatureFlag(FeatureFlags.AutomaticSync, true);
            _featureFlagService.SetFeatureFlag(FeatureFlags.ConflictResolution, true);
            
            LoadCurrentSettings();
            
            // Restart sync service with default settings
            await _syncService.StopAsync();
            await _syncService.StartAsync();
            
            await DisplayAlert("Reset Complete", "All feature flags have been reset to their default values.", "OK");
        }
    }

    private async void OnApplyChangesClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Changes Applied", "All feature flag changes have been applied successfully.", "OK");
    }
}