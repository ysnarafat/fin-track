using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using FinTrack.Maui.Models;
using FinTrack.Maui.Services;

namespace FinTrack.Maui.ViewModels;

/// <summary>
/// ViewModel for budget creation and editing form
/// </summary>
public class BudgetFormViewModel : INotifyPropertyChanged, IQueryAttributable
{
    private readonly IBudgetService _budgetService;
    private bool _isLoading;
    private bool _isSaving;
    private string _name = string.Empty;
    private decimal _budgetLimit;
    private DateTime _startDate;
    private DateTime _endDate;
    private CategoryOption? _selectedCategory;
    private int? _budgetId;
    private bool _isEditMode;
    
    public BudgetFormViewModel(IBudgetService budgetService)
    {
        _budgetService = budgetService;
        Categories = new ObservableCollection<CategoryOption>();
        
        // Initialize dates to current month
        var now = DateTime.Today;
        _startDate = new DateTime(now.Year, now.Month, 1);
        _endDate = _startDate.AddMonths(1).AddDays(-1);
        
        LoadCategoriesCommand = new Command(async () => await LoadCategoriesAsync());
        SaveCommand = new Command(async () => await SaveBudgetAsync(), CanSave);
        CancelCommand = new Command(async () => await CancelAsync());
        
        // Validate form when properties change
        PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(Name) || 
                e.PropertyName == nameof(BudgetLimit) || 
                e.PropertyName == nameof(SelectedCategory))
            {
                ((Command)SaveCommand).ChangeCanExecute();
            }
        };
    }
    
    #region Properties
    
    public ObservableCollection<CategoryOption> Categories { get; }
    
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }
    
    public bool IsSaving
    {
        get => _isSaving;
        set => SetProperty(ref _isSaving, value);
    }
    
    public bool IsEditMode
    {
        get => _isEditMode;
        set => SetProperty(ref _isEditMode, value);
    }
    
    public string PageTitle => IsEditMode ? "Edit Budget" : "Create Budget";
    public string SaveButtonText => IsEditMode ? "Update" : "Create";
    
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }
    
    public decimal BudgetLimit
    {
        get => _budgetLimit;
        set => SetProperty(ref _budgetLimit, value);
    }
    
    public DateTime StartDate
    {
        get => _startDate;
        set
        {
            if (SetProperty(ref _startDate, value))
            {
                // Auto-adjust end date to end of month if start date changes
                if (value.Day == 1)
                {
                    EndDate = value.AddMonths(1).AddDays(-1);
                }
            }
        }
    }
    
    public DateTime EndDate
    {
        get => _endDate;
        set => SetProperty(ref _endDate, value);
    }
    
    public CategoryOption? SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            if (SetProperty(ref _selectedCategory, value) && value != null && string.IsNullOrEmpty(Name))
            {
                // Auto-generate name based on category
                Name = $"{value.Name} Budget";
            }
        }
    }
    
    public DateTime MinDate => DateTime.Today.AddYears(-1);
    public DateTime MaxDate => DateTime.Today.AddYears(2);
    
    #endregion
    
    #region Commands
    
    public ICommand LoadCategoriesCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    
    #endregion
    
    #region Methods
    
    public async Task InitializeAsync()
    {
        await LoadCategoriesAsync();
        
        if (_budgetId.HasValue)
        {
            await LoadBudgetAsync(_budgetId.Value);
        }
    }
    
    private async Task LoadCategoriesAsync()
    {
        if (IsLoading) return;
        
        try
        {
            IsLoading = true;
            
            var categories = await _budgetService.GetAvailableCategoriesAsync();
            
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Categories.Clear();
                foreach (var category in categories)
                {
                    Categories.Add(category);
                }
            });
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Failed to load categories", ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    private async Task LoadBudgetAsync(int budgetId)
    {
        try
        {
            IsLoading = true;
            
            var budget = await _budgetService.GetBudgetAsync(budgetId);
            if (budget != null)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Name = budget.Name;
                    BudgetLimit = budget.BudgetLimit;
                    StartDate = budget.StartDate;
                    EndDate = budget.EndDate;
                    SelectedCategory = Categories.FirstOrDefault(c => c.Id == budget.CategoryId);
                    IsEditMode = true;
                });
            }
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Failed to load budget", ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    private bool CanSave()
    {
        return !string.IsNullOrWhiteSpace(Name) &&
               BudgetLimit > 0 &&
               SelectedCategory != null &&
               StartDate <= EndDate &&
               !IsSaving;
    }
    
    private async Task SaveBudgetAsync()
    {
        if (!CanSave()) return;
        
        try
        {
            IsSaving = true;
            
            var budget = new BudgetModel
            {
                Id = _budgetId ?? 0,
                Name = Name.Trim(),
                CategoryId = SelectedCategory!.Id,
                BudgetLimit = BudgetLimit,
                StartDate = StartDate,
                EndDate = EndDate
            };
            
            if (IsEditMode)
            {
                await _budgetService.UpdateBudgetAsync(budget);
                await ShowSuccessAsync("Budget updated successfully");
            }
            else
            {
                await _budgetService.CreateBudgetAsync(budget);
                await ShowSuccessAsync("Budget created successfully");
            }
            
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Save Failed", ex.Message);
        }
        finally
        {
            IsSaving = false;
        }
    }
    
    private async Task CancelAsync()
    {
        var hasChanges = !string.IsNullOrWhiteSpace(Name) || BudgetLimit > 0 || SelectedCategory != null;
        
        if (hasChanges)
        {
            var confirm = await (Application.Current?.MainPage?.DisplayAlert(
                "Discard Changes",
                "Are you sure you want to discard your changes?",
                "Discard",
                "Continue Editing") ?? Task.FromResult(false));
            
            if (!confirm) return;
        }
        
        await Shell.Current.GoToAsync("..");
    }
    
    private async Task ShowErrorAsync(string title, string message)
    {
        await Application.Current?.MainPage?.DisplayAlert(title, message, "OK");
    }
    
    private async Task ShowSuccessAsync(string message)
    {
        // In a real app, you might use a toast notification
        await Application.Current?.MainPage?.DisplayAlert("Success", message, "OK");
    }
    
    #endregion
    
    #region IQueryAttributable
    
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("budgetId", out var budgetIdObj) && 
            int.TryParse(budgetIdObj.ToString(), out var budgetId))
        {
            _budgetId = budgetId;
        }
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