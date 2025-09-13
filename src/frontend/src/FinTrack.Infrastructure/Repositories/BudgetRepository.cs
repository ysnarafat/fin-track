using FinTrack.Core.Entities;
using FinTrack.Core.Enums;
using FinTrack.Core.Interfaces;
using FinTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinTrack.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Budget-specific operations
/// </summary>
public class BudgetRepository : BaseRepository<Budget>, IBudgetRepository
{
    /// <summary>
    /// Constructor for BudgetRepository
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="logger">Logger instance</param>
    public BudgetRepository(FinTrackDbContext context, ILogger<BudgetRepository> logger)
        : base(context, logger)
    {
    }

    /// <summary>
    /// Gets budgets by period type
    /// </summary>
    public async Task<IEnumerable<Budget>> GetByPeriodAsync(BudgetPeriod period, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting budgets of period {Period}", period);
            
            var budgets = await _dbSet
                .Include(b => b.Category)
                .Where(b => b.Period == period && !b.IsDeleted)
                .OrderBy(b => b.StartDate)
                .ThenBy(b => b.Name)
                .ToListAsync(cancellationToken);
                
            _logger.LogDebug("Retrieved {Count} budgets of period {Period}", budgets.Count, period);
            return budgets;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting budgets of period {Period}", period);
            throw;
        }
    }

    /// <summary>
    /// Gets active budgets only
    /// </summary>
    public async Task<IEnumerable<Budget>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting active budgets");
            
            var budgets = await _dbSet
                .Include(b => b.Category)
                .Where(b => b.IsActive && !b.IsDeleted)
                .OrderBy(b => b.StartDate)
                .ThenBy(b => b.Name)
                .ToListAsync(cancellationToken);
                
            _logger.LogDebug("Retrieved {Count} active budgets", budgets.Count);
            return budgets;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active budgets");
            throw;
        }
    }

    /// <summary>
    /// Gets budgets for a specific category
    /// </summary>
    public async Task<IEnumerable<Budget>> GetByCategoryAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting budgets for category {CategoryId}", categoryId);
            
            var budgets = await _dbSet
                .Include(b => b.Category)
                .Where(b => b.CategoryId == categoryId && !b.IsDeleted)
                .OrderBy(b => b.StartDate)
                .ThenBy(b => b.Name)
                .ToListAsync(cancellationToken);
                
            _logger.LogDebug("Retrieved {Count} budgets for category {CategoryId}", budgets.Count, categoryId);
            return budgets;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting budgets for category {CategoryId}", categoryId);
            throw;
        }
    }

    /// <summary>
    /// Gets current budgets (budgets that are active in the current date)
    /// </summary>
    public async Task<IEnumerable<Budget>> GetCurrentBudgetsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var currentDate = DateTime.Today;
            _logger.LogDebug("Getting current budgets for date {CurrentDate}", currentDate);
            
            var budgets = await _dbSet
                .Include(b => b.Category)
                .Where(b => b.IsActive && 
                           b.StartDate <= currentDate && 
                           b.EndDate >= currentDate && 
                           !b.IsDeleted)
                .OrderBy(b => b.Name)
                .ToListAsync(cancellationToken);
                
            _logger.LogDebug("Retrieved {Count} current budgets", budgets.Count);
            return budgets;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current budgets");
            throw;
        }
    }

    /// <summary>
    /// Gets budgets for a specific date range
    /// </summary>
    public async Task<IEnumerable<Budget>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting budgets from {StartDate} to {EndDate}", startDate, endDate);
            
            var budgets = await _dbSet
                .Include(b => b.Category)
                .Where(b => (b.StartDate <= endDate && b.EndDate >= startDate) && !b.IsDeleted)
                .OrderBy(b => b.StartDate)
                .ThenBy(b => b.Name)
                .ToListAsync(cancellationToken);
                
            _logger.LogDebug("Retrieved {Count} budgets from {StartDate} to {EndDate}", 
                budgets.Count, startDate, endDate);
            return budgets;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting budgets from {StartDate} to {EndDate}", startDate, endDate);
            throw;
        }
    }

    /// <summary>
    /// Gets budgets that have exceeded their limits
    /// </summary>
    public async Task<IEnumerable<Budget>> GetExceededBudgetsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting exceeded budgets");
            
            var budgets = await _dbSet
                .Include(b => b.Category)
                .Where(b => b.IsActive && !b.IsDeleted)
                .ToListAsync(cancellationToken);
            
            var exceededBudgets = budgets.Where(b => b.IsExceeded).ToList();
            
            _logger.LogDebug("Found {Count} exceeded budgets", exceededBudgets.Count);
            return exceededBudgets;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting exceeded budgets");
            throw;
        }
    }

    /// <summary>
    /// Gets budgets that have reached their alert threshold
    /// </summary>
    public async Task<IEnumerable<Budget>> GetBudgetsAtAlertThresholdAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting budgets at alert threshold");
            
            var budgets = await _dbSet
                .Include(b => b.Category)
                .Where(b => b.IsActive && b.AlertsEnabled && !b.IsDeleted)
                .ToListAsync(cancellationToken);
            
            var alertBudgets = budgets.Where(b => b.HasReachedAlertThreshold).ToList();
            
            _logger.LogDebug("Found {Count} budgets at alert threshold", alertBudgets.Count);
            return alertBudgets;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting budgets at alert threshold");
            throw;
        }
    }

    /// <summary>
    /// Updates spent amount for a budget
    /// </summary>
    public async Task<bool> UpdateSpentAmountAsync(int budgetId, decimal spentAmount, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Updating spent amount for budget {BudgetId} to {SpentAmount}", budgetId, spentAmount);
            
            var budget = await _dbSet
                .Where(b => b.Id == budgetId && !b.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);
                
            if (budget == null)
            {
                _logger.LogWarning("Budget {BudgetId} not found for spent amount update", budgetId);
                return false;
            }
            
            budget.SpentAmount = spentAmount;
            budget.MarkAsModified();
            
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogDebug("Updated spent amount for budget {BudgetId} to {SpentAmount}", budgetId, spentAmount);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating spent amount for budget {BudgetId}", budgetId);
            throw;
        }
    }

    /// <summary>
    /// Recalculates spent amounts for all budgets based on transactions
    /// </summary>
    public async Task<int> RecalculateAllSpentAmountsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Recalculating spent amounts for all budgets");
            
            var budgets = await _dbSet
                .Where(b => b.IsActive && !b.IsDeleted)
                .ToListAsync(cancellationToken);
            
            var updatedCount = 0;
            
            foreach (var budget in budgets)
            {
                var spentAmount = await CalculateSpentAmountForBudget(budget, cancellationToken);
                
                if (budget.SpentAmount != spentAmount)
                {
                    budget.SpentAmount = spentAmount;
                    budget.MarkAsModified();
                    updatedCount++;
                }
            }
            
            if (updatedCount > 0)
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            
            _logger.LogDebug("Recalculated spent amounts for {Count} budgets", updatedCount);
            return updatedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recalculating spent amounts for all budgets");
            throw;
        }
    }

    /// <summary>
    /// Recalculates spent amount for a specific budget based on transactions
    /// </summary>
    public async Task<bool> RecalculateSpentAmountAsync(int budgetId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Recalculating spent amount for budget {BudgetId}", budgetId);
            
            var budget = await _dbSet
                .Where(b => b.Id == budgetId && !b.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);
                
            if (budget == null)
            {
                _logger.LogWarning("Budget {BudgetId} not found for spent amount recalculation", budgetId);
                return false;
            }
            
            var spentAmount = await CalculateSpentAmountForBudget(budget, cancellationToken);
            
            if (budget.SpentAmount != spentAmount)
            {
                budget.SpentAmount = spentAmount;
                budget.MarkAsModified();
                await _context.SaveChangesAsync(cancellationToken);
            }
            
            _logger.LogDebug("Recalculated spent amount for budget {BudgetId}: {SpentAmount}", budgetId, spentAmount);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recalculating spent amount for budget {BudgetId}", budgetId);
            throw;
        }
    }

    /// <summary>
    /// Gets budget performance summary for a date range
    /// </summary>
    public async Task<IEnumerable<BudgetPerformance>> GetBudgetPerformanceAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting budget performance from {StartDate} to {EndDate}", startDate, endDate);
            
            var budgets = await _dbSet
                .Include(b => b.Category)
                .Where(b => (b.StartDate <= endDate && b.EndDate >= startDate) && !b.IsDeleted)
                .ToListAsync(cancellationToken);
            
            var performance = new List<BudgetPerformance>();
            
            foreach (var budget in budgets)
            {
                var budgetPerformance = new BudgetPerformance
                {
                    Budget = budget,
                    UtilizationPercentage = budget.UtilizationPercentage,
                    RemainingAmount = budget.RemainingAmount,
                    ProjectedSpending = budget.ProjectedSpending,
                    DaysRemaining = budget.DaysRemaining,
                    IsOnTrack = budget.ProjectedSpending <= budget.TotalBudgetAmount,
                    DailySpendingRate = budget.DailySpendingRate,
                    RecommendedDailySpending = budget.DaysRemaining > 0 ? budget.RemainingAmount / budget.DaysRemaining : 0
                };
                
                performance.Add(budgetPerformance);
            }
            
            _logger.LogDebug("Retrieved performance data for {Count} budgets", performance.Count);
            return performance;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting budget performance from {StartDate} to {EndDate}", startDate, endDate);
            throw;
        }
    }

    /// <summary>
    /// Gets budgets with their category information
    /// </summary>
    public async Task<IEnumerable<Budget>> GetWithCategoryAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting budgets with category information");
            
            var budgets = await _dbSet
                .Include(b => b.Category)
                .Where(b => !b.IsDeleted)
                .OrderBy(b => b.StartDate)
                .ThenBy(b => b.Name)
                .ToListAsync(cancellationToken);
                
            _logger.LogDebug("Retrieved {Count} budgets with category information", budgets.Count);
            return budgets;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting budgets with category information");
            throw;
        }
    }

    /// <summary>
    /// Creates budgets for the next period based on existing budgets
    /// </summary>
    public async Task<IEnumerable<Budget>> CreateNextPeriodBudgetsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Creating budgets for next period");
            
            var currentBudgets = await _dbSet
                .Where(b => b.IsActive && b.EndDate < DateTime.Today && !b.IsDeleted)
                .ToListAsync(cancellationToken);
            
            var nextPeriodBudgets = new List<Budget>();
            
            foreach (var currentBudget in currentBudgets)
            {
                var nextBudget = currentBudget.CreateNextPeriodBudget();
                nextPeriodBudgets.Add(nextBudget);
            }
            
            if (nextPeriodBudgets.Any())
            {
                _dbSet.AddRange(nextPeriodBudgets);
                await _context.SaveChangesAsync(cancellationToken);
            }
            
            _logger.LogDebug("Created {Count} budgets for next period", nextPeriodBudgets.Count);
            return nextPeriodBudgets;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating budgets for next period");
            throw;
        }
    }

    /// <summary>
    /// Gets budget utilization statistics
    /// </summary>
    public async Task<BudgetUtilizationStats> GetUtilizationStatsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting budget utilization statistics");
            
            var budgets = await _dbSet
                .Where(b => !b.IsDeleted)
                .ToListAsync(cancellationToken);
            
            var activeBudgets = budgets.Where(b => b.IsActive).ToList();
            var exceededBudgets = activeBudgets.Where(b => b.IsExceeded).ToList();
            var alertBudgets = activeBudgets.Where(b => b.HasReachedAlertThreshold).ToList();
            
            var stats = new BudgetUtilizationStats
            {
                TotalBudgets = budgets.Count,
                ActiveBudgets = activeBudgets.Count,
                ExceededBudgets = exceededBudgets.Count,
                BudgetsAtAlert = alertBudgets.Count,
                TotalBudgetAmount = activeBudgets.Sum(b => b.TotalBudgetAmount),
                TotalSpentAmount = activeBudgets.Sum(b => b.SpentAmount),
                AverageUtilization = activeBudgets.Any() ? activeBudgets.Average(b => b.UtilizationPercentage) : 0,
                TotalRemainingAmount = activeBudgets.Sum(b => b.RemainingAmount)
            };
            
            _logger.LogDebug("Retrieved budget utilization statistics");
            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting budget utilization statistics");
            throw;
        }
    }

    /// <summary>
    /// Searches budgets by name
    /// </summary>
    public async Task<IEnumerable<Budget>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return Enumerable.Empty<Budget>();
                
            _logger.LogDebug("Searching budgets by name: {SearchTerm}", searchTerm);
            
            var budgets = await _dbSet
                .Include(b => b.Category)
                .Where(b => b.Name.Contains(searchTerm) && !b.IsDeleted)
                .OrderBy(b => b.StartDate)
                .ThenBy(b => b.Name)
                .ToListAsync(cancellationToken);
                
            _logger.LogDebug("Found {Count} budgets matching name: {SearchTerm}", budgets.Count, searchTerm);
            return budgets;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching budgets by name: {SearchTerm}", searchTerm);
            throw;
        }
    }

    /// <summary>
    /// Helper method to calculate spent amount for a budget based on transactions
    /// </summary>
    private async Task<decimal> CalculateSpentAmountForBudget(Budget budget, CancellationToken cancellationToken)
    {
        var query = _context.Transactions
            .Where(t => t.Type == TransactionType.Expense && 
                       t.Date >= budget.StartDate && 
                       t.Date <= budget.EndDate && 
                       !t.IsDeleted);
        
        // If budget is for a specific category, filter by category
        if (budget.CategoryId.HasValue)
        {
            query = query.Where(t => t.CategoryId == budget.CategoryId.Value);
        }
        
        return await query.SumAsync(t => t.Amount, cancellationToken);
    }

    /// <summary>
    /// Gets budgets that overlap with the specified date range for a category
    /// </summary>
    public async Task<IEnumerable<Budget>> GetOverlappingBudgetsAsync(int? categoryId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting overlapping budgets for category {CategoryId} from {StartDate} to {EndDate}", categoryId, startDate, endDate);
            
            var query = _dbSet.Where(b => !b.IsDeleted && b.IsActive);
            
            // Filter by category if specified
            if (categoryId.HasValue)
            {
                query = query.Where(b => b.CategoryId == categoryId.Value);
            }
            else
            {
                query = query.Where(b => b.CategoryId == null); // Only budgets that apply to all categories
            }
            
            // Check for date overlap
            query = query.Where(b => 
                (startDate >= b.StartDate && startDate <= b.EndDate) ||
                (endDate >= b.StartDate && endDate <= b.EndDate) ||
                (startDate <= b.StartDate && endDate >= b.EndDate));
            
            var budgets = await query.ToListAsync(cancellationToken);
            
            _logger.LogDebug("Found {Count} overlapping budgets", budgets.Count);
            return budgets;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting overlapping budgets for category {CategoryId}", categoryId);
            throw;
        }
    }
}