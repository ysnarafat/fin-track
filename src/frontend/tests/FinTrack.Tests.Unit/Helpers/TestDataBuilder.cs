using FinTrack.Core.Entities;
using FinTrack.Core.Enums;
using FinTrack.Shared.Services;

namespace FinTrack.Tests.Unit.Helpers;

/// <summary>
/// Builder class for creating test data objects
/// </summary>
public static class TestDataBuilder
{
    /// <summary>
    /// Creates a valid Account for testing
    /// </summary>
    public static Account CreateAccount(
        string name = "Test Account",
        decimal balance = 1000m,
        AccountType type = AccountType.Checking,
        string currency = "USD",
        bool isActive = true)
    {
        return new Account
        {
            Name = name,
            Balance = balance,
            Type = type,
            Currency = currency,
            IsActive = isActive,
            InitialBalance = balance
        };
    }

    /// <summary>
    /// Creates a valid Transaction for testing
    /// </summary>
    public static Transaction CreateTransaction(
        decimal amount = 100m,
        string description = "Test Transaction",
        TransactionType type = TransactionType.Expense,
        int categoryId = 1,
        int accountId = 1,
        DateTime? date = null)
    {
        return new Transaction
        {
            Amount = amount,
            Description = description,
            Type = type,
            CategoryId = categoryId,
            AccountId = accountId,
            Date = date ?? DateTime.Today
        };
    }

    /// <summary>
    /// Creates a valid Category for testing
    /// </summary>
    public static Category CreateCategory(
        string name = "Test Category",
        TransactionType categoryType = TransactionType.Expense,
        string icon = "test_icon",
        string color = "#FF0000",
        bool isActive = true,
        bool isSystem = false)
    {
        return new Category
        {
            Name = name,
            CategoryType = categoryType,
            Icon = icon,
            Color = color,
            IsActive = isActive,
            IsSystem = isSystem,
            SortOrder = 1
        };
    }

    /// <summary>
    /// Creates a valid Budget for testing
    /// </summary>
    public static Budget CreateBudget(
        decimal amount = 500m,
        int categoryId = 1,
        BudgetPeriod period = BudgetPeriod.Monthly,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.Today.AddDays(-DateTime.Today.Day + 1); // First day of current month
        var end = endDate ?? start.AddMonths(1).AddDays(-1); // Last day of current month

        return new Budget
        {
            Amount = amount,
            CategoryId = categoryId,
            Period = period,
            StartDate = start,
            EndDate = end,
            IsActive = true
        };
    }

    /// <summary>
    /// Creates a valid Goal for testing
    /// </summary>
    public static Goal CreateGoal(
        string name = "Test Goal",
        decimal targetAmount = 1000m,
        GoalType type = GoalType.Savings,
        int? accountId = 1,
        DateTime? targetDate = null)
    {
        return new Goal
        {
            Name = name,
            TargetAmount = targetAmount,
            Type = type,
            TargetDate = targetDate ?? DateTime.Today.AddMonths(6),
            CurrentAmount = 0,
            LinkedAccountId = accountId
        };
    }

    // Note: BudgetModel, BudgetAlert, and BudgetSummary are MAUI-specific models
    // and are not available in the unit test context. These methods have been removed.
    // Use CreateBudget() for creating Budget entities instead.

    /// <summary>
    /// Creates a list of test accounts
    /// </summary>
    public static List<Account> CreateAccountList(int count = 3)
    {
        var accounts = new List<Account>();
        for (int i = 1; i <= count; i++)
        {
            accounts.Add(CreateAccount(
                name: $"Account {i}",
                balance: 1000m * i,
                type: (AccountType)(i % 4) // Cycle through account types
            ));
        }
        return accounts;
    }

    /// <summary>
    /// Creates a list of test transactions
    /// </summary>
    public static List<Transaction> CreateTransactionList(int count = 5, int accountId = 1)
    {
        var transactions = new List<Transaction>();
        var random = new Random(42); // Fixed seed for consistent tests
        
        for (int i = 1; i <= count; i++)
        {
            var type = i % 2 == 0 ? TransactionType.Income : TransactionType.Expense;
            transactions.Add(CreateTransaction(
                amount: random.Next(50, 500),
                description: $"Transaction {i}",
                type: type,
                categoryId: random.Next(1, 10),
                accountId: accountId,
                date: DateTime.Today.AddDays(-random.Next(0, 30))
            ));
        }
        return transactions;
    }

    /// <summary>
    /// Creates a list of test categories
    /// </summary>
    public static List<Category> CreateCategoryList()
    {
        return new List<Category>
        {
            CreateCategory("Food & Dining", TransactionType.Expense, "restaurant", "#FF6B6B"),
            CreateCategory("Transportation", TransactionType.Expense, "car", "#4ECDC4"),
            CreateCategory("Shopping", TransactionType.Expense, "shopping_cart", "#45B7D1"),
            CreateCategory("Salary", TransactionType.Income, "work", "#2ECC71"),
            CreateCategory("Freelance", TransactionType.Income, "laptop", "#27AE60")
        };
    }

    /// <summary>
    /// Creates a transfer transaction pair
    /// </summary>
    public static (Transaction source, Transaction destination) CreateTransferPair(
        int sourceAccountId = 1,
        int destinationAccountId = 2,
        decimal amount = 100m,
        string description = "Transfer")
    {
        var sourceTransaction = new Transaction
        {
            Id = 1,
            Amount = amount,
            Description = description,
            Type = TransactionType.Transfer,
            CategoryId = 1,
            AccountId = sourceAccountId,
            TransferToAccountId = destinationAccountId,
            Date = DateTime.Today
        };

        var destinationTransaction = sourceTransaction.CreateTransferCounterpart(destinationAccountId);
        destinationTransaction.Id = 2;
        destinationTransaction.LinkedTransactionId = 1;
        sourceTransaction.LinkedTransactionId = 2;

        return (sourceTransaction, destinationTransaction);
    }

    /// <summary>
    /// Creates an account with transactions for testing calculations
    /// </summary>
    public static Account CreateAccountWithTransactions(
        string name = "Test Account",
        decimal initialBalance = 1000m)
    {
        var account = CreateAccount(name, initialBalance);
        account.Id = 1;

        // Add some test transactions
        var transactions = new List<Transaction>
        {
            CreateTransaction(500m, "Salary", TransactionType.Income, 1, 1, DateTime.Today.AddDays(-5)),
            CreateTransaction(100m, "Groceries", TransactionType.Expense, 2, 1, DateTime.Today.AddDays(-3)),
            CreateTransaction(50m, "Gas", TransactionType.Expense, 3, 1, DateTime.Today.AddDays(-1)),
            CreateTransaction(200m, "Bonus", TransactionType.Income, 1, 1, DateTime.Today.AddDays(-10))
        };

        foreach (var transaction in transactions)
        {
            account.Transactions.Add(transaction);
        }

        return account;
    }

    // Method aliases for backward compatibility with existing tests
    public static Account CreateTestAccount(
        string name = "Test Account",
        decimal initialBalance = 1000m,
        AccountType type = AccountType.Checking,
        string currency = "USD",
        bool isActive = true)
    {
        return CreateAccount(name, initialBalance, type, currency, isActive);
    }

    public static Category CreateTestCategory(
        string name = "Test Category",
        TransactionType type = TransactionType.Expense,
        string icon = "category",
        string color = "#FF6B6B")
    {
        return CreateCategory(name, type, icon, color);
    }

    public static Transaction CreateTestTransaction(
        decimal amount = 100m,
        string description = "Test Transaction",
        TransactionType type = TransactionType.Expense,
        int categoryId = 1,
        int accountId = 1,
        DateTime? date = null)
    {
        return CreateTransaction(amount, description, type, categoryId, accountId, date);
    }

    public static Budget CreateTestBudget(
        string name = "Test Budget",
        decimal amount = 500m,
        int? categoryId = null,
        BudgetPeriod period = BudgetPeriod.Monthly)
    {
        return CreateBudget(amount, categoryId ?? 1, period);
    }

    public static Goal CreateTestGoal(
        string name = "Test Goal",
        decimal targetAmount = 1000m,
        DateTime? targetDate = null)
    {
        return CreateGoal(name, targetAmount, GoalType.Savings, 1, targetDate);
    }

    // Note: GoalMilestone entity does not exist in the current domain model
    // These methods have been removed
}