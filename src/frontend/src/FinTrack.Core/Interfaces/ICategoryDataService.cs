using FinTrack.Core.Entities;

namespace FinTrack.Core.Interfaces;

/// <summary>
/// Data service interface for Category-specific business operations
/// </summary>
public interface ICategoryDataService : IDataService<Category>
{
    /// <summary>
    /// Gets the complete category hierarchy with spending data
    /// </summary>
    /// <param name="includeInactive">Whether to include inactive categories</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of categories with hierarchy and spending data</returns>
    Task<IEnumerable<CategoryWithSpending>> GetCategoryHierarchyAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Creates a new category with validation
    /// </summary>
    /// <param name="category">Category to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created category</returns>
    Task<Category> CreateCategoryAsync(Category category, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates a category with hierarchy validation
    /// </summary>
    /// <param name="category">Category to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated category</returns>
    Task<Category> UpdateCategoryAsync(Category category, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes a category and handles transaction reassignment
    /// </summary>
    /// <param name="categoryId">Category ID to delete</param>
    /// <param name="replacementCategoryId">Category ID to reassign transactions to</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeleteCategoryAsync(int categoryId, int replacementCategoryId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Moves a category to a new parent with validation
    /// </summary>
    /// <param name="categoryId">Category ID to move</param>
    /// <param name="newParentId">New parent category ID (null for root)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated category</returns>
    Task<Category?> MoveCategoryAsync(int categoryId, int? newParentId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Reorders categories within the same parent
    /// </summary>
    /// <param name="categoryOrders">Dictionary of category ID to new sort order</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of categories updated</returns>
    Task<int> ReorderCategoriesAsync(Dictionary<int, int> categoryOrders, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets category spending analysis for a date range
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="includeSubcategories">Whether to include subcategory spending</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of category spending analysis</returns>
    Task<IEnumerable<CategorySpendingAnalysis>> GetSpendingAnalysisAsync(DateTime startDate, DateTime endDate, bool includeSubcategories = true, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets budget performance for categories with budget limits
    /// </summary>
    /// <param name="year">Year</param>
    /// <param name="month">Month (1-12)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of budget performance data</returns>
    Task<IEnumerable<CategoryBudgetPerformance>> GetBudgetPerformanceAsync(int year, int month, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets trending categories based on spending changes
    /// </summary>
    /// <param name="currentPeriodStart">Current period start date</param>
    /// <param name="currentPeriodEnd">Current period end date</param>
    /// <param name="previousPeriodStart">Previous period start date</param>
    /// <param name="previousPeriodEnd">Previous period end date</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of category trends</returns>
    Task<IEnumerable<CategoryTrend>> GetCategoryTrendsAsync(DateTime currentPeriodStart, DateTime currentPeriodEnd, DateTime previousPeriodStart, DateTime previousPeriodEnd, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Creates default system categories
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of created system categories</returns>
    Task<IEnumerable<Category>> CreateDefaultCategoriesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets category usage statistics
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    /// <param name="startDate">Start date for statistics</param>
    /// <param name="endDate">End date for statistics</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Category usage statistics</returns>
    Task<CategoryUsageStats> GetCategoryUsageStatsAsync(int categoryId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Merges two categories (moves all transactions from source to target)
    /// </summary>
    /// <param name="sourceCategoryId">Source category ID (will be deleted)</param>
    /// <param name="targetCategoryId">Target category ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Target category with merged data</returns>
    Task<Category?> MergeCategoriesAsync(int sourceCategoryId, int targetCategoryId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets categories that are over budget for the current month
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of over-budget categories with details</returns>
    Task<IEnumerable<OverBudgetCategory>> GetOverBudgetCategoriesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Suggests categories based on transaction description
    /// </summary>
    /// <param name="description">Transaction description</param>
    /// <param name="amount">Transaction amount</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of suggested categories with confidence scores</returns>
    Task<IEnumerable<CategorySuggestion>> SuggestCategoriesAsync(string description, decimal amount, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Validates category data and business rules
    /// </summary>
    /// <param name="category">Category to validate</param>
    /// <param name="isUpdate">Whether this is an update operation</param>
    /// <returns>Validation result</returns>
    Task<ValidationResult> ValidateCategoryAsync(Category category, bool isUpdate = false);
}

/// <summary>
/// Represents a category with spending information
/// </summary>
public class CategoryWithSpending
{
    public Category Category { get; set; } = null!;
    public decimal CurrentMonthSpending { get; set; }
    public decimal PreviousMonthSpending { get; set; }
    public decimal YearToDateSpending { get; set; }
    public int TransactionCount { get; set; }
    public List<CategoryWithSpending> SubCategories { get; set; } = new();
}

/// <summary>
/// Represents category spending analysis
/// </summary>
public class CategorySpendingAnalysis
{
    public Category Category { get; set; } = null!;
    public decimal TotalSpending { get; set; }
    public decimal AverageTransactionAmount { get; set; }
    public int TransactionCount { get; set; }
    public decimal PercentageOfTotal { get; set; }
    public decimal SpendingWithSubcategories { get; set; }
}

/// <summary>
/// Represents category budget performance
/// </summary>
public class CategoryBudgetPerformance
{
    public Category Category { get; set; } = null!;
    public decimal BudgetLimit { get; set; }
    public decimal ActualSpending { get; set; }
    public decimal RemainingBudget { get; set; }
    public decimal UtilizationPercentage { get; set; }
    public bool IsOverBudget { get; set; }
    public int DaysRemainingInPeriod { get; set; }
    public decimal ProjectedSpending { get; set; }
}

/// <summary>
/// Represents category spending trend
/// </summary>
public class CategoryTrend
{
    public Category Category { get; set; } = null!;
    public decimal CurrentPeriodSpending { get; set; }
    public decimal PreviousPeriodSpending { get; set; }
    public decimal ChangeAmount { get; set; }
    public decimal ChangePercentage { get; set; }
    public TrendDirection Direction { get; set; }
}

/// <summary>
/// Represents category usage statistics
/// </summary>
public class CategoryUsageStats
{
    public Category Category { get; set; } = null!;
    public int TotalTransactions { get; set; }
    public decimal TotalSpending { get; set; }
    public decimal AverageTransactionAmount { get; set; }
    public decimal LargestTransaction { get; set; }
    public decimal SmallestTransaction { get; set; }
    public DateTime FirstTransactionDate { get; set; }
    public DateTime LastTransactionDate { get; set; }
    public Dictionary<int, int> TransactionsByMonth { get; set; } = new();
    public Dictionary<int, decimal> SpendingByMonth { get; set; } = new();
}

/// <summary>
/// Represents an over-budget category
/// </summary>
public class OverBudgetCategory
{
    public Category Category { get; set; } = null!;
    public decimal BudgetLimit { get; set; }
    public decimal ActualSpending { get; set; }
    public decimal OverageAmount { get; set; }
    public decimal OveragePercentage { get; set; }
    public int DaysIntoMonth { get; set; }
}

/// <summary>
/// Represents a category suggestion
/// </summary>
public class CategorySuggestion
{
    public Category Category { get; set; } = null!;
    public double ConfidenceScore { get; set; }
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Trend direction enumeration
/// </summary>
public enum TrendDirection
{
    Increasing,
    Decreasing,
    Stable
}