using FinTrack.Maui.ViewModels;
using FinTrack.Core.Interfaces;

namespace FinTrack.Maui.Views;

public partial class TransactionsPage : ContentPage
{
    private readonly IConnectivityService _connectivityService;
    private readonly IFeatureFlagService _featureFlagService;

    public TransactionsPage(TransactionsViewModel viewModel, IConnectivityService connectivityService, IFeatureFlagService featureFlagService)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _connectivityService = connectivityService;
        _featureFlagService = featureFlagService;
        
        // Subscribe to connectivity changes
        _connectivityService.ConnectivityChanged += OnConnectivityChanged;
        
        // Set initial state
        UpdateOfflineIndicator(_connectivityService.IsConnected);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _connectivityService.StartMonitoring();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _connectivityService.StopMonitoring();
    }

    private void OnConnectivityChanged(object? sender, bool isConnected)
    {
        MainThread.BeginInvokeOnMainThread(() => UpdateOfflineIndicator(isConnected));
    }

    private void UpdateOfflineIndicator(bool isConnected)
    {
        // Only show offline banner if sync indicators are enabled
        OfflineBanner.IsVisible = !isConnected && _featureFlagService.IsFeatureEnabled(FeatureFlags.SyncStatusIndicators);
    }
}