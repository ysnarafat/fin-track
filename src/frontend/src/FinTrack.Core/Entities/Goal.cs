using FinTrack.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace FinTrack.Core.Entities;

/// <summary>
/// Represents a financial goal that users can set and track progress towards
/// </summary>
public class Goal : BaseEntity
{
    /// <summary>
    /// Name of the financial goal
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Detailed description of the goal
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Target amount to achieve
    /// </summary>
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Target amount must be greater than 0")]
    public decimal TargetAmount { get; set; }
    
    /// <summary>
    /// Current amount saved towards the goal
    /// </summary>
    public decimal CurrentAmount { get; set; } = 0;
    
    /// <summary>
    /// Target date to achieve the goal
    /// </summary>
    [Required]
    public DateTime TargetDate { get; set; }
    
    /// <summary>
    /// Priority level of the goal (1 = highest, 5 = lowest)
    /// </summary>
    [Range(1, 5)]
    public int Priority { get; set; } = 3;
    
    /// <summary>
    /// Type of the goal (Savings, DebtPayoff, Investment, etc.)
    /// </summary>
    [Required]
    public GoalType Type { get; set; }
    
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
    /// Account linked to this goal for automatic progress tracking
    /// </summary>
    public int? LinkedAccountId { get; set; }
    
    // Navigation properties
    
    /// <summary>
    /// Navigation property to the linked account
    /// </summary>
    public virtual Account? LinkedAccount { get; set; }
    
    /// <summary>
    /// Collection of milestones for this goal
    /// </summary>
    public virtual ICollection<GoalMilestone> Milestones { get; set; } = new List<GoalMilestone>();
    
    /// <summary>
    /// Constructor that initializes default values
    /// </summary>
    public Goal()
    {
        Type = GoalType.Savings;
        Priority = 3;
        Color = "#3B82F6";
        IsCompleted = false;
        CurrentAmount = 0;
        TargetDate = DateTime.Today.AddYears(1);
    }
    
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
    
    /// <summary>
    /// Validates the goal data
    /// </summary>
    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(Name)) return false;
        if (TargetAmount <= 0) return false;
        if (CurrentAmount < 0) return false;
        if (TargetDate <= DateTime.Today) return false;
        if (Priority < 1 || Priority > 5) return false;
        if (!string.IsNullOrEmpty(Color) && !IsValidHexColor(Color)) return false;
        
        return true;
    }
    
    /// <summary>
    /// Validates if a string is a valid hex color
    /// </summary>
    private static bool IsValidHexColor(string color)
    {
        if (string.IsNullOrEmpty(color)) return false;
        if (!color.StartsWith("#")) return false;
        if (color.Length != 7) return false;
        
        return color[1..].All(c => char.IsDigit(c) || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f'));
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
    /// Target date for achieving this milestone
    /// </summary>
    public DateTime? TargetDate { get; set; }
    
    /// <summary>
    /// Sort order for displaying milestones
    /// </summary>
    public int SortOrder { get; set; }
    
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