using FinTrack.Core.Entities;
using FinTrack.Core.Enums;
using FinTrack.Core.Interfaces;
using FinTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinTrack.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Category-specific operations
/// </summary>
public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
{
    /// <summary>
    /// Constructor for CategoryRepository
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="logger">Logger instance</param>
    public CategoryRepository(FinTrackDbContext context, ILogger<CategoryRepository> logger)
        : base(context, logger)
    {
    }

    /// <summary>
    /// Gets categories by type (Income/Expense)
    /// </summary>
    public async Task<IEnumerable<Category>> GetByTypeAsync(TransactionType categoryType, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting categories of type {CategoryType}", categoryType);
            
            var categories = await _dbSet
                .Where(c => c.CategoryType == categoryType && !c.IsDeleted)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .ToListAsync(cancellationToken);
                
            _logger.LogDebug("Retrieved {Count} categories of type {CategoryType}", categories.Count, categoryType);
            return categories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting categories of type {CategoryType}", categoryType);
            throw;
        }
    }

    /// <summary>
    /// Gets active categories only
    /// </summary>
    public async Task<IEnumerable<Category>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting active categories");
            
            var categories = await _dbSet
                .Where(c => c.IsActive && !c.IsDeleted)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .ToListAsync(cancellationToken);
                
            _logger.LogDebug("Retrieved {Count} active categories", categories.Count);
            return categories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active categories");
            throw;
        }
    }

    /// <summary>
    /// Gets root categories (categories without parent)
    /// </summary>
    public async Task<IEnumerable<Category>> GetRootCategoriesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting root categories");
            
            var categories = await _dbSet
                .Where(c => c.ParentCategoryId == null && !c.IsDeleted)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .ToListAsync(cancellationToken);
                
            _logger.LogDebug("Retrieved {Count} root categories", categories.Count);
            return categories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting root categories");
            throw;
        }
    }

    /// <summary>
    /// Gets subcategories for a parent category
    /// </summary>
    public async Task<IEnumerable<Category>> GetSubCategoriesAsync(int parentCategoryId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting subcategories for parent category {ParentCategoryId}", parentCategoryId);
            
            var categories = await _dbSet
                .Where(c => c.ParentCategoryId == parentCategoryId && !c.IsDeleted)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .ToListAsync(cancellationToken);
                
            _logger.LogDebug("Retrieved {Count} subcategories for parent category {ParentCategoryId}", 
                categories.Count, parentCategoryId);
            return categories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subcategories for parent category {ParentCategoryId}", parentCategoryId);
            throw;
        }
    }

    /// <summary>
    /// Gets the full category hierarchy
    /// </summary>
    public async Task<IEnumerable<CategoryHierarchy>> GetHierarchyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting category hierarchy");
            
            var allCategories = await _dbSet
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .ToListAsync(cancellationToken);
            
            var categoryDict = allCategories.ToDictionary(c => c.Id, c => c);
            var hierarchy = new List<CategoryHierarchy>();
            
            // Build hierarchy starting with root categories
            var rootCategories = allCategories.Where(c => c.ParentCategoryId == null);
            
            foreach (var rootCategory in rootCategories)
            {
                var categoryHierarchy = BuildCategoryHierarchy(rootCategory, categoryDict, 0);
                hierarchy.Add(categoryHierarchy);
            }
            
            _logger.LogDebug("Built category hierarchy with {Count} root categories", hierarchy.Count);
            return hierarchy;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category hierarchy");
            throw;
        }
    }

    /// <summary>
    /// Gets categories with their transaction counts
    /// </summary>
    public async Task<IEnumerable<CategoryWithStats>> GetWithTransactionCountsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting categories with transaction counts from {StartDate} to {EndDate}", startDate, endDate);
            
            var categoriesWithStats = await _dbSet
                .Where(c => !c.IsDeleted)
                .Select(c => new CategoryWithStats
                {
                    Category = c,
                    TransactionCount = c.Transactions.Count(t => !t.IsDeleted && t.Date >= startDate && t.Date <= endDate),
                    FirstTransactionDate = c.Transactions.Where(t => !t.IsDeleted).Min(t => (DateTime?)t.Date),
                    LastTransactionDate = c.Transactions.Where(t => !t.IsDeleted).Max(t => (DateTime?)t.Date)
                })
                .OrderBy(c => c.Category.SortOrder)
                .ThenBy(c => c.Category.Name)
                .ToListAsync(cancellationToken);
                
            _logger.LogDebug("Retrieved {Count} categories with transaction counts", categoriesWithStats.Count);
            return categoriesWithStats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting categories with transaction counts from {StartDate} to {EndDate}", 
                startDate, endDate);
            throw;
        }
    }

    /// <summary>
    /// Gets categories with spending totals for a date range
    /// </summary>
    public async Task<IEnumerable<CategorySpending>> GetSpendingTotalsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting category spending totals from {StartDate} to {EndDate}", startDate, endDate);
            
            var totalSpending = await _context.Transactions
                .Where(t => t.Type == TransactionType.Expense && 
                           t.Date >= startDate && 
                           t.Date <= endDate && 
                           !t.IsDeleted)
                .SumAsync(t => t.Amount, cancellationToken);
            
            var categorySpending = await _dbSet
                .Where(c => !c.IsDeleted)
                .Select(c => new
                {
                    Category = c,
                    TotalSpending = c.Transactions
                        .Where(t => t.Type == TransactionType.Expense && 
                                   t.Date >= startDate && 
                                   t.Date <= endDate && 
                                   !t.IsDeleted)
                        .Sum(t => t.Amount),
                    TransactionCount = c.Transactions
                        .Count(t => t.Type == TransactionType.Expense && 
                                   t.Date >= startDate && 
                                   t.Date <= endDate && 
                                   !t.IsDeleted)
                })
                .Where(c => c.TotalSpending > 0)
                .ToListAsync(cancellationToken);
            
            var result = categorySpending.Select(c => new CategorySpending
            {
                Category = c.Category,
                TotalSpending = c.TotalSpending,
                TransactionCount = c.TransactionCount,
                AverageTransactionAmount = c.TransactionCount > 0 ? c.TotalSpending / c.TransactionCount : 0,
                PercentageOfTotal = totalSpending > 0 ? (c.TotalSpending / totalSpending) * 100 : 0
            })
            .OrderByDescending(c => c.TotalSpending)
            .ToList();
            
            _logger.LogDebug("Retrieved spending totals for {Count} categories", result.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category spending totals from {StartDate} to {EndDate}", 
                startDate, endDate);
            throw;
        }
    }

    /// <summary>
    /// Gets system-defined categories
    /// </summary>
    public async Task<IEnumerable<Category>> GetSystemCategoriesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting system categories");
            
            var categories = await _dbSet
                .Where(c => c.IsSystem && !c.IsDeleted)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .ToListAsync(cancellationToken);
                
            _logger.LogDebug("Retrieved {Count} system categories", categories.Count);
            return categories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting system categories");
            throw;
        }
    }

    /// <summary>
    /// Gets user-defined categories
    /// </summary>
    public async Task<IEnumerable<Category>> GetUserCategoriesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting user categories");
            
            var categories = await _dbSet
                .Where(c => !c.IsSystem && !c.IsDeleted)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .ToListAsync(cancellationToken);
                
            _logger.LogDebug("Retrieved {Count} user categories", categories.Count);
            return categories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user categories");
            throw;
        }
    }

    /// <summary>
    /// Searches categories by name
    /// </summary>
    public async Task<IEnumerable<Category>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return Enumerable.Empty<Category>();
                
            _logger.LogDebug("Searching categories by name: {SearchTerm}", searchTerm);
            
            var categories = await _dbSet
                .Where(c => c.Name.Contains(searchTerm) && !c.IsDeleted)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .ToListAsync(cancellationToken);
                
            _logger.LogDebug("Found {Count} categories matching name: {SearchTerm}", categories.Count, searchTerm);
            return categories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching categories by name: {SearchTerm}", searchTerm);
            throw;
        }
    }

    /// <summary>
    /// Gets categories that have transactions in a date range
    /// </summary>
    public async Task<IEnumerable<Category>> GetUsedInDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting categories used from {StartDate} to {EndDate}", startDate, endDate);
            
            var categories = await _dbSet
                .Where(c => c.Transactions.Any(t => t.Date >= startDate && 
                                                   t.Date <= endDate && 
                                                   !t.IsDeleted) && 
                           !c.IsDeleted)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .ToListAsync(cancellationToken);
                
            _logger.LogDebug("Retrieved {Count} categories used from {StartDate} to {EndDate}", 
                categories.Count, startDate, endDate);
            return categories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting categories used from {StartDate} to {EndDate}", startDate, endDate);
            throw;
        }
    }

    /// <summary>
    /// Gets categories ordered by sort order
    /// </summary>
    public async Task<IEnumerable<Category>> GetOrderedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting categories ordered by sort order");
            
            var categories = await _dbSet
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Name)
                .ToListAsync(cancellationToken);
                
            _logger.LogDebug("Retrieved {Count} ordered categories", categories.Count);
            return categories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ordered categories");
            throw;
        }
    }

    /// <summary>
    /// Updates the sort order for multiple categories
    /// </summary>
    public async Task<int> UpdateSortOrdersAsync(Dictionary<int, int> categoryOrders, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Updating sort orders for {Count} categories", categoryOrders.Count);
            
            var categoryIds = categoryOrders.Keys.ToList();
            var categories = await _dbSet
                .Where(c => categoryIds.Contains(c.Id) && !c.IsDeleted)
                .ToListAsync(cancellationToken);
            
            var updatedCount = 0;
            foreach (var category in categories)
            {
                if (categoryOrders.TryGetValue(category.Id, out var newSortOrder))
                {
                    category.SortOrder = newSortOrder;
                    category.MarkAsModified();
                    updatedCount++;
                }
            }
            
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogDebug("Updated sort orders for {Count} categories", updatedCount);
            return updatedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating sort orders for categories");
            throw;
        }
    }

    /// <summary>
    /// Checks if a category can be deleted (has no transactions or subcategories)
    /// </summary>
    public async Task<bool> CanDeleteAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Checking if category {CategoryId} can be deleted", categoryId);
            
            // Check for transactions
            var hasTransactions = await _context.Transactions
                .AnyAsync(t => t.CategoryId == categoryId && !t.IsDeleted, cancellationToken);
            
            if (hasTransactions)
            {
                _logger.LogDebug("Category {CategoryId} cannot be deleted - has transactions", categoryId);
                return false;
            }
            
            // Check for subcategories
            var hasSubcategories = await _dbSet
                .AnyAsync(c => c.ParentCategoryId == categoryId && !c.IsDeleted, cancellationToken);
            
            if (hasSubcategories)
            {
                _logger.LogDebug("Category {CategoryId} cannot be deleted - has subcategories", categoryId);
                return false;
            }
            
            // Check for budgets
            var hasBudgets = await _context.Budgets
                .AnyAsync(b => b.CategoryId == categoryId && !b.IsDeleted, cancellationToken);
            
            if (hasBudgets)
            {
                _logger.LogDebug("Category {CategoryId} cannot be deleted - has budgets", categoryId);
                return false;
            }
            
            _logger.LogDebug("Category {CategoryId} can be deleted", categoryId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if category {CategoryId} can be deleted", categoryId);
            throw;
        }
    }

    /// <summary>
    /// Gets the path from root to the specified category
    /// </summary>
    public async Task<IEnumerable<Category>> GetCategoryPathAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting category path for category {CategoryId}", categoryId);
            
            var path = new List<Category>();
            var currentCategoryId = (int?)categoryId;
            
            while (currentCategoryId.HasValue)
            {
                var category = await _dbSet
                    .Where(c => c.Id == currentCategoryId.Value && !c.IsDeleted)
                    .FirstOrDefaultAsync(cancellationToken);
                
                if (category == null)
                    break;
                
                path.Insert(0, category); // Insert at beginning to build path from root
                currentCategoryId = category.ParentCategoryId;
            }
            
            _logger.LogDebug("Retrieved category path with {Count} categories for category {CategoryId}", 
                path.Count, categoryId);
            return path;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category path for category {CategoryId}", categoryId);
            throw;
        }
    }

    /// <summary>
    /// Helper method to build category hierarchy recursively
    /// </summary>
    private CategoryHierarchy BuildCategoryHierarchy(Category category, Dictionary<int, Category> categoryDict, int level)
    {
        var hierarchy = new CategoryHierarchy
        {
            Category = category,
            Level = level,
            SubCategories = new List<CategoryHierarchy>()
        };
        
        var subCategories = categoryDict.Values
            .Where(c => c.ParentCategoryId == category.Id)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name);
        
        var subCategoryHierarchies = new List<CategoryHierarchy>();
        foreach (var subCategory in subCategories)
        {
            var subHierarchy = BuildCategoryHierarchy(subCategory, categoryDict, level + 1);
            subCategoryHierarchies.Add(subHierarchy);
        }
        
        hierarchy.SubCategories = subCategoryHierarchies;
        return hierarchy;
    }

    /// <summary>
    /// Gets a category by name
    /// </summary>
    public async Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting category by name: {Name}", name);
            
            var category = await _dbSet
                .Where(c => !c.IsDeleted && c.Name == name)
                .FirstOrDefaultAsync(cancellationToken);
                
            if (category == null)
            {
                _logger.LogDebug("Category with name {Name} not found", name);
            }
            
            return category;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category by name: {Name}", name);
            throw;
        }
    }
}