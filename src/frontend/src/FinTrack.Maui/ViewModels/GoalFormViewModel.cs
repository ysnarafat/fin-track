using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using FinTrack.Maui.Models;
using FinTrack.Maui.Services;

namespace FinTrack.Maui.ViewModels;

/// <summary>
/// ViewModel for the goal creation and editing form
/// </summary>
public class GoalFormViewModel : INotifyPropertyChanged, IQueryAttributable
{
    private readonly IGoalService _goalService;
    private bool _isLoading;
    private bool _isSaving;
    private bool _isEditMode;
    private int _goalId;
    private string _name = string.Empty;
    private string _description = string.Empty;
    private decimal _targetAmount;
    private decimal _currentAmount;
    private DateTime _targetDate = DateTime.Today.AddMonths(12);
    private int _priority = 3;
    private string _category = string.Empty;
    private string _color = "#3B82F6";
    private int _currentStep = 1;
    
    public GoalFormViewModel(IGoalService goalService)
    {
        _goalService = goalService;
        Milestones = new List<GoalMilestone>();
        
        SaveCommand = new Command(async () => await SaveGoalAsync(), CanSave);
        CancelCommand = new Command(async () => await CancelAsync());
        NextStepCommand = new Command(NextStep, CanGoNext);
        PreviousStepCommand = new Command(PreviousStep, CanGoPrevious);
        AddMilestoneCommand = new Command(AddMilestone);
        RemoveMilestoneCommand = new Command<GoalMilestone>(RemoveMilestone);
        SelectColorCommand = new Command<string>(SelectColor);
        
        // Initialize validation
        UpdateValidation();
    }
    
    #region Properties
    
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
    
    public string Title => IsEditMode ? "Edit Goal" : "Create Goal";
    
    public string Name
    {
        get => _name;
        set
        {
            if (SetProperty(ref _name, value))
            {
                UpdateValidation();
            }
        }
    }
    
    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }
    
    public decimal TargetAmount
    {
        get => _targetAmount;
        set
        {
            if (SetProperty(ref _targetAmount, value))
            {
                UpdateValidation();
            }
        }
    }
    
    public decimal CurrentAmount
    {
        get => _currentAmount;
        set => SetProperty(ref _currentAmount, value);
    }
    
    public DateTime TargetDate
    {
        get => _targetDate;
        set
        {
            if (SetProperty(ref _targetDate, value))
            {
                UpdateValidation();
            }
        }
    }
    
    public int Priority
    {
        get => _priority;
        set => SetProperty(ref _priority, value);
    }
    
    public string Category
    {
        get => _category;
        set => SetProperty(ref _category, value);
    }
    
    public string Color
    {
        get => _color;
        set => SetProperty(ref _color, value);
    }
    
    public int CurrentStep
    {
        get => _currentStep;
        set
        {
            if (SetProperty(ref _currentStep, value))
            {
                OnPropertyChanged(nameof(IsStep1));
                OnPropertyChanged(nameof(IsStep2));
                OnPropertyChanged(nameof(IsStep3));
                OnPropertyChanged(nameof(StepTitle));
                OnPropertyChanged(nameof(StepDescription));
                ((Command)NextStepCommand).ChangeCanExecute();
                ((Command)PreviousStepCommand).ChangeCanExecute();
            }
        }
    }
    
    public bool IsStep1 => CurrentStep == 1;
    public bool IsStep2 => CurrentStep == 2;
    public bool IsStep3 => CurrentStep == 3;
    
    public string StepTitle => CurrentStep switch
    {
        1 => "Goal Details",
        2 => "Timeline & Priority",
        3 => "Milestones",
        _ => "Goal Setup"
    };
    
    public string StepDescription => CurrentStep switch
    {
        1 => "What do you want to achieve?",
        2 => "When and how important is this goal?",
        3 => "Break it down into smaller milestones",
        _ => ""
    };
    
    public List<GoalMilestone> Milestones { get; set; }
    
    public List<string> CategoryOptions { get; } = new()
    {
        "Emergency Fund",
        "Vacation",
        "Car",
        "House",
        "Education",
        "Retirement",
        "Investment",
        "Debt Payoff",
        "Other"
    };
    
    public List<string> PriorityOptions { get; } = new()
    {
        "1 - High",
        "2 - Medium-High", 
        "3 - Medium",
        "4 - Medium-Low",
        "5 - Low"
    };
    
    public List<string> ColorOptions { get; } = new()
    {
        "#3B82F6", // Blue
        "#10B981", // Green
        "#F59E0B", // Yellow
        "#EF4444", // Red
        "#8B5CF6", // Purple
        "#F97316", // Orange
        "#06B6D4", // Cyan
        "#84CC16", // Lime
        "#EC4899", // Pink
        "#6B7280"  // Gray
    };
    
    // Validation properties
    public bool IsNameValid => !string.IsNullOrWhiteSpace(Name);
    public bool IsTargetAmountValid => TargetAmount > 0;
    public bool IsTargetDateValid => TargetDate > DateTime.Today;
    public bool IsFormValid => IsNameValid && IsTargetAmountValid && IsTargetDateValid;
    
    #endregion
    
    #region Commands
    
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand NextStepCommand { get; }
    public ICommand PreviousStepCommand { get; }
    public ICommand AddMilestoneCommand { get; }
    public ICommand RemoveMilestoneCommand { get; }
    public ICommand SelectColorCommand { get; }
    
    #endregion
    
    #region Methods
    
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("goalId", out var goalIdObj) && 
            int.TryParse(goalIdObj.ToString(), out var goalId))
        {
            _goalId = goalId;
            IsEditMode = true;
            _ = Task.Run(LoadGoalAsync);
        }
    }
    
    private async Task LoadGoalAsync()
    {
        if (_goalId <= 0) return;
        
        try
        {
            IsLoading = true;
            
            var goal = await _goalService.GetGoalWithMilestonesAsync(_goalId);
            if (goal != null)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Name = goal.Name;
                    Description = goal.Description;
                    TargetAmount = goal.TargetAmount;
                    CurrentAmount = goal.CurrentAmount;
                    TargetDate = goal.TargetDate;
                    Priority = goal.Priority;
                    Category = goal.Category;
                    Color = goal.Color;
                    Milestones = goal.Milestones.ToList();
                    
                    UpdateValidation();
                });
            }
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Failed to load goal", ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    private async Task SaveGoalAsync()
    {
        if (!CanSave()) return;
        
        try
        {
            IsSaving = true;
            
            var goal = new Goal
            {
                Id = _goalId,
                Name = Name,
                Description = Description,
                TargetAmount = TargetAmount,
                CurrentAmount = CurrentAmount,
                TargetDate = TargetDate,
                Priority = Priority,
                Category = Category,
                Color = Color,
                Milestones = Milestones
            };
            
            bool success;
            if (IsEditMode)
            {
                success = await _goalService.UpdateGoalAsync(goal);
            }
            else
            {
                var createdGoal = await _goalService.CreateGoalAsync(goal);
                success = createdGoal != null;
            }
            
            if (success)
            {
                await ShowSuccessAsync(IsEditMode ? "Goal updated successfully" : "Goal created successfully");
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                await ShowErrorAsync("Save Failed", "Failed to save the goal");
            }
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Save Error", ex.Message);
        }
        finally
        {
            IsSaving = false;
        }
    }
    
    private async Task CancelAsync()
    {
        var hasChanges = !string.IsNullOrWhiteSpace(Name) || 
                        TargetAmount > 0 || 
                        !string.IsNullOrWhiteSpace(Description);
        
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
    
    private void NextStep()
    {
        if (CanGoNext())
        {
            CurrentStep++;
        }
    }
    
    private void PreviousStep()
    {
        if (CanGoPrevious())
        {
            CurrentStep--;
        }
    }
    
    private bool CanGoNext()
    {
        return CurrentStep switch
        {
            1 => IsNameValid && IsTargetAmountValid,
            2 => IsTargetDateValid,
            _ => false
        };
    }
    
    private bool CanGoPrevious()
    {
        return CurrentStep > 1;
    }
    
    private void AddMilestone()
    {
        var milestone = new GoalMilestone
        {
            Name = $"Milestone {Milestones.Count + 1}",
            TargetAmount = TargetAmount * (Milestones.Count + 1) / 4, // Suggest 25%, 50%, 75%
            Description = ""
        };
        
        Milestones.Add(milestone);
        OnPropertyChanged(nameof(Milestones));
    }
    
    private void RemoveMilestone(GoalMilestone milestone)
    {
        if (milestone != null)
        {
            Milestones.Remove(milestone);
            OnPropertyChanged(nameof(Milestones));
        }
    }
    
    private void SelectColor(string color)
    {
        if (!string.IsNullOrWhiteSpace(color))
        {
            Color = color;
        }
    }
    
    private bool CanSave()
    {
        return IsFormValid && !IsSaving;
    }
    
    private void UpdateValidation()
    {
        OnPropertyChanged(nameof(IsNameValid));
        OnPropertyChanged(nameof(IsTargetAmountValid));
        OnPropertyChanged(nameof(IsTargetDateValid));
        OnPropertyChanged(nameof(IsFormValid));
        ((Command)SaveCommand).ChangeCanExecute();
        ((Command)NextStepCommand).ChangeCanExecute();
    }
    
    private async Task ShowErrorAsync(string title, string message)
    {
        await Application.Current?.MainPage?.DisplayAlert(title, message, "OK");
    }
    
    private async Task ShowSuccessAsync(string message)
    {
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