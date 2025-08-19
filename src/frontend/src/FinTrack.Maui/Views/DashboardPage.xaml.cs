using FinTrack.Core.Interfaces;

namespace FinTrack.Maui.Views;

public partial class DashboardPage : ContentPage
{
    private readonly IConnectivityService _connectivityService;
    private readonly IFeatureFlagService _featureFlagService;

    public DashboardPage(IConnectivityService connectivityService, IFeatureFlagService featureFlagService)
    {
        InitializeComponent();
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

    private async void OnAddTransactionClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//transactions/add");
    }

    private async void OnViewTransactionsClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//transactions");
    }
}