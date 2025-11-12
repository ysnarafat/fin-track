using FinTrack.Core.Entities;

namespace FinTrack.Maui.Models;

/// <summary>
/// Budget model for UI display and data binding
/// </summary>
public class BudgetModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryColor { get; set; } = "#6B7280";
    public decimal BudgetLimit { get; set; }
    public decimal CurrentSpending { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    
    /// <summary>
    /// Remaining budget amount
    /// </summary>
    public decimal RemainingAmount => BudgetLimit - CurrentSpending;
    
    /// <summary>
    /// Budget utilization percentage (0-100)
    /// </summary>
    public double UtilizationPercentage => BudgetLimit > 0 ? (double)(CurrentSpending / BudgetLimit * 100) : 0;
    
    /// <summary>
    /// Budget status based on spending
    /// </summary>
    public BudgetStatus Status
    {
        get
        {
            var percentage = UtilizationPercentage;
            if (percentage >= 100) return BudgetStatus.Exceeded;
            if (percentage >= 80) return BudgetStatus.Warning;
            if (percentage >= 50) return BudgetStatus.OnTrack;
            return BudgetStatus.Good;
        }
    }
    
    /// <summary>
    /// Status color for UI display
    /// </summary>
    public string StatusColor => Status switch
    {
        BudgetStatus.Good => "#10B981", // Green
        BudgetStatus.OnTrack => "#F59E0B", // Yellow
        BudgetStatus.Warning => "#F97316", // Orange
        BudgetStatus.Exceeded => "#EF4444", // Red
        _ => "#6B7280" // Gray
    };
    
    /// <summary>
    /// Status text for display
    /// </summary>
    public string StatusText => Status switch
    {
        BudgetStatus.Good => "Good",
        BudgetStatus.OnTrack => "On Track",
        BudgetStatus.Warning => "Warning",
        BudgetStatus.Exceeded => "Exceeded",
        _ => "Unknown"
    };
    
    /// <summary>
    /// Indicates if the budget period is currently active
    /// </summary>
    public bool IsCurrentPeriod
    {
        get
        {
            var now = DateTime.Today;
            return now >= StartDate.Date && now <= EndDate.Date;
        }
    }
}

/// <summary>
/// Budget status enumeration
/// </summary>
public enum BudgetStatus
{
    Good,
    OnTrack,
    Warning,
    Exceeded
}

/// <summary>
/// Budget summary data for dashboard display
/// </summary>
public class BudgetSummary
{
    public decimal TotalBudgeted { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal TotalRemaining => TotalBudgeted - TotalSpent;
    public int ActiveBudgets { get; set; }
    public int ExceededBudgets { get; set; }
    public int WarningBudgets { get; set; }
    
    /// <summary>
    /// Overall budget utilization percentage
    /// </summary>
    public double OverallUtilization => TotalBudgeted > 0 ? (double)(TotalSpent / TotalBudgeted * 100) : 0;
}