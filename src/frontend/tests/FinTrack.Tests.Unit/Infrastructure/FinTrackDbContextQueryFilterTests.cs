using FinTrack.Tests.Unit.Helpers;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Tests.Unit.Infrastructure;

/// <summary>
/// Tests for global query filter functionality in FinTrackDbContext
/// </summary>
public class FinTrackDbContextQueryFilterTests : DbContextTestBase
{
    [Fact]
    public async Task GlobalQueryFilter_ExcludesSoftDeletedEntities_ForAccounts()
    {
        // Arrange
        var activeAccount = TestDataBuilder.CreateTestAccount("Active Account");
        var deletedAccount = TestDataBuilder.CreateTestAccount("Deleted Account");
        deletedAccount.IsDeleted = true;

        Context.Accounts.AddRange(activeAccount, deletedAccount);
        await Context.SaveChangesAsync();

        // Act
        var accounts = await Context.Accounts.ToListAsync();

        // Assert
        Assert.Single(accounts);
        Assert.Equal("Active Account", accounts[0].Name);
        Assert.DoesNotContain(accounts, a => a.Name == "Deleted Account");
    }

    [Fact]
    public async Task GlobalQueryFilter_ExcludesSoftDeletedEntities_ForTransactions()
    {
        // Arrange
        var account = TestDataBuilder.CreateTestAccount();
        var category = TestDataBuilder.CreateTestCategory();
        Context.Accounts.Add(account);
        Context.Categories.Add(category);
        await Context.SaveChangesAsync();

        var activeTransaction = TestDataBuilder.CreateTestTransaction(
            description: "Active Transaction",
            accountId: account.Id,
            categoryId: category.Id);
        var deletedTransaction = TestDataBuilder.CreateTestTransaction(
            description: "Deleted Transaction",
            accountId: account.Id,
            categoryId: category.Id);
        deletedTransaction.IsDeleted = true;

        Context.Transactions.AddRange(activeTransaction, deletedTransaction);
        await Context.SaveChangesAsync();

        // Act
        var transactions = await Context.Transactions.ToListAsync();

        // Assert
        Assert.Single(transactions);
        Assert.Equal("Active Transaction", transactions[0].Description);
        Assert.DoesNotContain(transactions, t => t.Description == "Deleted Transaction");
    }

    [Fact]
    public async Task GlobalQueryFilter_ExcludesSoftDeletedEntities_ForCategories()
    {
        // Arrange
        var activeCategory = TestDataBuilder.CreateTestCategory("Active Category");
        var deletedCategory = TestDataBuilder.CreateTestCategory("Deleted Category");
        deletedCategory.IsDeleted = true;

        Context.Categories.AddRange(activeCategory, deletedCategory);
        await Context.SaveChangesAsync();

        // Act
        var categories = await Context.Categories.Where(c => !c.IsSystem).ToListAsync();

        // Assert
        Assert.Single(categories);
        Assert.Equal("Active Category", categories[0].Name);
        Assert.DoesNotContain(categories, c => c.Name == "Deleted Category");
    }

    [Fact]
    public async Task GlobalQueryFilter_ExcludesSoftDeletedEntities_ForBudgets()
    {
        // Arrange
        var activeBudget = TestDataBuilder.CreateTestBudget("Active Budget");
        var deletedBudget = TestDataBuilder.CreateTestBudget("Deleted Budget");
        deletedBudget.IsDeleted = true;

        Context.Budgets.AddRange(activeBudget, deletedBudget);
        await Context.SaveChangesAsync();

        // Act
        var budgets = await Context.Budgets.ToListAsync();

        // Assert
        Assert.Single(budgets);
        Assert.Equal("Active Budget", budgets[0].Name);
        Assert.DoesNotContain(budgets, b => b.Name == "Deleted Budget");
    }

    [Fact]
    public async Task GlobalQueryFilter_ExcludesSoftDeletedEntities_ForGoals()
    {
        // Arrange
        var activeGoal = TestDataBuilder.CreateTestGoal("Active Goal");
        var deletedGoal = TestDataBuilder.CreateTestGoal("Deleted Goal");
        deletedGoal.IsDeleted = true;

        Context.Goals.AddRange(activeGoal, deletedGoal);
        await Context.SaveChangesAsync();

        // Act
        var goals = await Context.Goals.ToListAsync();

        // Assert
        Assert.Single(goals);
        Assert.Equal("Active Goal", goals[0].Name);
        Assert.DoesNotContain(goals, g => g.Name == "Deleted Goal");
    }

    [Fact]
    public async Task GlobalQueryFilter_ExcludesSoftDeletedEntities_ForGoalMilestones()
    {
        // Arrange
        var goal = TestDataBuilder.CreateTestGoal();
        Context.Goals.Add(goal);
        await Context.SaveChangesAsync();

        var activeMilestone = TestDataBuilder.CreateTestGoalMilestone(goal.Id, "Active Milestone");
        var deletedMilestone = TestDataBuilder.CreateTestGoalMilestone(goal.Id, "Deleted Milestone");
        deletedMilestone.IsDeleted = true;

        Context.GoalMilestones.AddRange(activeMilestone, deletedMilestone);
        await Context.SaveChangesAsync();

        // Act
        var milestones = await Context.GoalMilestones.ToListAsync();

        // Assert
        Assert.Single(milestones);
        Assert.Equal("Active Milestone", milestones[0].Name);
        Assert.DoesNotContain(milestones, m => m.Name == "Deleted Milestone");
    }

    [Fact]
    public async Task IgnoreQueryFilters_IncludesSoftDeletedEntities()
    {
        // Arrange
        var activeAccount = TestDataBuilder.CreateTestAccount("Active Account");
        var deletedAccount = TestDataBuilder.CreateTestAccount("Deleted Account");
        deletedAccount.IsDeleted = true;

        Context.Accounts.AddRange(activeAccount, deletedAccount);
        await Context.SaveChangesAsync();

        // Act
        var allAccounts = await Context.Accounts.IgnoreQueryFilters().ToListAsync();

        // Assert
        Assert.Equal(2, allAccounts.Count);
        Assert.Contains(allAccounts, a => a.Name == "Active Account");
        Assert.Contains(allAccounts, a => a.Name == "Deleted Account");
    }

    [Fact]
    public async Task GlobalQueryFilter_WorksWithNavigationProperties()
    {
        // Arrange
        var account = TestDataBuilder.CreateTestAccount();
        var category = TestDataBuilder.CreateTestCategory();
        Context.Accounts.Add(account);
        Context.Categories.Add(category);
        await Context.SaveChangesAsync();

        var activeTransaction = TestDataBuilder.CreateTestTransaction(
            description: "Active Transaction",
            accountId: account.Id,
            categoryId: category.Id);
        var deletedTransaction = TestDataBuilder.CreateTestTransaction(
            description: "Deleted Transaction",
            accountId: account.Id,
            categoryId: category.Id);
        deletedTransaction.IsDeleted = true;

        Context.Transactions.AddRange(activeTransaction, deletedTransaction);
        await Context.SaveChangesAsync();

        // Act
        var accountWithTransactions = await Context.Accounts
            .Include(a => a.Transactions)
            .FirstAsync(a => a.Id == account.Id);

        // Assert
        Assert.Single(accountWithTransactions.Transactions);
        Assert.Equal("Active Transaction", accountWithTransactions.Transactions.First().Description);
    }

    [Fact]
    public async Task GlobalQueryFilter_WorksWithFind()
    {
        // Arrange
        var deletedAccount = TestDataBuilder.CreateTestAccount("Deleted Account");
        deletedAccount.IsDeleted = true;

        Context.Accounts.Add(deletedAccount);
        await Context.SaveChangesAsync();
        var accountId = deletedAccount.Id;

        // Clear context to ensure fresh query
        DetachAllEntities();

        // Act
        var foundAccount = await Context.Accounts.FindAsync(accountId);

        // Assert
        Assert.Null(foundAccount); // Should not find deleted entity
    }

    [Fact]
    public async Task GlobalQueryFilter_WorksWithFirstOrDefault()
    {
        // Arrange
        var deletedAccount = TestDataBuilder.CreateTestAccount("Deleted Account");
        deletedAccount.IsDeleted = true;

        Context.Accounts.Add(deletedAccount);
        await Context.SaveChangesAsync();

        // Act
        var foundAccount = await Context.Accounts
            .FirstOrDefaultAsync(a => a.Name == "Deleted Account");

        // Assert
        Assert.Null(foundAccount); // Should not find deleted entity
    }

    [Fact]
    public async Task GlobalQueryFilter_WorksWithCount()
    {
        // Arrange
        var activeAccount = TestDataBuilder.CreateTestAccount("Active Account");
        var deletedAccount = TestDataBuilder.CreateTestAccount("Deleted Account");
        deletedAccount.IsDeleted = true;

        Context.Accounts.AddRange(activeAccount, deletedAccount);
        await Context.SaveChangesAsync();

        // Act
        var count = await Context.Accounts.CountAsync();
        var countWithIgnore = await Context.Accounts.IgnoreQueryFilters().CountAsync();

        // Assert
        Assert.Equal(1, count); // Should only count active entities
        Assert.Equal(2, countWithIgnore); // Should count all entities when ignoring filters
    }

    [Fact]
    public async Task GlobalQueryFilter_WorksWithAny()
    {
        // Arrange
        var deletedAccount = TestDataBuilder.CreateTestAccount("Deleted Account");
        deletedAccount.IsDeleted = true;

        Context.Accounts.Add(deletedAccount);
        await Context.SaveChangesAsync();

        // Act
        var hasAny = await Context.Accounts.AnyAsync(a => a.Name == "Deleted Account");
        var hasAnyWithIgnore = await Context.Accounts
            .IgnoreQueryFilters()
            .AnyAsync(a => a.Name == "Deleted Account");

        // Assert
        Assert.False(hasAny); // Should not find deleted entity
        Assert.True(hasAnyWithIgnore); // Should find deleted entity when ignoring filters
    }

    [Fact]
    public async Task GlobalQueryFilter_WorksWithComplexQueries()
    {
        // Arrange
        var account = TestDataBuilder.CreateTestAccount();
        var category = TestDataBuilder.CreateTestCategory();
        Context.Accounts.Add(account);
        Context.Categories.Add(category);
        await Context.SaveChangesAsync();

        var activeTransaction = TestDataBuilder.CreateTestTransaction(
            amount: 100,
            accountId: account.Id,
            categoryId: category.Id);
        var deletedTransaction = TestDataBuilder.CreateTestTransaction(
            amount: 200,
            accountId: account.Id,
            categoryId: category.Id);
        deletedTransaction.IsDeleted = true;

        Context.Transactions.AddRange(activeTransaction, deletedTransaction);
        await Context.SaveChangesAsync();

        // Act
        var totalAmount = await Context.Transactions
            .Where(t => t.AccountId == account.Id)
            .SumAsync(t => t.Amount);

        var totalAmountWithIgnore = await Context.Transactions
            .IgnoreQueryFilters()
            .Where(t => t.AccountId == account.Id)
            .SumAsync(t => t.Amount);

        // Assert
        Assert.Equal(100, totalAmount); // Should only sum active transactions
        Assert.Equal(300, totalAmountWithIgnore); // Should sum all transactions when ignoring filters
    }

    [Fact]
    public async Task GlobalQueryFilter_AppliesAfterSoftDelete()
    {
        // Arrange
        var account = TestDataBuilder.CreateTestAccount("Test Account");
        Context.Accounts.Add(account);
        await Context.SaveChangesAsync();

        // Verify account is visible
        var visibleAccount = await Context.Accounts.FindAsync(account.Id);
        Assert.NotNull(visibleAccount);

        // Act - Soft delete the account
        Context.Accounts.Remove(account);
        await Context.SaveChangesAsync();

        // Clear context to ensure fresh query
        DetachAllEntities();

        // Assert - Account should no longer be visible
        var hiddenAccount = await Context.Accounts.FindAsync(account.Id);
        Assert.Null(hiddenAccount);

        // But should be visible when ignoring filters
        var deletedAccount = await Context.Accounts
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.Id == account.Id);
        Assert.NotNull(deletedAccount);
        Assert.True(deletedAccount.IsDeleted);
    }
}