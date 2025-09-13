using FinTrack.Core.Entities;
using FinTrack.Core.Enums;
using FinTrack.Core.Exceptions;
using FinTrack.Core.Interfaces;
using FinTrack.Shared.Models;
using Microsoft.Extensions.Logging;

namespace FinTrack.Shared.Services;

/// <summary>
/// Service implementation for budget business logic and operations
/// </summary>
public class BudgetService : IBudgetService
{
    private readonly IBudgetRepository _budgetRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ILogger<BudgetService> _logger;

    public BudgetService(
        IBudgetRepository budgetRepository,
        ICategoryRepository categoryRepository,
        ITransactionRepository transactionRepository,
        ILogger<BudgetService> logger)
    {
        _budgetRepository = budgetRepository;
        _categoryRepository = categoryRepository;
        _transactionRepository = transactionRepository;
        _logger = logger;
    }

    public async Task<Budget> CreateBudgetAsync(Budget budget, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating budget: {BudgetName} for amount {Amount}", budget.Name, budget.Amount);

        // Validate the budget
        var validationResult = await ValidateBudgetAsync(budget, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new BusinessRuleException("BudgetValidationFailed", $"Budget validation failed: {string.Join(", ", validationResult.Errors)}");
        }

        // Calculate initial spent amount based on existing transactions
        if (budget.CategoryId.HasValue)
        {
            var spentAmount = await CalculateSpentAmountAsync(budget.CategoryId.Value, budget.StartDate, budget.EndDate, cancellationToken);
            budget.SpentAmount = spentAmount;
        }

        // Create the budget
        var createdBudget = await _budgetRepository.AddAsync(budget, cancellationToken);
        await _budgetRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Budget created successfully with ID: {BudgetId}", createdBudget.Id);
        return createdBudget;
    }

    public async Task<Budget> UpdateBudgetAsync(Budget budget, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating budget ID: {BudgetId}", budget.Id);

        // Validate the budget
        var validationResult = await ValidateBudgetAsync(budget, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new BusinessRuleException("BudgetValidationFailed", $"Budget validation failed: {string.Join(", ", validationResult.Errors)}");
        }

        // Update the budget
        var updatedBudget = await _budgetRepository.UpdateAsync(budget, cancellationToken);
        await _budgetRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Budget updated successfully: {BudgetId}", budget.Id);
        return updatedBudget;
    }

    public async Task<bool> DeleteBudgetAsync(int budgetId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting budget ID: {BudgetId}", budgetId);

        var budget = await _budgetRepository.GetByIdAsync(budgetId, cancellationToken);
        if (budget == null)
        {
            _logger.LogWarning("Budget not found for deletion: {BudgetId}", budgetId);
            return false;
        }

        var result = await _budgetRepository.DeleteAsync(budgetId, cancellationToken);
        await _budgetRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Budget deleted successfully: {BudgetId}", budgetId);
        return result;
    }

    public async Task<Budget?> GetBudgetAsync(int budgetId, CancellationToken cancellationToken = default)
    {
        return await _budgetRepository.GetByIdAsync(budgetId, cancellationToken);
    }

    public async Task<IEnumerable<Budget>> GetActiveBudgetsAsync(CancellationToken cancellationToken = default)
    {
        return await _budgetRepository.GetActiveAsync(cancellationToken);
    }

    public async Task<IEnumerable<Budget>> GetCurrentBudgetsAsync(CancellationToken cancellationToken = default)
    {
        return await _budgetRepository.GetCurrentBudgetsAsync(cancellationToken);
    }

    public async Task<IEnumerable<Budget>> GetBudgetsByPeriodAsync(BudgetPeriod period, CancellationToken cancellationToken = default)
    {
        return await _budgetRepository.GetByPeriodAsync(period, cancellationToken);
    }

    public async Task<IEnumerable<Budget>> GetBudgetsByCategoryAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        return await _budgetRepository.GetByCategoryAsync(categoryId, cancellationToken);
    }

    public async Task<IEnumerable<Budget>> GetExceededBudgetsAsync(CancellationToken cancellationToken = default)
    {
        return await _budgetRepository.GetExceededBudgetsAsync(cancellationToken);
    }

    public async Task<IEnumerable<Budget>> GetBudgetsAtAlertThresholdAsync(CancellationToken cancellationToken = default)
    {
        return await _budgetRepository.GetBudgetsAtAlertThresholdAsync(cancellationToken);
    }

    public async Task<int> RecalculateAllSpentAmountsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Recalculating spent amounts for all budgets");

        var budgets = await _budgetRepository.GetActiveAsync(cancellationToken);
        var updatedCount = 0;

        foreach (var budget in budgets)
        {
            if (await RecalculateSpentAmountAsync(budget.Id, cancellationToken))
            {
                updatedCount++;
            }
        }

        _logger.LogInformation("Recalculated spent amounts for {Count} budgets", updatedCount);
        return updatedCount;
    }

    public async Task<bool> RecalculateSpentAmountAsync(int budgetId, CancellationToken cancellationToken = default)
    {
        var budget = await _budgetRepository.GetByIdAsync(budgetId, cancellationToken);
        if (budget == null)
        {
            return false;
        }

        decimal spentAmount = 0;

        if (budget.CategoryId.HasValue)
        {
            spentAmount = await CalculateSpentAmountAsync(budget.CategoryId.Value, budget.StartDate, budget.EndDate, cancellationToken);
        }
        else
        {
            // Budget applies to all categories - calculate total expenses
            spentAmount = await _transactionRepository.CalculateExpensesAsync(null, budget.StartDate, budget.EndDate, cancellationToken);
        }

        if (budget.SpentAmount != spentAmount)
        {
            budget.SpentAmount = spentAmount;
            await _budgetRepository.UpdateAsync(budget, cancellationToken);
            await _budgetRepository.SaveChangesAsync(cancellationToken);
            return true;
        }

        return false;
    }

    public async Task<IEnumerable<BudgetPerformance>> GetBudgetPerformanceAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _budgetRepository.GetBudgetPerformanceAsync(startDate, endDate, cancellationToken);
    }

    public async Task<BudgetUtilizationStats> GetUtilizationStatsAsync(CancellationToken cancellationToken = default)
    {
        return await _budgetRepository.GetUtilizationStatsAsync(cancellationToken);
    }

    public async Task<IEnumerable<Budget>> CreateNextPeriodBudgetsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating budgets for next period");

        var currentBudgets = await _budgetRepository.GetCurrentBudgetsAsync(cancellationToken);
        var newBudgets = new List<Budget>();

        foreach (var currentBudget in currentBudgets)
        {
            if (currentBudget.IsActive)
            {
                var nextBudget = currentBudget.CreateNextPeriodBudget();
                var createdBudget = await _budgetRepository.AddAsync(nextBudget, cancellationToken);
                newBudgets.Add(createdBudget);
            }
        }

        await _budgetRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created {Count} budgets for next period", newBudgets.Count);
        return newBudgets;
    }

    public async Task<IEnumerable<BudgetAlert>> GetBudgetAlertsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating budget alerts");

        var alerts = new List<BudgetAlert>();
        var currentBudgets = await _budgetRepository.GetCurrentBudgetsAsync(cancellationToken);

        foreach (var budget in currentBudgets.Where(b => b.AlertsEnabled))
        {
            // Check for exceeded budgets
            if (budget.IsExceeded)
            {
                alerts.Add(new BudgetAlert
                {
                    Budget = budget,
                    AlertType = BudgetAlertType.BudgetExceeded,
                    Message = $"Budget '{budget.Name}' has been exceeded by {(budget.SpentAmount - budget.TotalBudgetAmount):C}",
                    Amount = budget.SpentAmount - budget.TotalBudgetAmount,
                    Percentage = budget.UtilizationPercentage
                });
            }
            // Check for threshold alerts
            else if (budget.HasReachedAlertThreshold)
            {
                alerts.Add(new BudgetAlert
                {
                    Budget = budget,
                    AlertType = BudgetAlertType.ThresholdReached,
                    Message = $"Budget '{budget.Name}' has reached {budget.UtilizationPercentage:F1}% of its limit",
                    Amount = budget.SpentAmount,
                    Percentage = budget.UtilizationPercentage
                });
            }

            // Check for projected overspend
            if (budget.ProjectedSpending > budget.TotalBudgetAmount && !budget.IsExceeded)
            {
                alerts.Add(new BudgetAlert
                {
                    Budget = budget,
                    AlertType = BudgetAlertType.ProjectedOverspend,
                    Message = $"Budget '{budget.Name}' is projected to exceed by {(budget.ProjectedSpending - budget.TotalBudgetAmount):C}",
                    Amount = budget.ProjectedSpending - budget.TotalBudgetAmount,
                    Percentage = (budget.ProjectedSpending / budget.TotalBudgetAmount) * 100
                });
            }

            // Check for period ending soon
            if (budget.DaysRemaining <= 7 && budget.DaysRemaining > 0)
            {
                alerts.Add(new BudgetAlert
                {
                    Budget = budget,
                    AlertType = BudgetAlertType.PeriodEnding,
                    Message = $"Budget '{budget.Name}' period ends in {budget.DaysRemaining} day(s)",
                    Amount = budget.RemainingAmount,
                    Percentage = budget.UtilizationPercentage
                });
            }

            // Check for no activity
            if (budget.SpentAmount == 0 && budget.DaysRemaining < (budget.EndDate - budget.StartDate).Days / 2)
            {
                alerts.Add(new BudgetAlert
                {
                    Budget = budget,
                    AlertType = BudgetAlertType.NoActivity,
                    Message = $"Budget '{budget.Name}' has no spending activity",
                    Amount = 0,
                    Percentage = 0
                });
            }
        }

        _logger.LogInformation("Generated {Count} budget alerts", alerts.Count);
        return alerts;
    }

    public async Task<IEnumerable<Budget>> SearchBudgetsAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return Enumerable.Empty<Budget>();
        }

        return await _budgetRepository.SearchByNameAsync(searchTerm, cancellationToken);
    }

    public async Task<bool> SetBudgetActiveStatusAsync(int budgetId, bool isActive, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Setting budget {BudgetId} active status to {IsActive}", budgetId, isActive);

        var budget = await _budgetRepository.GetByIdAsync(budgetId, cancellationToken);
        if (budget == null)
        {
            _logger.LogWarning("Budget not found: {BudgetId}", budgetId);
            return false;
        }

        budget.IsActive = isActive;
        await _budgetRepository.UpdateAsync(budget, cancellationToken);
        await _budgetRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Budget active status updated: {BudgetId} = {IsActive}", budgetId, isActive);
        return true;
    }

    public async Task<BusinessValidationResult> ValidateBudgetAsync(Budget budget, CancellationToken cancellationToken = default)
    {
        var result = new BusinessValidationResult { IsValid = true };

        // Basic validation
        if (!budget.IsValid())
        {
            result.IsValid = false;
            result.AddError("Budget data is invalid");
        }

        // Check for duplicate budget names in the same period
        var existingBudgets = await _budgetRepository.GetByDateRangeAsync(budget.StartDate, budget.EndDate, cancellationToken);
        var duplicateName = existingBudgets.Any(b => 
            b.Id != budget.Id && 
            string.Equals(b.Name.Trim(), budget.Name.Trim(), StringComparison.OrdinalIgnoreCase));

        if (duplicateName)
        {
            result.IsValid = false;
            result.AddError($"A budget with the name '{budget.Name}' already exists for this period");
        }

        // Verify category exists if specified
        if (budget.CategoryId.HasValue)
        {
            var category = await _categoryRepository.GetByIdAsync(budget.CategoryId.Value, cancellationToken);
            if (category == null)
            {
                result.IsValid = false;
                result.AddError($"Category {budget.CategoryId} not found");
            }
            else if (category.CategoryType != TransactionType.Expense)
            {
                result.AddWarning("Budgets are typically created for expense categories");
            }
        }

        // Check for overlapping budgets for the same category
        if (budget.CategoryId.HasValue)
        {
            var categoryBudgets = await _budgetRepository.GetByCategoryAsync(budget.CategoryId.Value, cancellationToken);
            var overlapping = categoryBudgets.Any(b => 
                b.Id != budget.Id && 
                b.IsActive &&
                ((budget.StartDate >= b.StartDate && budget.StartDate <= b.EndDate) ||
                 (budget.EndDate >= b.StartDate && budget.EndDate <= b.EndDate) ||
                 (budget.StartDate <= b.StartDate && budget.EndDate >= b.EndDate)));

            if (overlapping)
            {
                result.AddWarning($"Another active budget exists for this category during the same period");
            }
        }

        // Business rule validations
        if (budget.Amount > 100000)
        {
            result.AddWarning("Budget amount seems unusually high");
        }

        if (budget.AlertThreshold.HasValue && budget.AlertThreshold.Value < 50)
        {
            result.AddWarning("Alert threshold below 50% may generate frequent notifications");
        }

        var periodDays = (budget.EndDate - budget.StartDate).Days + 1;
        if (periodDays < 1)
        {
            result.IsValid = false;
            result.AddError("Budget period must be at least 1 day");
        }
        else if (periodDays > 366)
        {
            result.AddWarning("Budget period is longer than a year");
        }

        return result;
    }

    private async Task<decimal> CalculateSpentAmountAsync(int categoryId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var transactions = await _transactionRepository.GetByCategoryIdAsync(categoryId, cancellationToken);
        return transactions
            .Where(t => !t.IsDeleted && 
                       t.Date >= startDate && 
                       t.Date <= endDate && 
                       t.Type == TransactionType.Expense)
            .Sum(t => t.Amount);
    }
}




