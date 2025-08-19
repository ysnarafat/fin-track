using FinTrack.Maui.ViewModels;
using FinTrack.Core.Interfaces;

namespace FinTrack.Maui;

public partial class AppShell : Shell
{
    private readonly SyncStatusViewModel _syncStatusViewModel;
    private readonly ISyncService _syncService;
    private readonly IFeatureFlagService _featureFlagService;

    public AppShell(SyncStatusViewModel syncStatusViewModel, ISyncService syncService, IFeatureFlagService featureFlagService)
    {
        InitializeComponent();
        
        _syncStatusViewModel = syncStatusViewModel;
        _syncService = syncService;
        _featureFlagService = featureFlagService;
        
        // Set binding context for sync status
        SyncStatusFrame.BindingContext = _syncStatusViewModel;
        SyncStatusIcon.BindingContext = _syncStatusViewModel;
        SyncStatusText.BindingContext = _syncStatusViewModel;
        PendingChangesIndicator.BindingContext = _syncStatusViewModel;
        
        // Register routes for navigation
        Routing.RegisterRoute("transactions/add", typeof(Views.TransactionFormPage));
        Routing.RegisterRoute("sync/status", typeof(Views.SyncStatusPage));
        Routing.RegisterRoute("settings/features", typeof(Views.FeatureFlagsPage));
    }

    private async void OnSyncStatusTapped(object sender, EventArgs e)
    {
        // Check if sync feature is enabled
        if (!_featureFlagService.IsFeatureEnabled(FeatureFlags.OfflineSync))
        {
            await DisplayAlert("Sync Disabled", "Sync functionality is currently disabled.", "OK");
            return;
        }

        // Show sync status details or trigger manual sync
        var action = await DisplayActionSheet(
            "Sync Options", 
            "Cancel", 
            null, 
            "View Sync Status", 
            "Sync Now", 
            "View Pending Changes");

        switch (action)
        {
            case "View Sync Status":
                await Shell.Current.GoToAsync("sync/status");
                break;
            
            case "Sync Now":
                await _syncService.SyncAsync();
                break;
            
            case "View Pending Changes":
                await ShowPendingChangesAsync();
                break;
        }
    }

    private async Task ShowPendingChangesAsync()
    {
        var pendingChanges = await _syncService.GetPendingChangesAsync();
        var conflicts = await _syncService.GetConflictsAsync();
        
        var message = $"Pending Changes: {pendingChanges.Count()}\nConflicts: {conflicts.Count()}";
        
        if (conflicts.Any())
        {
            var resolveConflicts = await DisplayAlert(
                "Sync Status", 
                message + "\n\nYou have unresolved conflicts. Would you like to resolve them?", 
                "Resolve", 
                "Later");
            
            if (resolveConflicts && _featureFlagService.IsFeatureEnabled(FeatureFlags.ConflictResolution))
            {
                await ShowConflictResolutionAsync(conflicts.First());
            }
        }
        else
        {
            await DisplayAlert("Sync Status", message, "OK");
        }
    }

    private async Task ShowConflictResolutionAsync(SyncConflict conflict)
    {
        var resolution = await DisplayActionSheet(
            $"Resolve Conflict: {conflict.EntityType}",
            "Cancel",
            null,
            "Use Local Version",
            "Use Remote Version");

        if (resolution != "Cancel" && resolution != null)
        {
            var conflictResolution = resolution switch
            {
                "Use Local Version" => ConflictResolution.UseLocal,
                "Use Remote Version" => ConflictResolution.UseRemote,
                _ => ConflictResolution.UseLocal
            };

            await _syncService.ResolveConflictAsync(conflict.Id, conflictResolution);
            await DisplayAlert("Conflict Resolved", "The conflict has been resolved successfully.", "OK");
        }
    }
}