using FinTrack.Core.Entities;
using FinTrack.Core.Enums;
using FinTrack.Core.Interfaces;

namespace FinTrack.Shared.Services;

/// <summary>
/// Service interface for budget business logic and operations
/// </summary>
public interface IBudgetService
{
    /// <summary>
    /// Creates a new budget with business logic validation
    /// </summary>
    /// <param name="budget">Budget to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created budget</returns>
    Task<Budget> CreateBudgetAsync(Budget budget, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates an existing budget with business logic validation
    /// </summary>
    /// <param name="budget">Budget to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated budget</returns>
    Task<Budget> UpdateBudgetAsync(Budget budget, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes a budget after validating it can be deleted
    /// </summary>
    /// <param name="budgetId">Budget ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeleteBudgetAsync(int budgetId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a budget by ID
    /// </summary>
    /// <param name="budgetId">Budget ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Budget if found, null otherwise</returns>
    Task<Budget?> GetBudgetAsync(int budgetId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all active budgets
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of active budgets</returns>
    Task<IEnumerable<Budget>> GetActiveBudgetsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets current budgets (budgets active in the current period)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of current budgets</returns>
    Task<IEnumerable<Budget>> GetCurrentBudgetsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets budgets by period type
    /// </summary>
    /// <param name="period">Budget period</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of budgets for the specified period</returns>
    Task<IEnumerable<Budget>> GetBudgetsByPeriodAsync(BudgetPeriod period, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets budgets for a specific category
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of budgets for the category</returns>
    Task<IEnumerable<Budget>> GetBudgetsByCategoryAsync(int categoryId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets budgets that have exceeded their limits
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of exceeded budgets</returns>
    Task<IEnumerable<Budget>> GetExceededBudgetsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets budgets that have reached their alert threshold
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of budgets at alert threshold</returns>
    Task<IEnumerable<Budget>> GetBudgetsAtAlertThresholdAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Recalculates spent amounts for all budgets based on transactions
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of budgets updated</returns>
    Task<int> RecalculateAllSpentAmountsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Recalculates spent amount for a specific budget based on transactions
    /// </summary>
    /// <param name="budgetId">Budget ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if recalculated successfully</returns>
    Task<bool> RecalculateSpentAmountAsync(int budgetId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets budget performance summary for a date range
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of budget performance summaries</returns>
    Task<IEnumerable<BudgetPerformance>> GetBudgetPerformanceAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets budget utilization statistics
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Budget utilization statistics</returns>
    Task<BudgetUtilizationStats> GetUtilizationStatsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Creates budgets for the next period based on existing budgets
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of newly created budgets for the next period</returns>
    Task<IEnumerable<Budget>> CreateNextPeriodBudgetsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets budget alerts for budgets that need attention
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of budget alerts</returns>
    Task<IEnumerable<BudgetAlert>> GetBudgetAlertsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Searches budgets by name
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of matching budgets</returns>
    Task<IEnumerable<Budget>> SearchBudgetsAsync(string searchTerm, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Activates or deactivates a budget
    /// </summary>
    /// <param name="budgetId">Budget ID</param>
    /// <param name="isActive">Active status</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if updated successfully</returns>
    Task<bool> SetBudgetActiveStatusAsync(int budgetId, bool isActive, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Validates a budget against business rules
    /// </summary>
    /// <param name="budget">Budget to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result</returns>
    Task<FinTrack.Shared.Models.BusinessValidationResult> ValidateBudgetAsync(Budget budget, CancellationToken cancellationToken = default);
}

/// <summary>
/// Budget alert information
/// </summary>
public class BudgetAlert
{
    public Budget Budget { get; set; } = null!;
    public BudgetAlertType AlertType { get; set; }
    public string Message { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
    public DateTime AlertDate { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Types of budget alerts
/// </summary>
public enum BudgetAlertType
{
    /// <summary>
    /// Budget has reached the alert threshold
    /// </summary>
    ThresholdReached,
    
    /// <summary>
    /// Budget has been exceeded
    /// </summary>
    BudgetExceeded,
    
    /// <summary>
    /// Budget is projected to be exceeded based on current spending rate
    /// </summary>
    ProjectedOverspend,
    
    /// <summary>
    /// Budget period is ending soon
    /// </summary>
    PeriodEnding,
    
    /// <summary>
    /// Budget has no spending activity
    /// </summary>
    NoActivity
}

