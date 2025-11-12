using FinTrack.Core.Entities;
using FinTrack.Core.Enums;
using FinTrack.Infrastructure.Data;
using FinTrack.Infrastructure.Repositories;
using FinTrack.Tests.Unit.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinTrack.Tests.Unit.Infrastructure.Repositories;

/// <summary>
/// Unit tests for GoalRepository
/// </summary>
public class GoalRepositoryTests : IDisposable
{
    private readonly FinTrackDbContext _context;
    private readonly Mock<ILogger<GoalRepository>> _mockLogger;
    private readonly GoalRepository _repository;

    public GoalRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<FinTrackDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new FinTrackDbContext(options);
        _mockLogger = new Mock<ILogger<GoalRepository>>();
        _repository = new GoalRepository(_context, _mockLogger.Object);
    }

    [Fact]
    public async Task GetByTypeAsync_WithValidType_ShouldReturnGoalsOfSpecificType()
    {
        // Arrange
        var savingsGoal = TestDataBuilder.CreateGoal("Savings Goal", 1000m, GoalType.Savings);
        var debtGoal = TestDataBuilder.CreateGoal("Debt Goal", 2000m, GoalType.DebtPayoff);
        var investmentGoal = TestDataBuilder.CreateGoal("Investment Goal", 5000m, GoalType.Investment);

        _context.Goals.AddRange(savingsGoal, debtGoal, investmentGoal);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByTypeAsync(GoalType.Savings);

        // Assert
        Assert.Single(result);
        Assert.Equal(GoalType.Savings, result.First().Type);
        Assert.Equal("Savings Goal", result.First().Name);
    }

    [Fact]
    public async Task GetActiveGoalsAsync_ShouldReturnOnlyIncompleteGoals()
    {
        // Arrange
        var activeGoal = TestDataBuilder.CreateGoal("Active Goal", 1000m);
        var completedGoal = TestDataBuilder.CreateGoal("Completed Goal", 2000m);
        completedGoal.IsCompleted = true;
        completedGoal.CompletedDate = DateTime.UtcNow;

        _context.Goals.AddRange(activeGoal, completedGoal);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveGoalsAsync();

        // Assert
        Assert.Single(result);
        Assert.False(result.First().IsCompleted);
        Assert.Equal("Active Goal", result.First().Name);
    }

    [Fact]
    public async Task GetCompletedGoalsAsync_ShouldReturnOnlyCompletedGoals()
    {
        // Arrange
        var activeGoal = TestDataBuilder.CreateGoal("Active Goal", 1000m);
        var completedGoal = TestDataBuilder.CreateGoal("Completed Goal", 2000m);
        completedGoal.IsCompleted = true;
        completedGoal.CompletedDate = DateTime.UtcNow;

        _context.Goals.AddRange(activeGoal, completedGoal);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetCompletedGoalsAsync();

        // Assert
        Assert.Single(result);
        Assert.True(result.First().IsCompleted);
        Assert.Equal("Completed Goal", result.First().Name);
    }

    [Fact]
    public async Task GetByPriorityAsync_ShouldReturnGoalsWithSpecificPriority()
    {
        // Arrange
        var highPriorityGoal = TestDataBuilder.CreateGoal("High Priority", 1000m);
        highPriorityGoal.Priority = 1;
        var lowPriorityGoal = TestDataBuilder.CreateGoal("Low Priority", 2000m);
        lowPriorityGoal.Priority = 5;

        _context.Goals.AddRange(highPriorityGoal, lowPriorityGoal);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByPriorityAsync(1);

        // Assert
        Assert.Single(result);
        Assert.Equal(1, result.First().Priority);
        Assert.Equal("High Priority", result.First().Name);
    }

    [Fact]
    public async Task GetByLinkedAccountAsync_ShouldReturnGoalsLinkedToAccount()
    {
        // Arrange
        var account = TestDataBuilder.CreateAccount("Test Account");
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        var linkedGoal = TestDataBuilder.CreateGoal("Linked Goal", 1000m, accountId: account.Id);
        linkedGoal.LinkedAccountId = account.Id; // Set the LinkedAccountId property
        var unlinkedGoal = TestDataBuilder.CreateGoal("Unlinked Goal", 2000m, accountId: null);

        _context.Goals.AddRange(linkedGoal, unlinkedGoal);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByLinkedAccountAsync(account.Id);

        // Assert
        Assert.Single(result);
        Assert.Equal(account.Id, result.First().LinkedAccountId);
        Assert.Equal("Linked Goal", result.First().Name);
    }

    [Fact]
    public async Task GetByTargetDateRangeAsync_ShouldReturnGoalsInDateRange()
    {
        // Arrange
        var startDate = DateTime.Today;
        var endDate = DateTime.Today.AddMonths(6);

        var goalInRange = TestDataBuilder.CreateGoal("In Range", 1000m, targetDate: startDate.AddMonths(3));
        var goalOutOfRange = TestDataBuilder.CreateGoal("Out of Range", 2000m, targetDate: endDate.AddMonths(1));

        _context.Goals.AddRange(goalInRange, goalOutOfRange);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByTargetDateRangeAsync(startDate, endDate);

        // Assert
        Assert.Single(result);
        Assert.Equal("In Range", result.First().Name);
    }

    [Fact]
    public async Task GetOverdueGoalsAsync_ShouldReturnOverdueIncompleteGoals()
    {
        // Arrange
        var overdueGoal = TestDataBuilder.CreateGoal("Overdue Goal", 1000m, targetDate: DateTime.Today.AddDays(-1));
        var futureGoal = TestDataBuilder.CreateGoal("Future Goal", 2000m, targetDate: DateTime.Today.AddDays(1));
        var completedOverdueGoal = TestDataBuilder.CreateGoal("Completed Overdue", 3000m, targetDate: DateTime.Today.AddDays(-1));
        completedOverdueGoal.IsCompleted = true;

        _context.Goals.AddRange(overdueGoal, futureGoal, completedOverdueGoal);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetOverdueGoalsAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("Overdue Goal", result.First().Name);
        Assert.False(result.First().IsCompleted);
    }

    [Fact]
    public async Task GetGoalsDueSoonAsync_ShouldReturnGoalsDueWithinSpecifiedDays()
    {
        // Arrange
        var dueSoonGoal = TestDataBuilder.CreateGoal("Due Soon", 1000m, targetDate: DateTime.Today.AddDays(15));
        var farFutureGoal = TestDataBuilder.CreateGoal("Far Future", 2000m, targetDate: DateTime.Today.AddDays(45));

        _context.Goals.AddRange(dueSoonGoal, farFutureGoal);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetGoalsDueSoonAsync(30);

        // Assert
        Assert.Single(result);
        Assert.Equal("Due Soon", result.First().Name);
    }

    [Fact]
    public async Task UpdateProgressAsync_WithValidGoal_ShouldUpdateCurrentAmount()
    {
        // Arrange
        var goal = TestDataBuilder.CreateGoal("Test Goal", 1000m);
        _context.Goals.Add(goal);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.UpdateProgressAsync(goal.Id, 500m);

        // Assert
        Assert.True(result);
        var updatedGoal = await _context.Goals.FindAsync(goal.Id);
        Assert.Equal(500m, updatedGoal!.CurrentAmount);
    }

    [Fact]
    public async Task UpdateProgressAsync_WhenGoalReachesTarget_ShouldMarkAsCompleted()
    {
        // Arrange
        var goal = TestDataBuilder.CreateGoal("Test Goal", 1000m);
        _context.Goals.Add(goal);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.UpdateProgressAsync(goal.Id, 1000m);

        // Assert
        Assert.True(result);
        var updatedGoal = await _context.Goals.FindAsync(goal.Id);
        Assert.True(updatedGoal!.IsCompleted);
        Assert.NotNull(updatedGoal.CompletedDate);
    }

    [Fact]
    public async Task UpdateProgressAsync_WithNonExistentGoal_ShouldReturnFalse()
    {
        // Act
        var result = await _repository.UpdateProgressAsync(999, 500m);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task MarkAsCompletedAsync_WithValidGoal_ShouldMarkAsCompleted()
    {
        // Arrange
        var goal = TestDataBuilder.CreateGoal("Test Goal", 1000m);
        _context.Goals.Add(goal);
        await _context.SaveChangesAsync();

        var completionDate = DateTime.UtcNow;

        // Act
        var result = await _repository.MarkAsCompletedAsync(goal.Id, completionDate);

        // Assert
        Assert.True(result);
        var updatedGoal = await _context.Goals.FindAsync(goal.Id);
        Assert.True(updatedGoal!.IsCompleted);
        Assert.Equal(completionDate, updatedGoal.CompletedDate);
        Assert.Equal(1000m, updatedGoal.CurrentAmount); // Should be set to target amount
    }

    [Fact]
    public async Task MarkAsCompletedAsync_WithNonExistentGoal_ShouldReturnFalse()
    {
        // Act
        var result = await _repository.MarkAsCompletedAsync(999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetProgressStatsAsync_ShouldReturnCorrectStatistics()
    {
        // Arrange
        var activeGoal = TestDataBuilder.CreateGoal("Active", 1000m);
        activeGoal.CurrentAmount = 500m;
        
        var completedGoal = TestDataBuilder.CreateGoal("Completed", 2000m);
        completedGoal.IsCompleted = true;
        completedGoal.CurrentAmount = 2000m;
        
        var overdueGoal = TestDataBuilder.CreateGoal("Overdue", 1500m, targetDate: DateTime.Today.AddDays(-1));
        overdueGoal.CurrentAmount = 750m;

        _context.Goals.AddRange(activeGoal, completedGoal, overdueGoal);
        await _context.SaveChangesAsync();

        // Act
        var stats = await _repository.GetProgressStatsAsync();

        // Assert
        Assert.Equal(3, stats.TotalGoals);
        Assert.Equal(2, stats.ActiveGoals); // Active and overdue
        Assert.Equal(1, stats.CompletedGoals);
        Assert.Equal(1, stats.OverdueGoals);
        Assert.Equal(4500m, stats.TotalTargetAmount); // 1000 + 2000 + 1500
        Assert.Equal(3250m, stats.TotalCurrentAmount); // 500 + 2000 + 750
    }

    [Fact]
    public async Task SearchAsync_WithMatchingTerm_ShouldReturnMatchingGoals()
    {
        // Arrange
        var matchingGoal1 = TestDataBuilder.CreateGoal("Emergency Fund", 1000m);
        matchingGoal1.Description = "Build emergency savings";
        
        var matchingGoal2 = TestDataBuilder.CreateGoal("Vacation Fund", 2000m);
        matchingGoal2.Description = "Save for vacation";
        
        var nonMatchingGoal = TestDataBuilder.CreateGoal("Investment", 5000m);
        nonMatchingGoal.Description = "Long term investment";

        _context.Goals.AddRange(matchingGoal1, matchingGoal2, nonMatchingGoal);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync("Fund");

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, g => Assert.Contains("Fund", g.Name));
    }

    [Fact]
    public async Task SearchAsync_WithEmptyTerm_ShouldReturnEmpty()
    {
        // Arrange
        var goal = TestDataBuilder.CreateGoal("Test Goal", 1000m);
        _context.Goals.Add(goal);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchAsync("");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task RecalculateLinkedAccountProgressAsync_ShouldUpdateGoalProgress()
    {
        // Arrange
        var account = TestDataBuilder.CreateAccount("Savings Account", 1500m, AccountType.Savings);
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        var savingsGoal = TestDataBuilder.CreateGoal("Savings Goal", 2000m, GoalType.Savings, account.Id);
        savingsGoal.CurrentAmount = 1000m; // Different from account balance
        
        _context.Goals.Add(savingsGoal);
        await _context.SaveChangesAsync();

        // Act
        var updatedCount = await _repository.RecalculateLinkedAccountProgressAsync();

        // Assert
        Assert.Equal(1, updatedCount);
        var updatedGoal = await _context.Goals.FindAsync(savingsGoal.Id);
        Assert.Equal(1500m, updatedGoal!.CurrentAmount); // Should match account balance
    }

    [Fact]
    public async Task RecalculateLinkedAccountProgressAsync_WithDebtPayoffGoal_ShouldCalculateCorrectly()
    {
        // Arrange
        var account = TestDataBuilder.CreateAccount("Credit Card", -500m, AccountType.CreditCard);
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        var debtGoal = TestDataBuilder.CreateGoal("Pay Off Debt", 1000m, GoalType.DebtPayoff, account.Id);
        debtGoal.CurrentAmount = 0m;
        
        _context.Goals.Add(debtGoal);
        await _context.SaveChangesAsync();

        // Act
        var updatedCount = await _repository.RecalculateLinkedAccountProgressAsync();

        // Assert
        Assert.Equal(1, updatedCount);
        var updatedGoal = await _context.Goals.FindAsync(debtGoal.Id);
        Assert.Equal(500m, updatedGoal!.CurrentAmount); // 1000 - abs(-500) = 500
    }

    [Fact]
    public async Task GetAchievementSummaryAsync_ShouldReturnCorrectSummary()
    {
        // Arrange
        var startDate = DateTime.Today.AddMonths(-1);
        var endDate = DateTime.Today;

        var completedGoal1 = TestDataBuilder.CreateGoal("Goal 1", 1000m);
        completedGoal1.IsCompleted = true;
        completedGoal1.CompletedDate = startDate.AddDays(5);
        
        var completedGoal2 = TestDataBuilder.CreateGoal("Goal 2", 2000m, GoalType.Investment);
        completedGoal2.IsCompleted = true;
        completedGoal2.CompletedDate = startDate.AddDays(10);
        
        var incompleteGoal = TestDataBuilder.CreateGoal("Goal 3", 3000m);

        _context.Goals.AddRange(completedGoal1, completedGoal2, incompleteGoal);
        await _context.SaveChangesAsync();

        // Act
        var summary = await _repository.GetAchievementSummaryAsync(startDate, endDate);

        // Assert
        Assert.Equal(2, summary.GoalsCompleted);
        Assert.Equal(3000m, summary.TotalAmountAchieved);
        Assert.Equal(2, summary.CompletedGoals.Count());
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}