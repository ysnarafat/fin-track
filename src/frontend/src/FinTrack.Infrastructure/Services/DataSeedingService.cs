using FinTrack.Core.Entities;
using FinTrack.Core.Enums;
using FinTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinTrack.Infrastructure.Services;

/// <summary>
/// Service responsible for seeding initial data into the database
/// </summary>
public class DataSeedingService
{
    private readonly FinTrackDbContext _context;
    private readonly ILogger<DataSeedingService> _logger;

    public DataSeedingService(FinTrackDbContext context, ILogger<DataSeedingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Seeds all default data including categories, account types, and sample data
    /// </summary>
    public async Task SeedAllDataAsync(bool includeSampleData = false)
    {
        try
        {
            _logger.LogInformation("Starting data seeding process");

            await SeedDefaultCategoriesAsync();
            await SeedDefaultAccountTypesAsync();
            
            // Save the default data first
            await _context.SaveChangesAsync();

            if (includeSampleData)
            {
                await SeedSampleDataAsync();
                await _context.SaveChangesAsync();
            }
            _logger.LogInformation("Data seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during data seeding");
            throw;
        }
    }

    /// <summary>
    /// Seeds default expense and income categories
    /// </summary>
    public async Task SeedDefaultCategoriesAsync()
    {
        if (await _context.Categories.AnyAsync())
        {
            _logger.LogInformation("Categories already exist, skipping category seeding");
            return;
        }

        _logger.LogInformation("Seeding default categories");

        var expenseCategories = new[]
        {
            new Category { Name = "Food & Dining", CategoryType = TransactionType.Expense, Icon = "restaurant", Color = "#FF6B6B", IsSystem = true, IsActive = true, SortOrder = 1, Description = "Restaurants, groceries, and food delivery" },
            new Category { Name = "Transportation", CategoryType = TransactionType.Expense, Icon = "car", Color = "#4ECDC4", IsSystem = true, IsActive = true, SortOrder = 2, Description = "Gas, public transport, car maintenance" },
            new Category { Name = "Shopping", CategoryType = TransactionType.Expense, Icon = "shopping_cart", Color = "#45B7D1", IsSystem = true, IsActive = true, SortOrder = 3, Description = "Clothing, electronics, general shopping" },
            new Category { Name = "Entertainment", CategoryType = TransactionType.Expense, Icon = "movie", Color = "#96CEB4", IsSystem = true, IsActive = true, SortOrder = 4, Description = "Movies, games, hobbies, subscriptions" },
            new Category { Name = "Bills & Utilities", CategoryType = TransactionType.Expense, Icon = "receipt", Color = "#FFEAA7", IsSystem = true, IsActive = true, SortOrder = 5, Description = "Electricity, water, internet, phone" },
            new Category { Name = "Healthcare", CategoryType = TransactionType.Expense, Icon = "medical_services", Color = "#DDA0DD", IsSystem = true, IsActive = true, SortOrder = 6, Description = "Doctor visits, medications, insurance" },
            new Category { Name = "Education", CategoryType = TransactionType.Expense, Icon = "school", Color = "#98D8C8", IsSystem = true, IsActive = true, SortOrder = 7, Description = "Tuition, books, courses, training" },
            new Category { Name = "Travel", CategoryType = TransactionType.Expense, Icon = "flight", Color = "#F7DC6F", IsSystem = true, IsActive = true, SortOrder = 8, Description = "Flights, hotels, vacation expenses" },
            new Category { Name = "Personal Care", CategoryType = TransactionType.Expense, Icon = "spa", Color = "#BB8FCE", IsSystem = true, IsActive = true, SortOrder = 9, Description = "Haircuts, cosmetics, gym membership" },
            new Category { Name = "Home & Garden", CategoryType = TransactionType.Expense, Icon = "home", Color = "#85C1E9", IsSystem = true, IsActive = true, SortOrder = 10, Description = "Home improvement, furniture, gardening" },
            new Category { Name = "Insurance", CategoryType = TransactionType.Expense, Icon = "security", Color = "#F8C471", IsSystem = true, IsActive = true, SortOrder = 11, Description = "Life, health, auto, home insurance" },
            new Category { Name = "Taxes", CategoryType = TransactionType.Expense, Icon = "account_balance", Color = "#CD6155", IsSystem = true, IsActive = true, SortOrder = 12, Description = "Income tax, property tax, other taxes" },
            new Category { Name = "Other Expenses", CategoryType = TransactionType.Expense, Icon = "more_horiz", Color = "#BDC3C7", IsSystem = true, IsActive = true, SortOrder = 13, Description = "Miscellaneous expenses" }
        };

        var incomeCategories = new[]
        {
            new Category { Name = "Salary", CategoryType = TransactionType.Income, Icon = "work", Color = "#2ECC71", IsSystem = true, IsActive = true, SortOrder = 1, Description = "Regular employment income" },
            new Category { Name = "Freelance", CategoryType = TransactionType.Income, Icon = "laptop", Color = "#27AE60", IsSystem = true, IsActive = true, SortOrder = 2, Description = "Freelance and contract work" },
            new Category { Name = "Investment Returns", CategoryType = TransactionType.Income, Icon = "trending_up", Color = "#16A085", IsSystem = true, IsActive = true, SortOrder = 3, Description = "Dividends, interest, capital gains" },
            new Category { Name = "Business Income", CategoryType = TransactionType.Income, Icon = "business", Color = "#1ABC9C", IsSystem = true, IsActive = true, SortOrder = 4, Description = "Revenue from business activities" },
            new Category { Name = "Rental Income", CategoryType = TransactionType.Income, Icon = "apartment", Color = "#48C9B0", IsSystem = true, IsActive = true, SortOrder = 5, Description = "Income from property rentals" },
            new Category { Name = "Gifts & Bonuses", CategoryType = TransactionType.Income, Icon = "card_giftcard", Color = "#58D68D", IsSystem = true, IsActive = true, SortOrder = 6, Description = "Gifts, bonuses, and windfalls" },
            new Category { Name = "Refunds", CategoryType = TransactionType.Income, Icon = "receipt_long", Color = "#7DCEA0", IsSystem = true, IsActive = true, SortOrder = 7, Description = "Tax refunds, purchase refunds" },
            new Category { Name = "Other Income", CategoryType = TransactionType.Income, Icon = "attach_money", Color = "#82E0AA", IsSystem = true, IsActive = true, SortOrder = 8, Description = "Miscellaneous income" }
        };

        await _context.Categories.AddRangeAsync(expenseCategories);
        await _context.Categories.AddRangeAsync(incomeCategories);

        _logger.LogInformation("Added {ExpenseCount} expense categories and {IncomeCount} income categories", 
            expenseCategories.Length, incomeCategories.Length);
    }

    /// <summary>
    /// Seeds default account types with sample accounts
    /// </summary>
    public async Task SeedDefaultAccountTypesAsync()
    {
        if (await _context.Accounts.AnyAsync())
        {
            _logger.LogInformation("Accounts already exist, skipping account seeding");
            return;
        }

        _logger.LogInformation("Seeding default accounts");

        var defaultAccounts = new[]
        {
            new Account
            {
                Name = "Primary Checking",
                Balance = 0,
                Type = AccountType.Checking,
                Currency = "USD",
                Description = "Main checking account for daily expenses",
                IsActive = true,
                InitialBalance = 0,
                Institution = "Default Bank",
                AccountNumber = "****1234"
            },
            new Account
            {
                Name = "Savings Account",
                Balance = 0,
                Type = AccountType.Savings,
                Currency = "USD",
                Description = "Primary savings account",
                IsActive = true,
                InitialBalance = 0,
                Institution = "Default Bank",
                AccountNumber = "****5678"
            }
        };

        await _context.Accounts.AddRangeAsync(defaultAccounts);

        _logger.LogInformation("Added {AccountCount} default accounts", defaultAccounts.Length);
    }

    /// <summary>
    /// Seeds sample data for development and testing purposes
    /// </summary>
    public async Task SeedSampleDataAsync()
    {
        _logger.LogInformation("Seeding sample data for development/testing");

        // Get the first account and categories for sample transactions
        var account = await _context.Accounts.FirstAsync();
        var expenseCategory = await _context.Categories
            .FirstAsync(c => c.CategoryType == TransactionType.Expense);
        var incomeCategory = await _context.Categories
            .FirstAsync(c => c.CategoryType == TransactionType.Income);

        // Create sample transactions
        var sampleTransactions = new[]
        {
            new Transaction
            {
                Amount = 1000.00m,
                Description = "Initial deposit",
                Date = DateTime.Today.AddDays(-30),
                CategoryId = incomeCategory.Id,
                AccountId = account.Id,
                Type = TransactionType.Income,
                Notes = "Sample income transaction"
            },
            new Transaction
            {
                Amount = 50.00m,
                Description = "Grocery shopping",
                Date = DateTime.Today.AddDays(-5),
                CategoryId = expenseCategory.Id,
                AccountId = account.Id,
                Type = TransactionType.Expense,
                Notes = "Sample expense transaction"
            },
            new Transaction
            {
                Amount = 25.00m,
                Description = "Coffee shop",
                Date = DateTime.Today.AddDays(-2),
                CategoryId = expenseCategory.Id,
                AccountId = account.Id,
                Type = TransactionType.Expense,
                Notes = "Sample expense transaction"
            }
        };

        await _context.Transactions.AddRangeAsync(sampleTransactions);

        // Update account balance
        account.Balance = sampleTransactions
            .Where(t => t.Type == TransactionType.Income)
            .Sum(t => t.Amount) - 
            sampleTransactions
            .Where(t => t.Type == TransactionType.Expense)
            .Sum(t => t.Amount);

        // Create sample budget
        var sampleBudget = new Budget
        {
            Name = "Monthly Food Budget",
            Amount = 500.00m,
            Period = BudgetPeriod.Monthly,
            StartDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1),
            EndDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month)),
            CategoryId = expenseCategory.Id,
            SpentAmount = 75.00m,
            IsActive = true,
            AlertThreshold = 0.8m,
            Description = "Sample monthly budget for food expenses"
        };

        await _context.Budgets.AddAsync(sampleBudget);

        // Create sample goal
        var sampleGoal = new Goal
        {
            Name = "Emergency Fund",
            Description = "Build an emergency fund for unexpected expenses",
            TargetAmount = 5000.00m,
            CurrentAmount = 925.00m,
            TargetDate = DateTime.Today.AddMonths(12),
            Type = GoalType.Savings,
            IsCompleted = false,
            LinkedAccountId = account.Id,
            Priority = 1
        };

        await _context.Goals.AddAsync(sampleGoal);

        _logger.LogInformation("Added sample transactions, budget, and goal");
    }

    /// <summary>
    /// Checks if the database has been seeded with default data
    /// </summary>
    public async Task<bool> IsDataSeededAsync()
    {
        var hasCategories = await _context.Categories.AnyAsync();
        var hasAccounts = await _context.Accounts.AnyAsync();
        
        return hasCategories && hasAccounts;
    }

    /// <summary>
    /// Resets all data in the database (for testing purposes)
    /// </summary>
    public async Task ResetDataAsync()
    {
        _logger.LogWarning("Resetting all database data");

        _context.Transactions.RemoveRange(_context.Transactions);
        _context.Budgets.RemoveRange(_context.Budgets);
        _context.Goals.RemoveRange(_context.Goals);
        _context.GoalMilestones.RemoveRange(_context.GoalMilestones);
        _context.Categories.RemoveRange(_context.Categories);
        _context.Accounts.RemoveRange(_context.Accounts);

        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Database reset completed");
    }
}