namespace FinTrack.Maui.Views;

public partial class DashboardPage : ContentPage
{
    public DashboardPage()
    {
        InitializeComponent();
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