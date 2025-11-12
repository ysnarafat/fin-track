using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using FinTrack.Maui.Models;
using FinTrack.Maui.Services;

namespace FinTrack.Maui.ViewModels;

/// <summary>
/// ViewModel for the budgets overview page
/// </summary>
public class BudgetsViewModel : INotifyPropertyChanged
{
    private readonly IBudgetService _budgetService;
    private bool _isLoading;
    private bool _isRefreshing;
    private BudgetSummary? _budgetSummary;
    
    public BudgetsViewModel(IBudgetService budgetService)
    {
        _budgetService = budgetService;
        Budgets = new ObservableCollection<BudgetModel>();
        BudgetAlerts = new ObservableCollection<BudgetAlert>();
        
        LoadBudgetsCommand = new Command(async () => await LoadBudgetsAsync());
        RefreshCommand = new Command(async () => await RefreshAsync());
        AddBudgetCommand = new Command(async () => await AddBudgetAsync());
        EditBudgetCommand = new Command<BudgetModel>(async (budget) => await EditBudgetAsync(budget));
        DeleteBudgetCommand = new Command<BudgetModel>(async (budget) => await DeleteBudgetAsync(budget));
        DismissAlertCommand = new Command<BudgetAlert>(DismissAlert);
    }
    
    #region Properties
    
    public ObservableCollection<BudgetModel> Budgets { get; }
    public ObservableCollection<BudgetAlert> BudgetAlerts { get; }
    
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }
    
    public bool IsRefreshing
    {
        get => _isRefreshing;
        set => SetProperty(ref _isRefreshing, value);
    }
    
    public BudgetSummary? BudgetSummary
    {
        get => _budgetSummary;
        set => SetProperty(ref _budgetSummary, value);
    }
    
    public bool HasBudgets => Budgets.Count > 0;
    public bool HasAlerts => BudgetAlerts.Count > 0;
    
    #endregion
    
    #region Commands
    
    public ICommand LoadBudgetsCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand AddBudgetCommand { get; }
    public ICommand EditBudgetCommand { get; }
    public ICommand DeleteBudgetCommand { get; }
    public ICommand DismissAlertCommand { get; }
    
    #endregion
    
    #region Methods
    
    public async Task InitializeAsync()
    {
        await LoadBudgetsAsync();
    }
    
    private async Task LoadBudgetsAsync()
    {
        if (IsLoading) return;
        
        try
        {
            IsLoading = true;
            
            // Load budgets and summary in parallel
            var budgetsTask = _budgetService.GetCurrentMonthBudgetsAsync();
            var summaryTask = _budgetService.GetBudgetSummaryAsync();
            var alertsTask = _budgetService.GetBudgetAlertsAsync();
            
            await Task.WhenAll(budgetsTask, summaryTask, alertsTask);
            
            // Update collections on UI thread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Budgets.Clear();
                foreach (var budget in budgetsTask.Result)
                {
                    Budgets.Add(budget);
                }
                
                BudgetAlerts.Clear();
                foreach (var alert in alertsTask.Result)
                {
                    BudgetAlerts.Add(alert);
                }
                
                BudgetSummary = summaryTask.Result;
                
                OnPropertyChanged(nameof(HasBudgets));
                OnPropertyChanged(nameof(HasAlerts));
            });
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Failed to load budgets", ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    private async Task RefreshAsync()
    {
        if (IsRefreshing) return;
        
        try
        {
            IsRefreshing = true;
            await LoadBudgetsAsync();
        }
        finally
        {
            IsRefreshing = false;
        }
    }
    
    private async Task AddBudgetAsync()
    {
        try
        {
            await Shell.Current.GoToAsync("budgets/add");
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Navigation Error", ex.Message);
        }
    }
    
    private async Task EditBudgetAsync(BudgetModel budget)
    {
        if (budget == null) return;
        
        try
        {
            await Shell.Current.GoToAsync($"budgets/edit?budgetId={budget.Id}");
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Navigation Error", ex.Message);
        }
    }
    
    private async Task DeleteBudgetAsync(BudgetModel budget)
    {
        if (budget == null) return;
        
        try
        {
            var confirm = await (Application.Current?.MainPage?.DisplayAlert(
                "Delete Budget",
                $"Are you sure you want to delete the budget for {budget.CategoryName}?",
                "Delete",
                "Cancel") ?? Task.FromResult(false));
            
            if (!confirm) return;
            
            var success = await _budgetService.DeleteBudgetAsync(budget.Id);
            if (success)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Budgets.Remove(budget);
                    OnPropertyChanged(nameof(HasBudgets));
                });
                
                await ShowSuccessAsync("Budget deleted successfully");
                await RefreshAsync(); // Refresh to update summary
            }
            else
            {
                await ShowErrorAsync("Delete Failed", "Failed to delete the budget");
            }
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Delete Error", ex.Message);
        }
    }
    
    private void DismissAlert(BudgetAlert alert)
    {
        if (alert == null) return;
        
        MainThread.BeginInvokeOnMainThread(() =>
        {
            BudgetAlerts.Remove(alert);
            OnPropertyChanged(nameof(HasAlerts));
        });
    }
    
    private async Task ShowErrorAsync(string title, string message)
    {
        await Application.Current?.MainPage?.DisplayAlert(title, message, "OK");
    }
    
    private async Task ShowSuccessAsync(string message)
    {
        // In a real app, you might use a toast notification or snackbar
        await Application.Current?.MainPage?.DisplayAlert("Success", message, "OK");
    }
    
    #endregion
    
    #region INotifyPropertyChanged
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value))
            return false;
        
        backingStore = value;
        OnPropertyChanged(propertyName);
        return true;
    }
    
    #endregion
}