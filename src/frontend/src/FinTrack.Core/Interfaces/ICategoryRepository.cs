using FinTrack.Core.Entities;

namespace FinTrack.Core.Interfaces;

/// <summary>
/// Repository interface for Category-specific operations
/// </summary>
public interface ICategoryRepository : IRepository<Category>
{
    /// <summary>
    /// Gets all root categories (categories without parent)
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
    /// Gets active categories only
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of active categories</returns>
    Task<IEnumerable<Category>> GetActiveAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets system categories (cannot be deleted)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of system categories</returns>
    Task<IEnumerable<Category>> GetSystemCategoriesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets user-created categories (non-system)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of user-created categories</returns>
    Task<IEnumerable<Category>> GetUserCategoriesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets categories with budget limits
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of categories with budget limits</returns>
    Task<IEnumerable<Category>> GetCategoriesWithBudgetAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets categories ordered by sort order
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of categories ordered by sort order</returns>
    Task<IEnumerable<Category>> GetOrderedAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets category hierarchy (all categories with their relationships)
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of categories with navigation properties loaded</returns>
    Task<IEnumerable<Category>> GetHierarchyAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets category by name
    /// </summary>
    /// <param name="name">Category name</param>
    /// <param name="parentCategoryId">Optional parent category ID for scoped search</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Category if found, null otherwise</returns>
    Task<Category?> GetByNameAsync(string name, int? parentCategoryId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Searches categories by name or description
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of categories matching the search term</returns>
    Task<IEnumerable<Category>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets categories that have transactions in a date range
    /// </summary>
    /// <param name="startDate">Start date (inclusive)</param>
    /// <param name="endDate">End date (inclusive)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of categories with transactions in the date range</returns>
    Task<IEnumerable<Category>> GetCategoriesWithTransactionsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets spending by category for a date range
    /// </summary>
    /// <param name="startDate">Start date (inclusive)</param>
    /// <param name="endDate">End date (inclusive)</param>
    /// <param name="includeSubcategories">Whether to include subcategory spending</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary of category to total spending amount</returns>
    Task<Dictionary<Category, decimal>> GetSpendingByCategoryAsync(DateTime startDate, DateTime endDate, bool includeSubcategories = true, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets categories over budget for the current month
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of categories that are over budget</returns>
    Task<IEnumerable<Category>> GetOverBudgetCategoriesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets budget utilization for categories with budget limits
    /// </summary>
    /// <param name="year">Year</param>
    /// <param name="month">Month (1-12)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary of category to budget utilization percentage</returns>
    Task<Dictionary<Category, decimal>> GetBudgetUtilizationAsync(int year, int month, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the full category path for a category
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Full category path string</returns>
    Task<string> GetCategoryPathAsync(int categoryId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all descendant categories (subcategories at all levels)
    /// </summary>
    /// <param name="parentCategoryId">Parent category ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of all descendant categories</returns>
    Task<IEnumerable<Category>> GetDescendantCategoriesAsync(int parentCategoryId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if a category can be deleted (has no transactions or subcategories)
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if category can be deleted, false otherwise</returns>
    Task<bool> CanDeleteAsync(int categoryId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates sort order for multiple categories
    /// </summary>
    /// <param name="categoryOrders">Dictionary of category ID to new sort order</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of categories updated</returns>
    Task<int> UpdateSortOrderAsync(Dictionary<int, int> categoryOrders, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Moves a category to a new parent
    /// </summary>
    /// <param name="categoryId">Category ID to move</param>
    /// <param name="newParentId">New parent category ID (null for root level)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated category</returns>
    Task<Category?> MoveCategoryAsync(int categoryId, int? newParentId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets category statistics (transaction count, total spending, etc.)
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    /// <param name="startDate">Start date for statistics</param>
    /// <param name="endDate">End date for statistics</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary containing category statistics</returns>
    Task<Dictionary<string, object>> GetCategoryStatisticsAsync(int categoryId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}