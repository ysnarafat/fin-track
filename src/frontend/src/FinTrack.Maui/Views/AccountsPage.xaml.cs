namespace FinTrack.Maui.Views;

public partial class AccountsPage : ContentPage
{
    public AccountsPage()
    {
        InitializeComponent();
    }

    private async void OnAddAccountClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Add Account", "Add account functionality will be implemented here.", "OK");
    }
}