using FinTrack.Maui.ViewModels;

namespace FinTrack.Maui.Views;

public partial class GoalsPage : ContentPage
{
    public GoalsPage(GoalsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
    
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        if (BindingContext is GoalsViewModel viewModel)
        {
            await viewModel.InitializeAsync();
        }
    }
}