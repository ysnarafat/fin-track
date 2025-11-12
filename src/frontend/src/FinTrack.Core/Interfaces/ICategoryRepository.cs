using FinTrack.Core.Entities;
using FinTrack.Core.Enums;

namespace FinTrack.Core.Interfaces;

/// <summary>
/// Repository interface for Category-specific operations
/// </summary>
public interface ICategoryRepository : IRepository<Category>
{
    /// <summary>
    /// Gets categories by type (Income/Expense)
    /// </summary>
    /// <param name="categoryType">Category type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of categories of the specified type</returns>
    Task<IEnumerable<Category>> GetByTypeAsync(TransactionType categoryType, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets active categories only
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of active categories</returns>
    Task<IEnumerable<Category>> GetActiveAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets root categories (categories without parent)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of root categories</returns>
    Task<IEnumerable<Category>> GetRootCategoriesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets subcategories for a parent category
    /// </summary>
    /// <param name="parentCategoryId">Parent category ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of subcategories</returns>
    Task<IEnumerable<Category>> GetSubCategoriesAsync(int parentCategoryId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the full category hierarchy
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of categories organized hierarchically</returns>
    Task<IEnumerable<CategoryHierarchy>> GetHierarchyAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets categories with their transaction counts
    /// </summary>
    /// <param name="startDate">Start date for transaction counting</param>
    /// <param name="endDate">End date for transaction counting</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of categories with transaction counts</returns>
    Task<IEnumerable<CategoryWithStats>> GetWithTransactionCountsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets categories with spending totals for a date range
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of categories with spending totals</returns>
    Task<IEnumerable<CategorySpending>> GetSpendingTotalsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets system-defined categories
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of system categories</returns>
    Task<IEnumerable<Category>> GetSystemCategoriesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets user-defined categories
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of user-defined categories</returns>
    Task<IEnumerable<Category>> GetUserCategoriesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Searches categories by name
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of matching categories</returns>
    Task<IEnumerable<Category>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets categories that have transactions in a date range
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of categories with transactions in the date range</returns>
    Task<IEnumerable<Category>> GetUsedInDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets categories ordered by sort order
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of categories ordered by sort order</returns>
    Task<IEnumerable<Category>> GetOrderedAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates the sort order for multiple categories
    /// </summary>
    /// <param name="categoryOrders">Dictionary of category ID to sort order</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of categories updated</returns>
    Task<int> UpdateSortOrdersAsync(Dictionary<int, int> categoryOrders, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if a category can be deleted (has no transactions or subcategories)
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if category can be deleted</returns>
    Task<bool> CanDeleteAsync(int categoryId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the path from root to the specified category
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of categories from root to specified category</returns>
    Task<IEnumerable<Category>> GetCategoryPathAsync(int categoryId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a category by name
    /// </summary>
    /// <param name="name">Category name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Category if found, null otherwise</returns>
    Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a category in a hierarchical structure
/// </summary>
public class CategoryHierarchy
{
    public Category Category { get; set; } = null!;
    public IEnumerable<CategoryHierarchy> SubCategories { get; set; } = new List<CategoryHierarchy>();
    public int Level { get; set; }
}

/// <summary>
/// Category with transaction statistics
/// </summary>
public class CategoryWithStats
{
    public Category Category { get; set; } = null!;
    public int TransactionCount { get; set; }
    public DateTime? LastTransactionDate { get; set; }
    public DateTime? FirstTransactionDate { get; set; }
}

/// <summary>
/// Category with spending information
/// </summary>
public class CategorySpending
{
    public Category Category { get; set; } = null!;
    public decimal TotalSpending { get; set; }
    public decimal AverageTransactionAmount { get; set; }
    public int TransactionCount { get; set; }
    public decimal PercentageOfTotal { get; set; }
}