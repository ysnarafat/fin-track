using FinTrack.Core.Enums;

namespace FinTrack.Tests.Unit.Infrastructure;

/// <summary>
/// Tests for seeded data functionality in FinTrackDbContext
/// </summary>
public class FinTrackDbContextSeedDataTests : DbContextTestBase
{
    [Fact]
    public async Task Database_WhenCreated_ShouldContainExpectedExpenseCategories()
    {
        // Arrange & Act
        await EnsureDatabaseCreatedAsync();
        var expenseCategories = Context.Categories
            .Where(c => c.CategoryType == TransactionType.Expense && c.IsSystem)
            .ToList();

        // Assert
        Assert.NotEmpty(expenseCategories);
        
        var expectedExpenseCategories = new[]
        {
            "Food & Dining",
            "Transportation", 
            "Shopping",
            "Entertainment",
            "Bills & Utilities",
            "Healthcare",
            "Education",
            "Travel",
            "Personal Care",
            "Other Expenses"
        };

        foreach (var expectedCategory in expectedExpenseCategories)
        {
            Assert.Contains(expenseCategories, c => c.Name == expectedCategory);
        }
    }

    [Fact]
    public async Task Database_WhenCreated_ShouldContainExpectedIncomeCategories()
    {
        // Arrange & Act
        await EnsureDatabaseCreatedAsync();
        var incomeCategories = Context.Categories
            .Where(c => c.CategoryType == TransactionType.Income && c.IsSystem)
            .ToList();

        // Assert
        Assert.NotEmpty(incomeCategories);
        
        var expectedIncomeCategories = new[]
        {
            "Salary",
            "Freelance",
            "Investment Returns",
            "Business Income",
            "Gifts & Bonuses",
            "Other Income"
        };

        foreach (var expectedCategory in expectedIncomeCategories)
        {
            Assert.Contains(incomeCategories, c => c.Name == expectedCategory);
        }
    }

    [Fact]
    public async Task Database_WhenCreated_ShouldHaveCorrectCategoryProperties()
    {
        // Arrange & Act
        await EnsureDatabaseCreatedAsync();
        var foodCategory = Context.Categories
            .First(c => c.Name == "Food & Dining");

        // Assert
        Assert.Equal(TransactionType.Expense, foodCategory.CategoryType);
        Assert.Equal("restaurant", foodCategory.Icon);
        Assert.Equal("#FF6B6B", foodCategory.Color);
        Assert.True(foodCategory.IsSystem);
        Assert.True(foodCategory.IsActive);
        Assert.Equal(1, foodCategory.SortOrder);
    }

    [Fact]
    public async Task Database_WhenCreated_ShouldContainDefaultAccount()
    {
        // Arrange & Act
        await EnsureDatabaseCreatedAsync();
        var defaultAccount = Context.Accounts
            .FirstOrDefault(a => a.Name == "Primary Checking");

        // Assert
        Assert.NotNull(defaultAccount);
        Assert.Equal(AccountType.Checking, defaultAccount.Type);
        Assert.Equal("USD", defaultAccount.Currency);
        Assert.Equal(0, defaultAccount.Balance);
        Assert.Equal(0, defaultAccount.InitialBalance);
        Assert.True(defaultAccount.IsActive);
        Assert.Equal("Default checking account", defaultAccount.Description);
    }

    [Fact]
    public async Task Database_WhenCreated_AllSeededCategoriesShouldHaveValidProperties()
    {
        // Arrange & Act
        await EnsureDatabaseCreatedAsync();
        var seededCategories = Context.Categories
            .Where(c => c.IsSystem)
            .ToList();

        // Assert
        Assert.NotEmpty(seededCategories);
        
        foreach (var category in seededCategories)
        {
            Assert.False(string.IsNullOrWhiteSpace(category.Name));
            Assert.NotNull(category.Icon);
            Assert.NotNull(category.Color);
            Assert.True(category.IsSystem);
            Assert.True(category.IsActive);
            Assert.True(category.SortOrder > 0);
            Assert.True(category.CategoryType == TransactionType.Income || 
                       category.CategoryType == TransactionType.Expense);
        }
    }

    [Fact]
    public async Task Database_WhenCreated_ShouldHaveCorrectNumberOfSeededCategories()
    {
        // Arrange & Act
        await EnsureDatabaseCreatedAsync();
        var expenseCategories = Context.Categories
            .Count(c => c.CategoryType == TransactionType.Expense && c.IsSystem);
        var incomeCategories = Context.Categories
            .Count(c => c.CategoryType == TransactionType.Income && c.IsSystem);

        // Assert
        Assert.Equal(10, expenseCategories); // 10 expense categories
        Assert.Equal(6, incomeCategories);   // 6 income categories
    }

    [Fact]
    public async Task Database_WhenCreated_SeededCategoriesShouldHaveUniqueIds()
    {
        // Arrange & Act
        await EnsureDatabaseCreatedAsync();
        var seededCategories = Context.Categories
            .Where(c => c.IsSystem)
            .ToList();

        // Assert
        var categoryIds = seededCategories.Select(c => c.Id).ToList();
        var uniqueIds = categoryIds.Distinct().ToList();
        
        Assert.Equal(categoryIds.Count, uniqueIds.Count);
    }

    [Fact]
    public async Task Database_WhenCreated_SeededCategoriesShouldHaveValidColors()
    {
        // Arrange & Act
        await EnsureDatabaseCreatedAsync();
        var seededCategories = Context.Categories
            .Where(c => c.IsSystem)
            .ToList();

        // Assert
        foreach (var category in seededCategories)
        {
            Assert.NotNull(category.Color);
            Assert.StartsWith("#", category.Color);
            Assert.Equal(7, category.Color.Length); // #RRGGBB format
            
            // Verify it's a valid hex color
            var hexPart = category.Color[1..];
            Assert.True(hexPart.All(c => char.IsDigit(c) || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f')));
        }
    }

    [Fact]
    public async Task Database_WhenCreated_ExpenseCategoriesShouldHaveCorrectSortOrder()
    {
        // Arrange & Act
        await EnsureDatabaseCreatedAsync();
        var expenseCategories = Context.Categories
            .Where(c => c.CategoryType == TransactionType.Expense && c.IsSystem)
            .OrderBy(c => c.SortOrder)
            .ToList();

        // Assert
        for (int i = 0; i < expenseCategories.Count; i++)
        {
            Assert.Equal(i + 1, expenseCategories[i].SortOrder);
        }
    }

    [Fact]
    public async Task Database_WhenCreated_IncomeCategoriesShouldHaveCorrectSortOrder()
    {
        // Arrange & Act
        await EnsureDatabaseCreatedAsync();
        var incomeCategories = Context.Categories
            .Where(c => c.CategoryType == TransactionType.Income && c.IsSystem)
            .OrderBy(c => c.SortOrder)
            .ToList();

        // Assert
        for (int i = 0; i < incomeCategories.Count; i++)
        {
            Assert.Equal(i + 1, incomeCategories[i].SortOrder);
        }
    }

    [Fact]
    public async Task Database_WhenCreated_SeededDataShouldNotInterfereWithUserData()
    {
        // Arrange
        await EnsureDatabaseCreatedAsync();
        
        var userCategory = new Core.Entities.Category
        {
            Name = "User Custom Category",
            CategoryType = TransactionType.Expense,
            IsSystem = false,
            IsActive = true
        };

        // Act
        Context.Categories.Add(userCategory);
        await Context.SaveChangesAsync();

        // Assert
        var allCategories = Context.Categories.ToList();
        var systemCategories = allCategories.Where(c => c.IsSystem).ToList();
        var userCategories = allCategories.Where(c => !c.IsSystem).ToList();

        Assert.Equal(16, systemCategories.Count); // 10 expense + 6 income
        Assert.Single(userCategories);
        Assert.Equal("User Custom Category", userCategories[0].Name);
    }

    [Fact]
    public async Task Database_WhenRecreated_ShouldMaintainConsistentSeededData()
    {
        // Arrange & Act - Create database first time
        await EnsureDatabaseCreatedAsync();
        var firstCategories = Context.Categories.Where(c => c.IsSystem).ToList();

        // Dispose and recreate context
        Context.Dispose();
        Context = CreateNewContext();
        await EnsureDatabaseCreatedAsync();
        var secondCategories = Context.Categories.Where(c => c.IsSystem).ToList();

        // Assert
        Assert.Equal(firstCategories.Count, secondCategories.Count);
        
        foreach (var firstCategory in firstCategories)
        {
            var matchingSecond = secondCategories.FirstOrDefault(c => c.Name == firstCategory.Name);
            Assert.NotNull(matchingSecond);
            Assert.Equal(firstCategory.CategoryType, matchingSecond.CategoryType);
            Assert.Equal(firstCategory.Icon, matchingSecond.Icon);
            Assert.Equal(firstCategory.Color, matchingSecond.Color);
            Assert.Equal(firstCategory.SortOrder, matchingSecond.SortOrder);
        }
    }

    [Fact]
    public async Task Database_WhenCreated_DefaultAccountShouldHaveValidAuditFields()
    {
        // Arrange & Act
        await EnsureDatabaseCreatedAsync();
        var defaultAccount = Context.Accounts
            .First(a => a.Name == "Primary Checking");

        // Assert
        Assert.True(defaultAccount.Id > 0);
        Assert.True(defaultAccount.CreatedAt > DateTime.MinValue);
        Assert.True(defaultAccount.UpdatedAt > DateTime.MinValue);
        Assert.False(string.IsNullOrEmpty(defaultAccount.SyncId));
        Assert.False(defaultAccount.IsDeleted);
    }
}