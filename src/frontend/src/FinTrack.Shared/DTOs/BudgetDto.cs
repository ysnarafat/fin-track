using FinTrack.Core.Enums;

namespace FinTrack.Shared.DTOs;

/// <summary>
/// Data transfer object for Budget entity
/// </summary>
public class BudgetDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public BudgetPeriod Period { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? CategoryId { get; set; }
    public decimal SpentAmount { get; set; }
    public bool IsActive { get; set; } = true;
    public decimal? AlertThreshold { get; set; }
    public string? Color { get; set; }
    public bool AlertsEnabled { get; set; } = true;
    public bool RolloverEnabled { get; set; }
    public decimal RolloverAmount { get; set; }
    
    // Navigation properties for display
    public string? CategoryName { get; set; }
    
    // Calculated properties
    public decimal TotalBudgetAmount => Amount + RolloverAmount;
    public decimal RemainingAmount => Math.Max(TotalBudgetAmount - SpentAmount, 0);
    public decimal UtilizationPercentage => TotalBudgetAmount > 0 ? (SpentAmount / TotalBudgetAmount) * 100 : 0;
    public bool IsExceeded => SpentAmount > TotalBudgetAmount;
}

/// <summary>
/// DTO for creating a new budget
/// </summary>
public class CreateBudgetDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public BudgetPeriod Period { get; set; } = BudgetPeriod.Monthly;
    public DateTime StartDate { get; set; } = DateTime.Today;
    public DateTime EndDate { get; set; } = DateTime.Today.AddMonths(1).AddDays(-1);
    public int? CategoryId { get; set; }
    public decimal? AlertThreshold { get; set; } = 80;
    public string? Color { get; set; } = "#3B82F6";
    public bool AlertsEnabled { get; set; } = true;
    public bool RolloverEnabled { get; set; }
}

/// <summary>
/// DTO for updating an existing budget
/// </summary>
public class UpdateBudgetDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public BudgetPeriod Period { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? CategoryId { get; set; }
    public bool IsActive { get; set; }
    public decimal? AlertThreshold { get; set; }
    public string? Color { get; set; }
    public bool AlertsEnabled { get; set; }
    public bool RolloverEnabled { get; set; }
}