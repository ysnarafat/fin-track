namespace FinTrack.Core.Entities;

/// <summary>
/// Represents a financial goal that users can set and track progress towards
/// </summary>
public class Goal : BaseEntity
{
    /// <summary>
    /// Name of the financial goal
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Detailed description of the goal
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Target amount to achieve
    /// </summary>
    public decimal TargetAmount { get; set; }
    
    /// <summary>
    /// Current amount saved towards the goal
    /// </summary>
    public decimal CurrentAmount { get; set; }
    
    /// <summary>
    /// Target date to achieve the goal
    /// </summary>
    public DateTime TargetDate { get; set; }
    
    /// <summary>
    /// Priority level of the goal (1 = highest, 5 = lowest)
    /// </summary>
    public int Priority { get; set; } = 3;
    
    /// <summary>
    /// Category or type of the goal (e.g., "Emergency Fund", "Vacation", "Car")
    /// </summary>
    public string Category { get; set; } = string.Empty;
    
    /// <summary>
    /// Color associated with the goal for visual representation
    /// </summary>
    public string Color { get; set; } = "#3B82F6";
    
    /// <summary>
    /// Whether the goal has been achieved
    /// </summary>
    public bool IsCompleted { get; set; }
    
    /// <summary>
    /// Date when the goal was completed
    /// </summary>
    public DateTime? CompletedDate { get; set; }
    
    /// <summary>
    /// Collection of milestones for this goal
    /// </summary>
    public List<GoalMilestone> Milestones { get; set; } = new();
    
    /// <summary>
    /// Calculates the progress percentage towards the goal
    /// </summary>
    public decimal ProgressPercentage => TargetAmount > 0 ? Math.Min((CurrentAmount / TargetAmount) * 100, 100) : 0;
    
    /// <summary>
    /// Calculates the remaining amount needed to achieve the goal
    /// </summary>
    public decimal RemainingAmount => Math.Max(TargetAmount - CurrentAmount, 0);
    
    /// <summary>
    /// Calculates days remaining until target date
    /// </summary>
    public int DaysRemaining => Math.Max((TargetDate - DateTime.Now).Days, 0);
    
    /// <summary>
    /// Determines if the goal is overdue
    /// </summary>
    public bool IsOverdue => DateTime.Now > TargetDate && !IsCompleted;
    
    /// <summary>
    /// Calculates the required monthly savings to achieve the goal
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
    /// Updates the current amount and checks for milestone achievements
    /// </summary>
    public void UpdateProgress(decimal newAmount)
    {
        var previousAmount = CurrentAmount;
        CurrentAmount = newAmount;
        MarkAsModified();
        
        // Check if goal is completed
        if (CurrentAmount >= TargetAmount && !IsCompleted)
        {
            IsCompleted = true;
            CompletedDate = DateTime.UtcNow;
        }
        
        // Check for milestone achievements
        foreach (var milestone in Milestones.Where(m => !m.IsAchieved))
        {
            if (CurrentAmount >= milestone.TargetAmount)
            {
                milestone.MarkAsAchieved();
            }
        }
    }
    
    /// <summary>
    /// Adds a milestone to the goal
    /// </summary>
    public void AddMilestone(string name, decimal targetAmount, string description = "")
    {
        var milestone = new GoalMilestone
        {
            GoalId = Id,
            Name = name,
            TargetAmount = targetAmount,
            Description = description
        };
        
        Milestones.Add(milestone);
        MarkAsModified();
    }
}

/// <summary>
/// Represents a milestone within a financial goal
/// </summary>
public class GoalMilestone : BaseEntity
{
    /// <summary>
    /// ID of the parent goal
    /// </summary>
    public int GoalId { get; set; }
    
    /// <summary>
    /// Name of the milestone
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Description of the milestone
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Target amount for this milestone
    /// </summary>
    public decimal TargetAmount { get; set; }
    
    /// <summary>
    /// Whether this milestone has been achieved
    /// </summary>
    public bool IsAchieved { get; set; }
    
    /// <summary>
    /// Date when the milestone was achieved
    /// </summary>
    public DateTime? AchievedDate { get; set; }
    
    /// <summary>
    /// Navigation property to the parent goal
    /// </summary>
    public Goal Goal { get; set; } = null!;
    
    /// <summary>
    /// Marks the milestone as achieved
    /// </summary>
    public void MarkAsAchieved()
    {
        IsAchieved = true;
        AchievedDate = DateTime.UtcNow;
        MarkAsModified();
    }
}