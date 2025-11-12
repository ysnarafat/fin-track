using FinTrack.Core.Entities;
using FinTrack.Core.Enums;

namespace FinTrack.Core.Interfaces;

/// <summary>
/// Repository interface for Budget-specific operations
/// </summary>
public interface IBudgetRepository : IRepository<Budget>
{
    /// <summary>
    /// Gets budgets by period type
    /// </summary>
    /// <param name="period">Budget period</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of budgets for the specified period</returns>
    Task<IEnumerable<Budget>> GetByPeriodAsync(BudgetPeriod period, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets active budgets only
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of active budgets</returns>
    Task<IEnumerable<Budget>> GetActiveAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets budgets for a specific category
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of budgets for the category</returns>
    Task<IEnumerable<Budget>> GetByCategoryAsync(int categoryId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets current budgets (budgets that are active in the current date)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of current budgets</returns>
    Task<IEnumerable<Budget>> GetCurrentBudgetsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets budgets for a specific date range
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of budgets that overlap with the date range</returns>
    Task<IEnumerable<Budget>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    
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
    /// Updates spent amount for a budget
    /// </summary>
    /// <param name="budgetId">Budget ID</param>
    /// <param name="spentAmount">New spent amount</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if updated successfully</returns>
    Task<bool> UpdateSpentAmountAsync(int budgetId, decimal spentAmount, CancellationToken cancellationToken = default);
    
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
    /// Gets budgets with their category information
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of budgets with category details</returns>
    Task<IEnumerable<Budget>> GetWithCategoryAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Creates budgets for the next period based on existing budgets
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of newly created budgets for the next period</returns>
    Task<IEnumerable<Budget>> CreateNextPeriodBudgetsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets budget utilization statistics
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Budget utilization statistics</returns>
    Task<BudgetUtilizationStats> GetUtilizationStatsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Searches budgets by name
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of matching budgets</returns>
    Task<IEnumerable<Budget>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets budgets that overlap with the specified date range for a category
    /// </summary>
    /// <param name="categoryId">Category ID (null for all categories)</param>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of overlapping budgets</returns>
    Task<IEnumerable<Budget>> GetOverlappingBudgetsAsync(int? categoryId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}

/// <summary>
/// Budget performance information
/// </summary>
public class BudgetPerformance
{
    public Budget Budget { get; set; } = null!;
    public decimal UtilizationPercentage { get; set; }
    public decimal RemainingAmount { get; set; }
    public decimal ProjectedSpending { get; set; }
    public int DaysRemaining { get; set; }
    public bool IsOnTrack { get; set; }
    public decimal DailySpendingRate { get; set; }
    public decimal RecommendedDailySpending { get; set; }
}

/// <summary>
/// Budget utilization statistics
/// </summary>
public class BudgetUtilizationStats
{
    public int TotalBudgets { get; set; }
    public int ActiveBudgets { get; set; }
    public int ExceededBudgets { get; set; }
    public int BudgetsAtAlert { get; set; }
    public decimal TotalBudgetAmount { get; set; }
    public decimal TotalSpentAmount { get; set; }
    public decimal AverageUtilization { get; set; }
    public decimal TotalRemainingAmount { get; set; }
}