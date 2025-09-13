using FinTrack.Core.Enums;

namespace FinTrack.Shared.DTOs;

/// <summary>
/// Data transfer object for Goal entity
/// </summary>
public class GoalDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; }
    public DateTime TargetDate { get; set; }
    public int Priority { get; set; } = 3;
    public GoalType Type { get; set; }
    public string Color { get; set; } = "#3B82F6";
    public bool IsCompleted { get; set; }
    public DateTime? CompletedDate { get; set; }
    public int? LinkedAccountId { get; set; }
    
    // Navigation properties for display
    public string? LinkedAccountName { get; set; }
    public List<GoalMilestoneDto> Milestones { get; set; } = new();
    
    // Calculated properties
    public decimal ProgressPercentage => TargetAmount > 0 ? Math.Min((CurrentAmount / TargetAmount) * 100, 100) : 0;
    public decimal RemainingAmount => Math.Max(TargetAmount - CurrentAmount, 0);
    public int DaysRemaining => Math.Max((TargetDate - DateTime.Now).Days, 0);
    public bool IsOverdue => DateTime.Now > TargetDate && !IsCompleted;
}

/// <summary>
/// Data transfer object for GoalMilestone entity
/// </summary>
public class GoalMilestoneDto
{
    public int Id { get; set; }
    public int GoalId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal TargetAmount { get; set; }
    public DateTime? TargetDate { get; set; }
    public int SortOrder { get; set; }
    public bool IsAchieved { get; set; }
    public DateTime? AchievedDate { get; set; }
}

/// <summary>
/// DTO for creating a new goal
/// </summary>
public class CreateGoalDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal TargetAmount { get; set; }
    public DateTime TargetDate { get; set; } = DateTime.Today.AddYears(1);
    public int Priority { get; set; } = 3;
    public GoalType Type { get; set; } = GoalType.Savings;
    public string Color { get; set; } = "#3B82F6";
    public int? LinkedAccountId { get; set; }
}

/// <summary>
/// DTO for updating an existing goal
/// </summary>
public class UpdateGoalDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; }
    public DateTime TargetDate { get; set; }
    public int Priority { get; set; }
    public GoalType Type { get; set; }
    public string Color { get; set; } = "#3B82F6";
    public int? LinkedAccountId { get; set; }
}