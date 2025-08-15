namespace FinTrack.Maui;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        
        // Register routes for navigation
        Routing.RegisterRoute("transactions/add", typeof(Views.TransactionFormPage));
    }
}