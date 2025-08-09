using FinTrack.Maui.ViewModels;

namespace FinTrack.Maui.Views;

public partial class TransactionsPage : ContentPage
{
    public TransactionsPage(TransactionsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}