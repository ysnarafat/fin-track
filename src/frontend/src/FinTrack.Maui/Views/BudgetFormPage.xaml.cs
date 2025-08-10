using FinTrack.Maui.ViewModels;

namespace FinTrack.Maui.Views;

public partial class BudgetFormPage : ContentPage
{
    public BudgetFormPage(BudgetFormViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
    
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        if (BindingContext is BudgetFormViewModel viewModel)
        {
            await viewModel.InitializeAsync();
        }
    }
}