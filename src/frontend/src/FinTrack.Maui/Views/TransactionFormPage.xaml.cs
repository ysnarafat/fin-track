using FinTrack.Maui.ViewModels;

namespace FinTrack.Maui.Views;

public partial class TransactionFormPage : ContentPage
{
    public TransactionFormPage(TransactionFormViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}