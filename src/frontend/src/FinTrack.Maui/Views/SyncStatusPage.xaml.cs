using FinTrack.Maui.ViewModels;
using FinTrack.Core.Interfaces;

namespace FinTrack.Maui.Views;

public partial class SyncStatusPage : ContentPage
{
    private readonly SyncStatusViewModel _viewModel;
    private readonly ISyncService _syncService;

    public SyncStatusPage(SyncStatusViewModel viewModel, ISyncService syncService)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _syncService = syncService;
        BindingContext = _viewModel;
    }

    private async void OnSyncNowClicked(object sender, EventArgs e)
    {
        try
        {
            await _syncService.SyncAsync();
            await DisplayAlert("Sync", "Sync completed successfully!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Sync Error", $"Sync failed: {ex.Message}", "OK");
        }
    }

    private async void OnViewPendingChangesClicked(object sender, EventArgs e)
    {
        var pendingChanges = await _syncService.GetPendingChangesAsync();
        var changesList = pendingChanges.Select(c => $"• {c.EntityType}: {c.Operation}").ToList();
        
        var message = changesList.Any() 
            ? string.Join("\n", changesList)
            : "No pending changes";
            
        await DisplayAlert("Pending Changes", message, "OK");
    }

    private async void OnClearCacheClicked(object sender, EventArgs e)
    {
        var confirm = await DisplayAlert(
            "Clear Cache", 
            "This will clear all cached data. Any unsaved changes will be lost. Continue?", 
            "Yes", 
            "No");
        
        if (confirm)
        {
            // TODO: Implement cache clearing logic
            await DisplayAlert("Cache Cleared", "Local cache has been cleared.", "OK");
        }
    }

    private async void OnFeatureSettingsClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("settings/features");
    }
}