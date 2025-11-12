using FinTrack.Maui.Models;
using FinTrack.Core.Entities;

namespace FinTrack.Maui.Services;

/// <summary>
/// Service implementation for budget management operations
/// </summary>
public class BudgetService : IBudgetService
{
    // Mock data for demonstration - in real implementation, this would use repositories
    private readonly List<BudgetModel> _budgets;
    private readonly List<CategoryOption> _categories;
    
    public BudgetService()
    {
        // Initialize with sample data
        _categories = new List<CategoryOption>
        {
            new() { Id = 1, Name = "Groceries", Color = "#10B981", Icon = "🛒" },
            new() { Id = 2, Name = "Transportation", Color = "#3B82F6", Icon = "🚗" },
            new() { Id = 3, Name = "Entertainment", Color = "#8B5CF6", Icon = "🎬" },
            new() { Id = 4, Name = "Dining Out", Color = "#F59E0B", Icon = "🍽️" },
            new() { Id = 5, Name = "Shopping", Color = "#EF4444", Icon = "🛍️" },
            new() { Id = 6, Name = "Utilities", Color = "#6B7280", Icon = "⚡" },
            new() { Id = 7, Name = "Healthcare", Color = "#EC4899", Icon = "🏥" },
            new() { Id = 8, Name = "Education", Color = "#14B8A6", Icon = "📚" }
        };
        
        var currentMonth = DateTime.Today;
        var startOfMonth = new DateTime(currentMonth.Year, currentMonth.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
        
        _budgets = new List<BudgetModel>
        {
            new BudgetModel
            {
                Id = 1,
                Name = "Monthly Groceries",
                CategoryId = 1,
                CategoryName = "Groceries",
                CategoryColor = "#10B981",
                BudgetLimit = 800m,
                CurrentSpending = 520m,
                StartDate = startOfMonth,
                EndDate = endOfMonth,
                IsActive = true
            },
            new BudgetModel
            {
                Id = 2,
                Name = "Transportation Budget",
                CategoryId = 2,
                CategoryName = "Transportation",
                CategoryColor = "#3B82F6",
                BudgetLimit = 300m,
                CurrentSpending = 280m,
                StartDate = startOfMonth,
                EndDate = endOfMonth,
                IsActive = true
            },
            new BudgetModel
            {
                Id = 3,
                Name = "Entertainment",
                CategoryId = 3,
                CategoryName = "Entertainment",
                CategoryColor = "#8B5CF6",
                BudgetLimit = 200m,
                CurrentSpending = 250m,
                StartDate = startOfMonth,
                EndDate = endOfMonth,
                IsActive = true
            },
            new BudgetModel
            {
                Id = 4,
                Name = "Dining Out",
                CategoryId = 4,
                CategoryName = "Dining Out",
                CategoryColor = "#F59E0B",
                BudgetLimit = 400m,
                CurrentSpending = 180m,
                StartDate = startOfMonth,
                EndDate = endOfMonth,
                IsActive = true
            }
        };
        
        // Mark categories with existing budgets
        foreach (var budget in _budgets.Where(b => b.IsActive))
        {
            var category = _categories.FirstOrDefault(c => c.Id == budget.CategoryId);
            if (category != null)
            {
                category.HasExistingBudget = true;
            }
        }
    }
    
    public async Task<IEnumerable<BudgetModel>> GetBudgetsAsync()
    {
        await Task.Delay(100); // Simulate async operation
        return _budgets.Where(b => b.IsActive).OrderBy(b => b.CategoryName);
    }
    
    public async Task<IEnumerable<BudgetModel>> GetCurrentMonthBudgetsAsync()
    {
        await Task.Delay(100);
        var now = DateTime.Today;
        return _budgets.Where(b => b.IsActive && b.IsCurrentPeriod).OrderBy(b => b.CategoryName);
    }
    
    public async Task<BudgetModel?> GetBudgetAsync(int id)
    {
        await Task.Delay(50);
        return _budgets.FirstOrDefault(b => b.Id == id && b.IsActive);
    }
    
    public async Task<BudgetModel> CreateBudgetAsync(BudgetModel budget)
    {
        await Task.Delay(100);
        
        budget.Id = _budgets.Count > 0 ? _budgets.Max(b => b.Id) + 1 : 1;
        budget.IsActive = true;
        budget.CurrentSpending = 0; // New budgets start with zero spending
        
        // Get category details
        var category = _categories.FirstOrDefault(c => c.Id == budget.CategoryId);
        if (category != null)
        {
            budget.CategoryName = category.Name;
            budget.CategoryColor = category.Color;
            category.HasExistingBudget = true;
        }
        
        _budgets.Add(budget);
        return budget;
    }
    
    public async Task<BudgetModel> UpdateBudgetAsync(BudgetModel budget)
    {
        await Task.Delay(100);
        
        var existingBudget = _budgets.FirstOrDefault(b => b.Id == budget.Id);
        if (existingBudget != null)
        {
            existingBudget.Name = budget.Name;
            existingBudget.BudgetLimit = budget.BudgetLimit;
            existingBudget.StartDate = budget.StartDate;
            existingBudget.EndDate = budget.EndDate;
            
            // Update category if changed
            if (existingBudget.CategoryId != budget.CategoryId)
            {
                // Mark old category as not having budget
                var oldCategory = _categories.FirstOrDefault(c => c.Id == existingBudget.CategoryId);
                if (oldCategory != null)
                {
                    oldCategory.HasExistingBudget = false;
                }
                
                // Update to new category
                existingBudget.CategoryId = budget.CategoryId;
                var newCategory = _categories.FirstOrDefault(c => c.Id == budget.CategoryId);
                if (newCategory != null)
                {
                    existingBudget.CategoryName = newCategory.Name;
                    existingBudget.CategoryColor = newCategory.Color;
                    newCategory.HasExistingBudget = true;
                }
            }
            
            return existingBudget;
        }
        
        throw new ArgumentException("Budget not found");
    }
    
    public async Task<bool> DeleteBudgetAsync(int id)
    {
        await Task.Delay(100);
        
        var budget = _budgets.FirstOrDefault(b => b.Id == id);
        if (budget != null)
        {
            budget.IsActive = false;
            
            // Mark category as not having budget
            var category = _categories.FirstOrDefault(c => c.Id == budget.CategoryId);
            if (category != null)
            {
                category.HasExistingBudget = false;
            }
            
            return true;
        }
        
        return false;
    }
    
    public async Task<BudgetSummary> GetBudgetSummaryAsync()
    {
        await Task.Delay(100);
        
        var activeBudgets = _budgets.Where(b => b.IsActive && b.IsCurrentPeriod).ToList();
        
        return new BudgetSummary
        {
            TotalBudgeted = activeBudgets.Sum(b => b.BudgetLimit),
            TotalSpent = activeBudgets.Sum(b => b.CurrentSpending),
            ActiveBudgets = activeBudgets.Count,
            ExceededBudgets = activeBudgets.Count(b => b.Status == BudgetStatus.Exceeded),
            WarningBudgets = activeBudgets.Count(b => b.Status == BudgetStatus.Warning)
        };
    }
    
    public async Task<IEnumerable<CategoryOption>> GetAvailableCategoriesAsync()
    {
        await Task.Delay(50);
        return _categories.OrderBy(c => c.Name);
    }
    
    public async Task<IEnumerable<BudgetAlert>> GetBudgetAlertsAsync()
    {
        await Task.Delay(100);
        
        var alerts = new List<BudgetAlert>();
        var activeBudgets = _budgets.Where(b => b.IsActive && b.IsCurrentPeriod);
        
        foreach (var budget in activeBudgets)
        {
            if (budget.Status == BudgetStatus.Exceeded)
            {
                alerts.Add(new BudgetAlert
                {
                    BudgetId = budget.Id,
                    BudgetName = budget.Name,
                    CategoryName = budget.CategoryName,
                    Type = BudgetAlertType.Exceeded,
                    Message = $"Budget exceeded by ${budget.CurrentSpending - budget.BudgetLimit:F2}",
                    CurrentSpending = budget.CurrentSpending,
                    BudgetLimit = budget.BudgetLimit,
                    UtilizationPercentage = budget.UtilizationPercentage
                });
            }
            else if (budget.Status == BudgetStatus.Warning)
            {
                alerts.Add(new BudgetAlert
                {
                    BudgetId = budget.Id,
                    BudgetName = budget.Name,
                    CategoryName = budget.CategoryName,
                    Type = BudgetAlertType.Warning,
                    Message = $"Budget at {budget.UtilizationPercentage:F0}% - ${budget.RemainingAmount:F2} remaining",
                    CurrentSpending = budget.CurrentSpending,
                    BudgetLimit = budget.BudgetLimit,
                    UtilizationPercentage = budget.UtilizationPercentage
                });
            }
        }
        
        return alerts.OrderByDescending(a => a.UtilizationPercentage);
    }
}