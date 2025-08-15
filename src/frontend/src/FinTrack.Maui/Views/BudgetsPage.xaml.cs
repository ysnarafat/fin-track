using FinTrack.Maui.ViewModels;

namespace FinTrack.Maui.Views;

public partial class BudgetsPage : ContentPage
{
    public BudgetsPage(BudgetsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
    
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        if (BindingContext is BudgetsViewModel viewModel)
        {
            await viewModel.InitializeAsync();
        }
    }
}