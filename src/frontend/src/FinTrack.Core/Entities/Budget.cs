using FinTrack.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace FinTrack.Core.Entities;

/// <summary>
/// Represents a budget for tracking spending limits and financial planning
/// </summary>
public class Budget : BaseEntity
{
    /// <summary>
    /// Name of the budget
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional description of the budget
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Budget amount limit
    /// </summary>
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Budget amount must be greater than 0")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Budget period (Monthly, Quarterly, Annual, etc.)
    /// </summary>
    [Required]
    public BudgetPeriod Period { get; set; }

    /// <summary>
    /// Start date of the budget period
    /// </summary>
    [Required]
    public DateTime StartDate { get; set; }

    /// <summary>
    /// End date of the budget period
    /// </summary>
    [Required]
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Category this budget applies to (optional - null means all categories)
    /// </summary>
    public int? CategoryId { get; set; }

    /// <summary>
    /// Current amount spent against this budget
    /// </summary>
    public decimal SpentAmount { get; set; } = 0;

    /// <summary>
    /// Indicates if the budget is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Alert threshold percentage (0-100) for notifications
    /// </summary>
    [Range(0, 100)]
    public decimal? AlertThreshold { get; set; } = 80;

    /// <summary>
    /// Color for visual representation (hex format)
    /// </summary>
    [StringLength(7)]
    public string? Color { get; set; } = "#3B82F6";

    /// <summary>
    /// Indicates if budget alerts are enabled
    /// </summary>
    public bool AlertsEnabled { get; set; } = true;

    /// <summary>
    /// Indicates if the budget should roll over unused amounts to the next period
    /// </summary>
    public bool RolloverEnabled { get; set; } = false;

    /// <summary>
    /// Amount rolled over from the previous period
    /// </summary>
    public decimal RolloverAmount { get; set; } = 0;

    // Navigation properties

    /// <summary>
    /// Navigation property to the category this budget applies to
    /// </summary>
    public virtual Category? Category { get; set; }

    /// <summary>
    /// Constructor that initializes default values
    /// </summary>
    public Budget()
    {
        Period = BudgetPeriod.Monthly;
        StartDate = DateTime.Today;
        EndDate = DateTime.Today.AddMonths(1).AddDays(-1);
        IsActive = true;
        AlertThreshold = 80;
        Color = "#3B82F6";
        AlertsEnabled = true;
        RolloverEnabled = false;
    }

    /// <summary>
    /// Gets the total available budget amount (including rollover)
    /// </summary>
    public decimal TotalBudgetAmount => Amount + RolloverAmount;

    /// <summary>
    /// Gets the remaining budget amount
    /// </summary>
    public decimal RemainingAmount => Math.Max(TotalBudgetAmount - SpentAmount, 0);

    /// <summary>
    /// Gets the budget utilization percentage
    /// </summary>
    public decimal UtilizationPercentage => TotalBudgetAmount > 0 ? (SpentAmount / TotalBudgetAmount) * 100 : 0;

    /// <summary>
    /// Indicates if the budget has been exceeded
    /// </summary>
    public bool IsExceeded => SpentAmount > TotalBudgetAmount;

    /// <summary>
    /// Indicates if the budget has reached the alert threshold
    /// </summary>
    public bool HasReachedAlertThreshold => AlertThreshold.HasValue && UtilizationPercentage >= AlertThreshold.Value;

    /// <summary>
    /// Gets the number of days remaining in the budget period
    /// </summary>
    public int DaysRemaining => Math.Max((EndDate - DateTime.Today).Days + 1, 0);

    /// <summary>
    /// Indicates if the budget period is currently active
    /// </summary>
    public bool IsCurrentPeriod => DateTime.Today >= StartDate && DateTime.Today <= EndDate;

    /// <summary>
    /// Gets the daily spending rate based on current spending
    /// </summary>
    public decimal DailySpendingRate
    {
        get
        {
            var daysElapsed = Math.Max((DateTime.Today - StartDate).Days + 1, 1);
            return SpentAmount / daysElapsed;
        }
    }

    /// <summary>
    /// Gets the projected spending for the entire budget period
    /// </summary>
    public decimal ProjectedSpending
    {
        get
        {
            if (!IsCurrentPeriod) return SpentAmount;

            var totalDays = (EndDate - StartDate).Days + 1;
            return DailySpendingRate * totalDays;
        }
    }

    /// <summary>
    /// Updates the spent amount and marks the budget as modified
    /// </summary>
    /// <param name="amount">Amount to add to spent amount</param>
    public void AddSpending(decimal amount)
    {
        if (amount < 0)
            throw new ArgumentException("Spending amount cannot be negative", nameof(amount));

        SpentAmount += amount;
        MarkAsModified();
    }

    /// <summary>
    /// Removes spending from the budget (for transaction deletions/modifications)
    /// </summary>
    /// <param name="amount">Amount to subtract from spent amount</param>
    public void RemoveSpending(decimal amount)
    {
        if (amount < 0)
            throw new ArgumentException("Spending amount cannot be negative", nameof(amount));

        SpentAmount = Math.Max(SpentAmount - amount, 0);
        MarkAsModified();
    }

    /// <summary>
    /// Resets the budget for a new period
    /// </summary>
    /// <param name="newStartDate">Start date of the new period</param>
    /// <param name="newEndDate">End date of the new period</param>
    public void ResetForNewPeriod(DateTime newStartDate, DateTime newEndDate)
    {
        if (RolloverEnabled && RemainingAmount > 0)
        {
            RolloverAmount = RemainingAmount;
        }
        else
        {
            RolloverAmount = 0;
        }

        StartDate = newStartDate;
        EndDate = newEndDate;
        SpentAmount = 0;
        MarkAsModified();
    }

    /// <summary>
    /// Validates the budget data
    /// </summary>
    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(Name)) return false;
        if (Amount <= 0) return false;
        if (StartDate >= EndDate) return false;
        if (AlertThreshold.HasValue && (AlertThreshold.Value < 0 || AlertThreshold.Value > 100)) return false;
        if (!string.IsNullOrEmpty(Color) && !IsValidHexColor(Color)) return false;
        if (SpentAmount < 0) return false;
        if (RolloverAmount < 0) return false;

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

    /// <summary>
    /// Creates a budget for the next period based on this budget's settings
    /// </summary>
    public Budget CreateNextPeriodBudget()
    {
        var (nextStart, nextEnd) = CalculateNextPeriodDates();

        var nextBudget = new Budget
        {
            Name = Name,
            Description = Description,
            Amount = Amount,
            Period = Period,
            StartDate = nextStart,
            EndDate = nextEnd,
            CategoryId = CategoryId,
            IsActive = IsActive,
            AlertThreshold = AlertThreshold,
            Color = Color,
            AlertsEnabled = AlertsEnabled,
            RolloverEnabled = RolloverEnabled,
            RolloverAmount = RolloverEnabled ? RemainingAmount : 0
        };

        return nextBudget;
    }

    /// <summary>
    /// Calculates the start and end dates for the next budget period
    /// </summary>
    private (DateTime start, DateTime end) CalculateNextPeriodDates()
    {
        return Period switch
        {
            BudgetPeriod.Weekly => (EndDate.AddDays(1), EndDate.AddDays(7)),
            BudgetPeriod.Monthly => (EndDate.AddDays(1), EndDate.AddMonths(1)),
            BudgetPeriod.Quarterly => (EndDate.AddDays(1), EndDate.AddMonths(3)),
            BudgetPeriod.Annual => (EndDate.AddDays(1), EndDate.AddYears(1)),
            BudgetPeriod.Custom => (EndDate.AddDays(1), EndDate.AddDays((EndDate - StartDate).Days + 1)),
            _ => throw new InvalidOperationException($"Unsupported budget period: {Period}")
        };
    }
}