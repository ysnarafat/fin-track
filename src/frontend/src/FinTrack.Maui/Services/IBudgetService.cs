using FinTrack.Maui.Models;

namespace FinTrack.Maui.Services;

/// <summary>
/// Service interface for budget management operations
/// </summary>
public interface IBudgetService
{
    /// <summary>
    /// Gets all active budgets
    /// </summary>
    Task<IEnumerable<Budget>> GetBudgetsAsync();
    
    /// <summary>
    /// Gets budgets for the current month
    /// </summary>
    Task<IEnumerable<Budget>> GetCurrentMonthBudgetsAsync();
    
    /// <summary>
    /// Gets a specific budget by ID
    /// </summary>
    Task<Budget?> GetBudgetAsync(int id);
    
    /// <summary>
    /// Creates a new budget
    /// </summary>
    Task<Budget> CreateBudgetAsync(Budget budget);
    
    /// <summary>
    /// Updates an existing budget
    /// </summary>
    Task<Budget> UpdateBudgetAsync(Budget budget);
    
    /// <summary>
    /// Deletes a budget
    /// </summary>
    Task<bool> DeleteBudgetAsync(int id);
    
    /// <summary>
    /// Gets budget summary for dashboard
    /// </summary>
    Task<BudgetSummary> GetBudgetSummaryAsync();
    
    /// <summary>
    /// Gets available categories for budget creation
    /// </summary>
    Task<IEnumerable<CategoryOption>> GetAvailableCategoriesAsync();
    
    /// <summary>
    /// Checks if a budget alert should be shown
    /// </summary>
    Task<IEnumerable<BudgetAlert>> GetBudgetAlertsAsync();
}

/// <summary>
/// Category option for budget creation
/// </summary>
public class CategoryOption
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#6B7280";
    public string Icon { get; set; } = string.Empty;
    public bool HasExistingBudget { get; set; }
}

/// <summary>
/// Budget alert for notifications
/// </summary>
public class BudgetAlert
{
    public int BudgetId { get; set; }
    public string BudgetName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public BudgetAlertType Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public decimal CurrentSpending { get; set; }
    public decimal BudgetLimit { get; set; }
    public double UtilizationPercentage { get; set; }
}

/// <summary>
/// Budget alert types
/// </summary>
public enum BudgetAlertType
{
    Warning, // 80% threshold
    Exceeded // 100% threshold
}