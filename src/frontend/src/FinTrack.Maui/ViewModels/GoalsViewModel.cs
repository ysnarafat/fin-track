using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using FinTrack.Maui.Models;
using FinTrack.Maui.Services;

namespace FinTrack.Maui.ViewModels;

/// <summary>
/// ViewModel for the goals overview page
/// </summary>
public class GoalsViewModel : INotifyPropertyChanged
{
    private readonly IGoalService _goalService;
    private bool _isLoading;
    private bool _isRefreshing;
    private GoalStatistics? _goalStatistics;
    private string _selectedFilter = "All";
    
    public GoalsViewModel(IGoalService goalService)
    {
        _goalService = goalService;
        Goals = new ObservableCollection<Goal>();
        FilteredGoals = new ObservableCollection<Goal>();
        RecentAchievements = new ObservableCollection<GoalMilestone>();
        
        LoadGoalsCommand = new Command(async () => await LoadGoalsAsync());
        RefreshCommand = new Command(async () => await RefreshAsync());
        AddGoalCommand = new Command(async () => await AddGoalAsync());
        EditGoalCommand = new Command<Goal>(async (goal) => await EditGoalAsync(goal));
        DeleteGoalCommand = new Command<Goal>(async (goal) => await DeleteGoalAsync(goal));
        UpdateProgressCommand = new Command<Goal>(async (goal) => await UpdateProgressAsync(goal));
        FilterGoalsCommand = new Command<string>(FilterGoals);
        CelebrateAchievementCommand = new Command<Goal>(CelebrateAchievement);
    }
    
    #region Properties
    
    public ObservableCollection<Goal> Goals { get; }
    public ObservableCollection<Goal> FilteredGoals { get; }
    public ObservableCollection<GoalMilestone> RecentAchievements { get; }
    
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
    
    public GoalStatistics? GoalStatistics
    {
        get => _goalStatistics;
        set => SetProperty(ref _goalStatistics, value);
    }
    
    public string SelectedFilter
    {
        get => _selectedFilter;
        set
        {
            if (SetProperty(ref _selectedFilter, value))
            {
                FilterGoals(value);
            }
        }
    }
    
    public bool HasGoals => Goals.Count > 0;
    public bool HasFilteredGoals => FilteredGoals.Count > 0;
    public bool HasRecentAchievements => RecentAchievements.Count > 0;
    
    public List<string> FilterOptions { get; } = new()
    {
        "All", "Active", "Completed", "Overdue", "High Priority"
    };
    
    #endregion
    
    #region Commands
    
    public ICommand LoadGoalsCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand AddGoalCommand { get; }
    public ICommand EditGoalCommand { get; }
    public ICommand DeleteGoalCommand { get; }
    public ICommand UpdateProgressCommand { get; }
    public ICommand FilterGoalsCommand { get; }
    public ICommand CelebrateAchievementCommand { get; }
    
    #endregion
    
    #region Methods
    
    public async Task InitializeAsync()
    {
        await LoadGoalsAsync();
    }
    
    private async Task LoadGoalsAsync()
    {
        if (IsLoading) return;
        
        try
        {
            IsLoading = true;
            
            // Load goals and statistics in parallel
            var goalsTask = _goalService.GetGoalsByPriorityAsync();
            var statisticsTask = _goalService.GetGoalStatisticsAsync();
            var achievementsTask = _goalService.GetRecentAchievementsAsync();
            
            await Task.WhenAll(goalsTask, statisticsTask, achievementsTask);
            
            // Update collections on UI thread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Goals.Clear();
                foreach (var goal in goalsTask.Result)
                {
                    Goals.Add(goal);
                }
                
                RecentAchievements.Clear();
                foreach (var achievement in achievementsTask.Result)
                {
                    RecentAchievements.Add(achievement);
                }
                
                GoalStatistics = statisticsTask.Result;
                
                // Apply current filter
                FilterGoals(SelectedFilter);
                
                OnPropertyChanged(nameof(HasGoals));
                OnPropertyChanged(nameof(HasRecentAchievements));
            });
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Failed to load goals", ex.Message);
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
            await LoadGoalsAsync();
        }
        finally
        {
            IsRefreshing = false;
        }
    }
    
    private async Task AddGoalAsync()
    {
        try
        {
            await Shell.Current.GoToAsync("goals/add");
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Navigation Error", ex.Message);
        }
    }
    
    private async Task EditGoalAsync(Goal goal)
    {
        if (goal == null) return;
        
        try
        {
            await Shell.Current.GoToAsync($"goals/edit?goalId={goal.Id}");
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Navigation Error", ex.Message);
        }
    }
    
    private async Task DeleteGoalAsync(Goal goal)
    {
        if (goal == null) return;
        
        try
        {
            var confirm = await (Application.Current?.MainPage?.DisplayAlert(
                "Delete Goal",
                $"Are you sure you want to delete the goal '{goal.Name}'?",
                "Delete",
                "Cancel") ?? Task.FromResult(false));
            
            if (!confirm) return;
            
            var success = await _goalService.DeleteGoalAsync(goal.Id);
            if (success)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Goals.Remove(goal);
                    FilteredGoals.Remove(goal);
                    OnPropertyChanged(nameof(HasGoals));
                    OnPropertyChanged(nameof(HasFilteredGoals));
                });
                
                await ShowSuccessAsync("Goal deleted successfully");
                await RefreshAsync(); // Refresh to update statistics
            }
            else
            {
                await ShowErrorAsync("Delete Failed", "Failed to delete the goal");
            }
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Delete Error", ex.Message);
        }
    }
    
    private async Task UpdateProgressAsync(Goal goal)
    {
        if (goal == null) return;
        
        try
        {
            var result = await Application.Current?.MainPage?.DisplayPromptAsync(
                "Update Progress",
                $"Enter current amount for '{goal.Name}':",
                "Update",
                "Cancel",
                placeholder: goal.CurrentAmount.ToString("F2"),
                keyboard: Keyboard.Numeric);
            
            if (string.IsNullOrWhiteSpace(result)) return;
            
            if (decimal.TryParse(result, out var newAmount))
            {
                var previousAmount = goal.CurrentAmount;
                goal.CurrentAmount = newAmount;
                
                var success = await _goalService.UpdateGoalProgressAsync(goal.Id, newAmount);
                if (success)
                {
                    // Check for milestone achievements or goal completion
                    if (goal.IsCompleted && previousAmount < goal.TargetAmount)
                    {
                        CelebrateAchievement(goal);
                    }
                    
                    await ShowSuccessAsync("Progress updated successfully");
                    await RefreshAsync(); // Refresh to update statistics and achievements
                }
                else
                {
                    // Revert the change if update failed
                    goal.CurrentAmount = previousAmount;
                    await ShowErrorAsync("Update Failed", "Failed to update goal progress");
                }
            }
            else
            {
                await ShowErrorAsync("Invalid Input", "Please enter a valid amount");
            }
        }
        catch (Exception ex)
        {
            await ShowErrorAsync("Update Error", ex.Message);
        }
    }
    
    private void FilterGoals(string filter)
    {
        if (string.IsNullOrWhiteSpace(filter)) return;
        
        MainThread.BeginInvokeOnMainThread(() =>
        {
            FilteredGoals.Clear();
            
            var filteredGoals = filter switch
            {
                "Active" => Goals.Where(g => !g.IsCompleted && !g.IsOverdue),
                "Completed" => Goals.Where(g => g.IsCompleted),
                "Overdue" => Goals.Where(g => g.IsOverdue),
                "High Priority" => Goals.Where(g => g.Priority <= 2),
                _ => Goals
            };
            
            foreach (var goal in filteredGoals)
            {
                FilteredGoals.Add(goal);
            }
            
            OnPropertyChanged(nameof(HasFilteredGoals));
        });
    }
    
    private void CelebrateAchievement(Goal goal)
    {
        if (goal == null) return;
        
        // Simple celebration - in a real app, you might show animations or confetti
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await Application.Current?.MainPage?.DisplayAlert(
                "🎉 Congratulations!",
                $"You've achieved your goal: {goal.Name}!",
                "Awesome!");
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