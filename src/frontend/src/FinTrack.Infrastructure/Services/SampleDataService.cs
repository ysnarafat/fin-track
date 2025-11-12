using FinTrack.Core.Entities;
using FinTrack.Core.Enums;
using FinTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinTrack.Infrastructure.Services;

/// <summary>
/// Service for creating sample data for development and testing purposes
/// </summary>
public class SampleDataService
{
    private readonly FinTrackDbContext _context;
    private readonly ILogger<SampleDataService> _logger;

    public SampleDataService(FinTrackDbContext context, ILogger<SampleDataService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Creates comprehensive sample data for development and testing
    /// </summary>
    public async Task CreateSampleDataAsync()
    {
        try
        {
            _logger.LogInformation("Creating sample data for development/testing");

            // Check if sample data already exists
            if (await _context.Transactions.AnyAsync())
            {
                _logger.LogInformation("Sample data already exists, skipping creation");
                return;
            }

            await CreateSampleAccountsAsync();
            await CreateSampleTransactionsAsync();
            await CreateSampleBudgetsAsync();
            await CreateSampleGoalsAsync();

            await _context.SaveChangesAsync();
            _logger.LogInformation("Sample data creation completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating sample data");
            throw;
        }
    }

    /// <summary>
    /// Creates additional sample accounts beyond the default one
    /// </summary>
    private async Task CreateSampleAccountsAsync()
    {
        var existingAccountsCount = await _context.Accounts.CountAsync();
        if (existingAccountsCount > 1)
        {
            _logger.LogInformation("Additional accounts already exist, skipping account creation");
            return;
        }

        var sampleAccounts = new[]
        {
            new Account
            {
                Name = "Savings Account",
                Balance = 5000.00m,
                Type = AccountType.Savings,
                Currency = "USD",
                Description = "High-yield savings account",
                IsActive = true,
                InitialBalance = 5000.00m,
                Institution = "Sample Bank",
                AccountNumber = "****9876"
            },
            new Account
            {
                Name = "Credit Card",
                Balance = -1250.00m,
                Type = AccountType.CreditCard,
                Currency = "USD",
                Description = "Main credit card",
                IsActive = true,
                InitialBalance = 0.00m,
                Institution = "Sample Credit Union",
                AccountNumber = "****5432"
            },
            new Account
            {
                Name = "Investment Account",
                Balance = 15000.00m,
                Type = AccountType.Investment,
                Currency = "USD",
                Description = "Stock and bond investments",
                IsActive = true,
                InitialBalance = 10000.00m,
                Institution = "Sample Investments",
                AccountNumber = "****1111"
            }
        };

        await _context.Accounts.AddRangeAsync(sampleAccounts);
        _logger.LogInformation("Created {Count} additional sample accounts", sampleAccounts.Length);
    }

    /// <summary>
    /// Creates sample transactions across different accounts and categories
    /// </summary>
    private async Task CreateSampleTransactionsAsync()
    {
        var accounts = await _context.Accounts.ToListAsync();
        var categories = await _context.Categories.ToListAsync();

        var checkingAccount = accounts.First(a => a.Type == AccountType.Checking);
        var savingsAccount = accounts.FirstOrDefault(a => a.Type == AccountType.Savings);
        var creditCardAccount = accounts.FirstOrDefault(a => a.Type == AccountType.CreditCard);

        var foodCategory = categories.First(c => c.Name == "Food & Dining");
        var transportCategory = categories.First(c => c.Name == "Transportation");
        var salaryCategory = categories.First(c => c.Name == "Salary");
        var shoppingCategory = categories.First(c => c.Name == "Shopping");
        var entertainmentCategory = categories.First(c => c.Name == "Entertainment");

        var sampleTransactions = new List<Transaction>();

        // Create transactions for the last 3 months
        var startDate = DateTime.Today.AddMonths(-3);
        var random = new Random(42); // Fixed seed for consistent sample data

        // Monthly salary
        for (int month = 0; month < 3; month++)
        {
            var salaryDate = startDate.AddMonths(month).AddDays(14); // 15th of each month
            sampleTransactions.Add(new Transaction
            {
                Amount = 4500.00m,
                Description = "Monthly Salary",
                Date = salaryDate,
                CategoryId = salaryCategory.Id,
                AccountId = checkingAccount.Id,
                Type = TransactionType.Income,
                Notes = "Regular monthly salary deposit"
            });
        }

        // Regular expenses
        var expenseTemplates = new[]
        {
            new { Category = foodCategory, Descriptions = new[] { "Grocery Store", "Restaurant", "Coffee Shop", "Fast Food", "Lunch" }, AmountRange = (10m, 150m) },
            new { Category = transportCategory, Descriptions = new[] { "Gas Station", "Public Transit", "Uber", "Parking", "Car Maintenance" }, AmountRange = (15m, 200m) },
            new { Category = shoppingCategory, Descriptions = new[] { "Amazon", "Target", "Clothing Store", "Electronics", "Home Goods" }, AmountRange = (25m, 300m) },
            new { Category = entertainmentCategory, Descriptions = new[] { "Movie Theater", "Streaming Service", "Concert", "Games", "Books" }, AmountRange = (10m, 100m) }
        };

        // Generate random transactions
        for (var date = startDate; date <= DateTime.Today; date = date.AddDays(1))
        {
            // Skip some days randomly
            if (random.NextDouble() > 0.3) continue;

            var template = expenseTemplates[random.Next(expenseTemplates.Length)];
            var description = template.Descriptions[random.Next(template.Descriptions.Length)];
            var amount = (decimal)(random.NextDouble() * (double)(template.AmountRange.Item2 - template.AmountRange.Item1)) + template.AmountRange.Item1;
            amount = Math.Round(amount, 2);

            // Randomly choose account (mostly checking, sometimes credit card)
            var account = random.NextDouble() > 0.7 && creditCardAccount != null ? creditCardAccount : checkingAccount;

            sampleTransactions.Add(new Transaction
            {
                Amount = amount,
                Description = description,
                Date = date,
                CategoryId = template.Category.Id,
                AccountId = account.Id,
                Type = TransactionType.Expense,
                Notes = $"Sample transaction from {description}"
            });
        }

        // Add some transfers to savings
        if (savingsAccount != null)
        {
            for (int month = 0; month < 3; month++)
            {
                var transferDate = startDate.AddMonths(month).AddDays(20);
                sampleTransactions.Add(new Transaction
                {
                    Amount = 500.00m,
                    Description = "Transfer to Savings",
                    Date = transferDate,
                    CategoryId = categories.First(c => c.Name == "Other Income").Id,
                    AccountId = savingsAccount.Id,
                    Type = TransactionType.Income,
                    Notes = "Monthly savings transfer"
                });

                sampleTransactions.Add(new Transaction
                {
                    Amount = 500.00m,
                    Description = "Transfer to Savings",
                    Date = transferDate,
                    CategoryId = categories.First(c => c.Name == "Other Expenses").Id,
                    AccountId = checkingAccount.Id,
                    Type = TransactionType.Expense,
                    Notes = "Monthly savings transfer"
                });
            }
        }

        await _context.Transactions.AddRangeAsync(sampleTransactions);

        // Update account balances based on transactions
        await UpdateAccountBalancesAsync();

        _logger.LogInformation("Created {Count} sample transactions", sampleTransactions.Count);
    }

    /// <summary>
    /// Creates sample budgets for different categories
    /// </summary>
    private async Task CreateSampleBudgetsAsync()
    {
        var categories = await _context.Categories.Where(c => c.CategoryType == TransactionType.Expense).ToListAsync();

        var currentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var nextMonth = currentMonth.AddMonths(1);

        var sampleBudgets = new[]
        {
            new Budget
            {
                Name = "Monthly Food Budget",
                Amount = 800.00m,
                Period = BudgetPeriod.Monthly,
                StartDate = currentMonth,
                EndDate = nextMonth.AddDays(-1),
                CategoryId = categories.First(c => c.Name == "Food & Dining").Id,
                SpentAmount = 0m, // Will be calculated
                IsActive = true,
                AlertThreshold = 0.8m,
                Description = "Monthly budget for food and dining expenses"
            },
            new Budget
            {
                Name = "Transportation Budget",
                Amount = 400.00m,
                Period = BudgetPeriod.Monthly,
                StartDate = currentMonth,
                EndDate = nextMonth.AddDays(-1),
                CategoryId = categories.First(c => c.Name == "Transportation").Id,
                SpentAmount = 0m,
                IsActive = true,
                AlertThreshold = 0.75m,
                Description = "Monthly budget for transportation costs"
            },
            new Budget
            {
                Name = "Entertainment Budget",
                Amount = 200.00m,
                Period = BudgetPeriod.Monthly,
                StartDate = currentMonth,
                EndDate = nextMonth.AddDays(-1),
                CategoryId = categories.First(c => c.Name == "Entertainment").Id,
                SpentAmount = 0m,
                IsActive = true,
                AlertThreshold = 0.9m,
                Description = "Monthly budget for entertainment and leisure"
            },
            new Budget
            {
                Name = "Quarterly Shopping Budget",
                Amount = 1500.00m,
                Period = BudgetPeriod.Quarterly,
                StartDate = new DateTime(DateTime.Today.Year, ((DateTime.Today.Month - 1) / 3) * 3 + 1, 1),
                EndDate = new DateTime(DateTime.Today.Year, ((DateTime.Today.Month - 1) / 3) * 3 + 3, DateTime.DaysInMonth(DateTime.Today.Year, ((DateTime.Today.Month - 1) / 3) * 3 + 3)),
                CategoryId = categories.First(c => c.Name == "Shopping").Id,
                SpentAmount = 0m,
                IsActive = true,
                AlertThreshold = 0.85m,
                Description = "Quarterly budget for shopping and purchases"
            }
        };

        await _context.Budgets.AddRangeAsync(sampleBudgets);

        // Calculate spent amounts for budgets
        foreach (var budget in sampleBudgets)
        {
            var spent = await _context.Transactions
                .Where(t => t.CategoryId == budget.CategoryId &&
                           t.Type == TransactionType.Expense &&
                           t.Date >= budget.StartDate &&
                           t.Date <= budget.EndDate &&
                           !t.IsDeleted)
                .SumAsync(t => t.Amount);

            budget.SpentAmount = spent;
        }

        _logger.LogInformation("Created {Count} sample budgets", sampleBudgets.Length);
    }

    /// <summary>
    /// Creates sample financial goals
    /// </summary>
    private async Task CreateSampleGoalsAsync()
    {
        var savingsAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.Type == AccountType.Savings);

        var sampleGoals = new[]
        {
            new Goal
            {
                Name = "Emergency Fund",
                Description = "Build an emergency fund covering 6 months of expenses",
                TargetAmount = 15000.00m,
                CurrentAmount = 5000.00m,
                TargetDate = DateTime.Today.AddMonths(18),
                Type = GoalType.Savings,
                IsCompleted = false,
                LinkedAccountId = savingsAccount?.Id,
                Priority = 1
            },
            new Goal
            {
                Name = "Vacation Fund",
                Description = "Save for a European vacation next summer",
                TargetAmount = 5000.00m,
                CurrentAmount = 1200.00m,
                TargetDate = DateTime.Today.AddMonths(10),
                Type = GoalType.Savings,
                IsCompleted = false,
                LinkedAccountId = savingsAccount?.Id,
                Priority = 2
            },
            new Goal
            {
                Name = "Pay Off Credit Card",
                Description = "Pay off remaining credit card debt",
                TargetAmount = 2500.00m,
                CurrentAmount = 1250.00m,
                TargetDate = DateTime.Today.AddMonths(8),
                Type = GoalType.DebtPayoff,
                IsCompleted = false,
                Priority = 1
            },
            new Goal
            {
                Name = "Investment Portfolio",
                Description = "Build a diversified investment portfolio",
                TargetAmount = 25000.00m,
                CurrentAmount = 15000.00m,
                TargetDate = DateTime.Today.AddMonths(24),
                Type = GoalType.Investment,
                IsCompleted = false,
                Priority = 3
            }
        };

        await _context.Goals.AddRangeAsync(sampleGoals);
        _logger.LogInformation("Created {Count} sample goals", sampleGoals.Length);
    }

    /// <summary>
    /// Updates account balances based on transactions
    /// </summary>
    private async Task UpdateAccountBalancesAsync()
    {
        var accounts = await _context.Accounts.ToListAsync();

        foreach (var account in accounts)
        {
            var income = await _context.Transactions
                .Where(t => t.AccountId == account.Id && t.Type == TransactionType.Income && !t.IsDeleted)
                .SumAsync(t => t.Amount);

            var expenses = await _context.Transactions
                .Where(t => t.AccountId == account.Id && t.Type == TransactionType.Expense && !t.IsDeleted)
                .SumAsync(t => t.Amount);

            // For credit cards, the balance should be negative (debt)
            if (account.Type == AccountType.CreditCard)
            {
                account.Balance = account.InitialBalance - income + expenses;
            }
            else
            {
                account.Balance = account.InitialBalance + income - expenses;
            }
        }

        _logger.LogInformation("Updated balances for {Count} accounts", accounts.Count);
    }

    /// <summary>
    /// Removes all sample data from the database
    /// </summary>
    public async Task ClearSampleDataAsync()
    {
        _logger.LogWarning("Clearing all sample data from database");

        // Remove in order to respect foreign key constraints
        _context.Transactions.RemoveRange(_context.Transactions);
        _context.Budgets.RemoveRange(_context.Budgets);
        _context.Goals.RemoveRange(_context.Goals);
        
        // Keep system categories and default account
        var nonSystemAccounts = await _context.Accounts.Where(a => a.Id != 1).ToListAsync();
        _context.Accounts.RemoveRange(nonSystemAccounts);

        await _context.SaveChangesAsync();
        
        // Reset the default account balance
        var defaultAccount = await _context.Accounts.FindAsync(1);
        if (defaultAccount != null)
        {
            defaultAccount.Balance = 0;
            await _context.SaveChangesAsync();
        }

        _logger.LogInformation("Sample data cleared successfully");
    }
}