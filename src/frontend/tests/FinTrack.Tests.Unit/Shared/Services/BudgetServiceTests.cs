using FinTrack.Core.Entities;
using FinTrack.Core.Enums;
using FinTrack.Core.Exceptions;
using FinTrack.Core.Interfaces;
using FinTrack.Shared.Models;
using FinTrack.Shared.Services;
using FinTrack.Tests.Unit.Helpers;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinTrack.Tests.Unit.Shared.Services;

/// <summary>
/// Unit tests for BudgetService
/// </summary>
public class BudgetServiceTests : IDisposable
{
    private readonly Mock<IBudgetRepository> _mockBudgetRepository;
    private readonly Mock<ICategoryRepository> _mockCategoryRepository;
    private readonly Mock<ITransactionRepository> _mockTransactionRepository;
    private readonly Mock<ILogger<BudgetService>> _mockLogger;
    private readonly BudgetService _service;

    public BudgetServiceTests()
    {
        _mockBudgetRepository = new Mock<IBudgetRepository>();
        _mockCategoryRepository = new Mock<ICategoryRepository>();
        _mockTransactionRepository = new Mock<ITransactionRepository>();
        _mockLogger = new Mock<ILogger<BudgetService>>();
        
        _service = new BudgetService(
            _mockBudgetRepository.Object,
            _mockCategoryRepository.Object,
            _mockTransactionRepository.Object,
            _mockLogger.Object);
    }

    #region CreateBudgetAsync Tests

    [Fact]
    public async Task CreateBudgetAsync_WithValidBudget_ShouldCreateBudget()
    {
        // Arrange
        var category = TestDataBuilder.CreateCategory("Food", TransactionType.Expense);
        var budget = TestDataBuilder.CreateBudget(500m, category.Id);
        
        _mockCategoryRepository.Setup(x => x.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _mockBudgetRepository.Setup(x => x.AddAsync(It.IsAny<Budget>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);

        // Act
        var result = await _service.CreateBudgetAsync(budget);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(budget.Amount, result.Amount);
        Assert.Equal(budget.CategoryId, result.CategoryId);
        _mockBudgetRepository.Verify(x => x.AddAsync(It.IsAny<Budget>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateBudgetAsync_WithNullBudget_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.CreateBudgetAsync(null!));
    }

    [Fact]
    public async Task CreateBudgetAsync_WithInvalidBudget_ShouldThrowBusinessRuleException()
    {
        // Arrange
        var budget = TestDataBuilder.CreateBudget(0m, 1); // Invalid amount

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.CreateBudgetAsync(budget));
        Assert.Equal("InvalidBudget", exception.RuleName);
    }

    [Fact]
    public async Task CreateBudgetAsync_WithNonExistentCategory_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var budget = TestDataBuilder.CreateBudget(500m, 999);
        
        _mockCategoryRepository.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.CreateBudgetAsync(budget));
    }

    [Fact]
    public async Task CreateBudgetAsync_WithOverlappingBudget_ShouldThrowBusinessRuleException()
    {
        // Arrange
        var category = TestDataBuilder.CreateCategory("Food", TransactionType.Expense);
        var budget = TestDataBuilder.CreateBudget(500m, category.Id);
        var existingBudgets = new[] { TestDataBuilder.CreateBudget(300m, category.Id) };
        
        _mockCategoryRepository.Setup(x => x.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _mockBudgetRepository.Setup(x => x.GetByDateRangeAsync(budget.StartDate, budget.EndDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBudgets);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.CreateBudgetAsync(budget));
        Assert.Contains("overlapping budget", exception.Message.ToLower());
    }

    #endregion

    #region UpdateBudgetAsync Tests

    [Fact]
    public async Task UpdateBudgetAsync_WithValidBudget_ShouldUpdateBudget()
    {
        // Arrange
        var budget = TestDataBuilder.CreateBudget(600m, 1);
        budget.Id = 1;
        
        _mockBudgetRepository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);
        _mockBudgetRepository.Setup(x => x.UpdateAsync(It.IsAny<Budget>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);

        // Act
        var result = await _service.UpdateBudgetAsync(budget);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(600m, result.Amount);
        _mockBudgetRepository.Verify(x => x.UpdateAsync(It.IsAny<Budget>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateBudgetAsync_WithNonExistentBudget_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var budget = TestDataBuilder.CreateBudget(500m, 1);
        budget.Id = 999;
        
        _mockBudgetRepository.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Budget?)null);

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.UpdateBudgetAsync(budget));
    }

    #endregion

    #region DeleteBudgetAsync Tests

    [Fact]
    public async Task DeleteBudgetAsync_WithValidId_ShouldDeleteBudget()
    {
        // Arrange
        var budget = TestDataBuilder.CreateBudget(500m, 1);
        budget.Id = 1;
        
        _mockBudgetRepository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);
        _mockBudgetRepository.Setup(x => x.DeleteAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteBudgetAsync(1);

        // Assert
        Assert.True(result);
        _mockBudgetRepository.Verify(x => x.DeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteBudgetAsync_WithNonExistentId_ShouldReturnFalse()
    {
        // Arrange
        _mockBudgetRepository.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Budget?)null);

        // Act
        var result = await _service.DeleteBudgetAsync(999);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Budget Tracking Tests

    [Fact]
    public async Task UpdateBudgetSpendingAsync_WithValidBudget_ShouldUpdateSpentAmount()
    {
        // Arrange
        var budget = TestDataBuilder.CreateBudget(500m, 1);
        budget.Id = 1;
        var spentAmount = 250m;
        
        _mockBudgetRepository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);
        _mockTransactionRepository.Setup(x => x.GetByCategoryAndDateRangeAsync(
            budget.CategoryId!.Value, budget.StartDate, budget.EndDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { TestDataBuilder.CreateTransaction(spentAmount, "Test", TransactionType.Expense, budget.CategoryId.Value, 1) });
        _mockBudgetRepository.Setup(x => x.UpdateSpentAmountAsync(1, spentAmount, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.UpdateBudgetSpendingAsync(1);

        // Assert
        Assert.True(result);
        _mockBudgetRepository.Verify(x => x.UpdateSpentAmountAsync(1, spentAmount, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RecalculateAllBudgetSpendingAsync_ShouldUpdateAllActiveBudgets()
    {
        // Arrange
        var budgets = new[]
        {
            TestDataBuilder.CreateBudget(500m, 1),
            TestDataBuilder.CreateBudget(300m, 2)
        };
        
        _mockBudgetRepository.Setup(x => x.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(budgets);
        _mockBudgetRepository.Setup(x => x.RecalculateAllSpentAmountsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        // Act
        var result = await _service.RecalculateAllBudgetSpendingAsync();

        // Assert
        Assert.Equal(2, result);
        _mockBudgetRepository.Verify(x => x.RecalculateAllSpentAmountsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CheckBudgetAlertsAsync_ShouldReturnExceededBudgets()
    {
        // Arrange
        var exceededBudgets = new[]
        {
            TestDataBuilder.CreateBudget(500m, 1)
        };
        exceededBudgets[0].SpentAmount = 600m; // Exceeded
        
        _mockBudgetRepository.Setup(x => x.GetExceededBudgetsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(exceededBudgets);

        // Act
        var result = await _service.CheckBudgetAlertsAsync();

        // Assert
        Assert.Single(result);
        Assert.True(result.First().IsExceeded);
    }

    [Fact]
    public async Task GetBudgetAlertsAsync_ShouldReturnAlertsForThresholdBudgets()
    {
        // Arrange
        var alertBudgets = new[]
        {
            TestDataBuilder.CreateBudget(500m, 1)
        };
        alertBudgets[0].SpentAmount = 400m; // 80% of budget
        alertBudgets[0].AlertThreshold = 0.75m; // 75% threshold
        
        _mockBudgetRepository.Setup(x => x.GetBudgetsAtAlertThresholdAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(alertBudgets);

        // Act
        var result = await _service.GetBudgetAlertsAsync();

        // Assert
        Assert.Single(result);
        Assert.Contains("alert", result.First().Message.ToLower());
    }

    #endregion

    #region Query Tests

    [Fact]
    public async Task GetCurrentBudgetsAsync_ShouldReturnCurrentBudgets()
    {
        // Arrange
        var budgets = new[]
        {
            TestDataBuilder.CreateBudget(500m, 1),
            TestDataBuilder.CreateBudget(300m, 2)
        };
        
        _mockBudgetRepository.Setup(x => x.GetCurrentBudgetsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(budgets);

        // Act
        var result = await _service.GetCurrentBudgetsAsync();

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetBudgetsByPeriodAsync_ShouldReturnBudgetsForPeriod()
    {
        // Arrange
        var budgets = new[] { TestDataBuilder.CreateBudget(500m, 1, BudgetPeriod.Monthly) };
        
        _mockBudgetRepository.Setup(x => x.GetByPeriodAsync(BudgetPeriod.Monthly, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budgets);

        // Act
        var result = await _service.GetBudgetsByPeriodAsync(BudgetPeriod.Monthly);

        // Assert
        Assert.Single(result);
        Assert.Equal(BudgetPeriod.Monthly, result.First().Period);
    }

    [Fact]
    public async Task GetBudgetsByCategoryAsync_ShouldReturnCategoryBudgets()
    {
        // Arrange
        var budgets = new[] { TestDataBuilder.CreateBudget(500m, 1) };
        
        _mockBudgetRepository.Setup(x => x.GetByCategoryAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(budgets);

        // Act
        var result = await _service.GetBudgetsByCategoryAsync(1);

        // Assert
        Assert.Single(result);
        Assert.Equal(1, result.First().CategoryId);
    }

    [Fact]
    public async Task GetBudgetPerformanceAsync_ShouldReturnPerformanceData()
    {
        // Arrange
        var startDate = DateTime.Today.AddDays(-30);
        var endDate = DateTime.Today;
        var performance = new[]
        {
            new BudgetPerformance
            {
                Budget = TestDataBuilder.CreateBudget(500m, 1),
                UtilizationPercentage = 60m,
                RemainingAmount = 200m,
                IsOnTrack = true
            }
        };
        
        _mockBudgetRepository.Setup(x => x.GetBudgetPerformanceAsync(startDate, endDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(performance);

        // Act
        var result = await _service.GetBudgetPerformanceAsync(startDate, endDate);

        // Assert
        Assert.Single(result);
        Assert.Equal(60m, result.First().UtilizationPercentage);
        Assert.True(result.First().IsOnTrack);
    }

    #endregion

    #region Budget Creation Helpers Tests

    [Fact]
    public async Task CreateMonthlyBudgetAsync_ShouldCreateBudgetForCurrentMonth()
    {
        // Arrange
        var category = TestDataBuilder.CreateCategory("Food", TransactionType.Expense);
        var amount = 500m;
        
        _mockCategoryRepository.Setup(x => x.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _mockBudgetRepository.Setup(x => x.AddAsync(It.IsAny<Budget>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Budget b, CancellationToken ct) => b);

        // Act
        var result = await _service.CreateMonthlyBudgetAsync(category.Id, amount, "Monthly Food Budget");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(amount, result.Amount);
        Assert.Equal(BudgetPeriod.Monthly, result.Period);
        Assert.Equal(category.Id, result.CategoryId);
        
        // Verify dates are for current month
        var currentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        Assert.Equal(currentMonth, result.StartDate);
        Assert.Equal(currentMonth.AddMonths(1).AddDays(-1), result.EndDate);
    }

    [Fact]
    public async Task CreateQuarterlyBudgetAsync_ShouldCreateBudgetForCurrentQuarter()
    {
        // Arrange
        var category = TestDataBuilder.CreateCategory("Shopping", TransactionType.Expense);
        var amount = 1500m;
        
        _mockCategoryRepository.Setup(x => x.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _mockBudgetRepository.Setup(x => x.AddAsync(It.IsAny<Budget>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Budget b, CancellationToken ct) => b);

        // Act
        var result = await _service.CreateQuarterlyBudgetAsync(category.Id, amount, "Quarterly Shopping Budget");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(amount, result.Amount);
        Assert.Equal(BudgetPeriod.Quarterly, result.Period);
        Assert.Equal(category.Id, result.CategoryId);
    }

    [Fact]
    public async Task CreateYearlyBudgetAsync_ShouldCreateBudgetForCurrentYear()
    {
        // Arrange
        var category = TestDataBuilder.CreateCategory("Travel", TransactionType.Expense);
        var amount = 5000m;
        
        _mockCategoryRepository.Setup(x => x.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _mockBudgetRepository.Setup(x => x.AddAsync(It.IsAny<Budget>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Budget b, CancellationToken ct) => b);

        // Act
        var result = await _service.CreateYearlyBudgetAsync(category.Id, amount, "Yearly Travel Budget");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(amount, result.Amount);
        Assert.Equal(BudgetPeriod.Yearly, result.Period);
        Assert.Equal(category.Id, result.CategoryId);
        
        // Verify dates are for current year
        var currentYear = new DateTime(DateTime.Today.Year, 1, 1);
        Assert.Equal(currentYear, result.StartDate);
        Assert.Equal(new DateTime(DateTime.Today.Year, 12, 31), result.EndDate);
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task GetBudgetUtilizationStatsAsync_ShouldReturnUtilizationStatistics()
    {
        // Arrange
        var stats = new BudgetUtilizationStats
        {
            TotalBudgets = 5,
            ActiveBudgets = 4,
            ExceededBudgets = 1,
            BudgetsAtAlert = 2,
            TotalBudgetAmount = 2500m,
            TotalSpentAmount = 1800m,
            AverageUtilization = 72m,
            TotalRemainingAmount = 700m
        };
        
        _mockBudgetRepository.Setup(x => x.GetUtilizationStatsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(stats);

        // Act
        var result = await _service.GetBudgetUtilizationStatsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.TotalBudgets);
        Assert.Equal(4, result.ActiveBudgets);
        Assert.Equal(1, result.ExceededBudgets);
        Assert.Equal(2, result.BudgetsAtAlert);
        Assert.Equal(2500m, result.TotalBudgetAmount);
        Assert.Equal(1800m, result.TotalSpentAmount);
        Assert.Equal(72m, result.AverageUtilization);
        Assert.Equal(700m, result.TotalRemainingAmount);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task ValidateBudgetAsync_WithValidBudget_ShouldReturnValidResult()
    {
        // Arrange
        var budget = TestDataBuilder.CreateBudget(500m, 1);

        // Act
        var result = await _service.ValidateBudgetAsync(budget);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ValidateBudgetAsync_WithInvalidAmount_ShouldReturnInvalidResult()
    {
        // Arrange
        var budget = TestDataBuilder.CreateBudget(0m, 1);

        // Act
        var result = await _service.ValidateBudgetAsync(budget);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsValid);
        Assert.Contains("Budget data is invalid", result.Errors);
    }

    [Fact]
    public async Task ValidateBudgetAsync_WithInvalidDateRange_ShouldReturnInvalidResult()
    {
        // Arrange
        var budget = TestDataBuilder.CreateBudget(500m, 1, BudgetPeriod.Monthly, DateTime.Today, DateTime.Today.AddDays(-1));

        // Act
        var result = await _service.ValidateBudgetAsync(budget);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsValid);
        Assert.Contains("Budget period must be at least 1 day", result.Errors);
    }

    #endregion

    public void Dispose()
    {
        // Cleanup if needed
    }
}