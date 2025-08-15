using FinTrack.Maui.ViewModels;

namespace FinTrack.Maui.Views;

public partial class GoalFormPage : ContentPage
{
    public GoalFormPage(GoalFormViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}