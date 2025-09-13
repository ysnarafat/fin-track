using System.ComponentModel;

namespace FinTrack.Maui.Models;

/// <summary>
/// Goal model for UI display and data binding
/// </summary>
public class Goal : INotifyPropertyChanged
{
    private decimal _currentAmount;
    private bool _isCompleted;
    
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal TargetAmount { get; set; }
    
    public decimal CurrentAmount
    {
        get => _currentAmount;
        set
        {
            if (_currentAmount != value)
            {
                _currentAmount = value;
                OnPropertyChanged(nameof(CurrentAmount));
                OnPropertyChanged(nameof(ProgressPercentage));
                OnPropertyChanged(nameof(RemainingAmount));
                OnPropertyChanged(nameof(StatusColor));
                OnPropertyChanged(nameof(StatusText));
                OnPropertyChanged(nameof(IsCompleted));
            }
        }
    }
    
    public DateTime TargetDate { get; set; }
    public int Priority { get; set; } = 3;
    public string Category { get; set; } = string.Empty;
    public string Color { get; set; } = "#3B82F6";
    
    public bool IsCompleted
    {
        get => _isCompleted || CurrentAmount >= TargetAmount;
        set
        {
            if (_isCompleted != value)
            {
                _isCompleted = value;
                OnPropertyChanged(nameof(IsCompleted));
                OnPropertyChanged(nameof(StatusColor));
                OnPropertyChanged(nameof(StatusText));
            }
        }
    }
    
    public DateTime? CompletedDate { get; set; }
    public List<GoalMilestone> Milestones { get; set; } = new();
    
    /// <summary>
    /// Progress percentage towards the goal (0-100)
    /// </summary>
    public double ProgressPercentage => TargetAmount > 0 ? Math.Min((double)(CurrentAmount / TargetAmount * 100), 100) : 0;
    
    /// <summary>
    /// Remaining amount needed to achieve the goal
    /// </summary>
    public decimal RemainingAmount => Math.Max(TargetAmount - CurrentAmount, 0);
    
    /// <summary>
    /// Days remaining until target date
    /// </summary>
    public int DaysRemaining => Math.Max((TargetDate - DateTime.Now).Days, 0);
    
    /// <summary>
    /// Determines if the goal is overdue
    /// </summary>
    public bool IsOverdue => DateTime.Now > TargetDate && !IsCompleted;
    
    /// <summary>
    /// Required monthly savings to achieve the goal
    /// </summary>
    public decimal RequiredMonthlySavings
    {
        get
        {
            if (IsCompleted || DaysRemaining <= 0) return 0;
            var monthsRemaining = Math.Max(DaysRemaining / 30.0m, 1);
            return RemainingAmount / monthsRemaining;
        }
    }
    
    /// <summary>
    /// Goal status based on progress and timeline
    /// </summary>
    public GoalStatus Status
    {
        get
        {
            if (IsCompleted) return GoalStatus.Completed;
            if (IsOverdue) return GoalStatus.Overdue;
            
            var progressPercentage = ProgressPercentage;
            var timePercentage = DaysRemaining > 0 ? 
                (1.0 - (double)DaysRemaining / Math.Max((TargetDate - DateTime.Now.AddDays(-365)).Days, 1)) * 100 : 100;
            
            if (progressPercentage >= timePercentage * 0.8) return GoalStatus.OnTrack;
            if (progressPercentage >= timePercentage * 0.5) return GoalStatus.Behind;
            return GoalStatus.AtRisk;
        }
    }
    
    /// <summary>
    /// Status color for UI display
    /// </summary>
    public string StatusColor => Status switch
    {
        GoalStatus.Completed => "#10B981", // Green
        GoalStatus.OnTrack => "#3B82F6", // Blue
        GoalStatus.Behind => "#F59E0B", // Yellow
        GoalStatus.AtRisk => "#F97316", // Orange
        GoalStatus.Overdue => "#EF4444", // Red
        _ => "#6B7280" // Gray
    };
    
    /// <summary>
    /// Status text for display
    /// </summary>
    public string StatusText => Status switch
    {
        GoalStatus.Completed => "Completed",
        GoalStatus.OnTrack => "On Track",
        GoalStatus.Behind => "Behind",
        GoalStatus.AtRisk => "At Risk",
        GoalStatus.Overdue => "Overdue",
        _ => "Unknown"
    };
    
    /// <summary>
    /// Priority text for display
    /// </summary>
    public string PriorityText => Priority switch
    {
        1 => "High",
        2 => "Medium-High",
        3 => "Medium",
        4 => "Medium-Low",
        5 => "Low",
        _ => "Medium"
    };
    
    /// <summary>
    /// Priority color for display
    /// </summary>
    public string PriorityColor => Priority switch
    {
        1 => "#EF4444", // Red
        2 => "#F97316", // Orange
        3 => "#F59E0B", // Yellow
        4 => "#3B82F6", // Blue
        5 => "#6B7280", // Gray
        _ => "#F59E0B"
    };
    
    /// <summary>
    /// Formatted target date for display
    /// </summary>
    public string TargetDateText => TargetDate.ToString("MMM dd, yyyy");
    
    /// <summary>
    /// Days remaining text for display
    /// </summary>
    public string DaysRemainingText
    {
        get
        {
            if (IsCompleted) return "Completed";
            if (IsOverdue) return $"{Math.Abs(DaysRemaining)} days overdue";
            if (DaysRemaining == 0) return "Due today";
            if (DaysRemaining == 1) return "1 day left";
            return $"{DaysRemaining} days left";
        }
    }
    
    /// <summary>
    /// Next milestone that hasn't been achieved
    /// </summary>
    public GoalMilestone? NextMilestone => Milestones
        .Where(m => !m.IsAchieved && m.TargetAmount > CurrentAmount)
        .OrderBy(m => m.TargetAmount)
        .FirstOrDefault();
    
    /// <summary>
    /// Recent milestone achievements (last 7 days)
    /// </summary>
    public IEnumerable<GoalMilestone> RecentAchievements => Milestones
        .Where(m => m.IsAchieved && m.AchievedDate.HasValue && 
                   m.AchievedDate.Value >= DateTime.Now.AddDays(-7))
        .OrderByDescending(m => m.AchievedDate);
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

/// <summary>
/// Goal milestone model for UI display
/// </summary>
public class GoalMilestone
{
    public int Id { get; set; }
    public int GoalId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal TargetAmount { get; set; }
    public bool IsAchieved { get; set; }
    public DateTime? AchievedDate { get; set; }
    
    /// <summary>
    /// Achievement date text for display
    /// </summary>
    public string AchievedDateText => AchievedDate?.ToString("MMM dd, yyyy") ?? "";
    
    /// <summary>
    /// Status icon for display
    /// </summary>
    public string StatusIcon => IsAchieved ? "✅" : "⭕";
}

/// <summary>
/// Goal status enumeration
/// </summary>
public enum GoalStatus
{
    OnTrack,
    Behind,
    AtRisk,
    Overdue,
    Completed
}

/// <summary>
/// Goal statistics summary
/// </summary>
public class GoalStatistics
{
    public int TotalGoals { get; set; }
    public int ActiveGoals { get; set; }
    public int CompletedGoals { get; set; }
    public int OverdueGoals { get; set; }
    public decimal TotalTargetAmount { get; set; }
    public decimal TotalCurrentAmount { get; set; }
    public decimal TotalRemainingAmount { get; set; }
    public double OverallProgressPercentage { get; set; }
    public int RecentMilestones { get; set; }
    
    /// <summary>
    /// Completion rate percentage
    /// </summary>
    public double CompletionRate => TotalGoals > 0 ? (double)CompletedGoals / TotalGoals * 100 : 0;
}