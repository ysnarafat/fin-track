using FinTrack.Core.Entities;
using FinTrack.Core.Enums;

namespace FinTrack.Tests.Unit.Helpers;

/// <summary>
/// Builder class for creating test data objects with fluent API
/// </summary>
public static class TestDataBuilder
{
    /// <summary>
    /// Creates a builder for Transaction entities
    /// </summary>
    public static TransactionBuilder Transaction() => new TransactionBuilder();
    
    /// <summary>
    /// Creates a builder for Account entities
    /// </summary>
    public static AccountBuilder Account() => new AccountBuilder();
    
    /// <summary>
    /// Creates a builder for Category entities
    /// </summary>
    public static CategoryBuilder Category() => new CategoryBuilder();
    
    /// <summary>
    /// Creates a builder for Goal entities
    /// </summary>
    public static GoalBuilder Goal() => new GoalBuilder();
}

/// <summary>
/// Builder for Transaction test data
/// </summary>
public class TransactionBuilder
{
    private readonly Transaction _transaction = new Transaction();

    public TransactionBuilder WithId(int id)
    {
        _transaction.Id = id;
        return this;
    }

    public TransactionBuilder WithAmount(decimal amount)
    {
        _transaction.Amount = amount;
        return this;
    }

    public TransactionBuilder WithDescription(string description)
    {
        _transaction.Description = description;
        return this;
    }

    public TransactionBuilder WithDate(DateTime date)
    {
        _transaction.Date = date;
        return this;
    }

    public TransactionBuilder WithType(TransactionType type)
    {
        _transaction.Type = type;
        return this;
    }

    public TransactionBuilder WithCategoryId(int categoryId)
    {
        _transaction.CategoryId = categoryId;
        return this;
    }

    public TransactionBuilder WithAccountId(int accountId)
    {
        _transaction.AccountId = accountId;
        return this;
    }

    public TransactionBuilder WithReferenceNumber(string referenceNumber)
    {
        _transaction.ReferenceNumber = referenceNumber;
        return this;
    }

    public TransactionBuilder WithNotes(string notes)
    {
        _transaction.Notes = notes;
        return this;
    }

    public TransactionBuilder AsTransfer(int toAccountId)
    {
        _transaction.Type = TransactionType.Transfer;
        _transaction.TransferToAccountId = toAccountId;
        return this;
    }

    public TransactionBuilder AsReconciled(DateTime? reconciledAt = null)
    {
        _transaction.IsReconciled = true;
        _transaction.ReconciledAt = reconciledAt ?? DateTime.UtcNow;
        return this;
    }

    public TransactionBuilder AsDeleted()
    {
        _transaction.MarkAsDeleted();
        return this;
    }

    public TransactionBuilder WithSyncStatus(SyncStatus status)
    {
        _transaction.SyncStatus = status;
        return this;
    }

    public Transaction Build() => _transaction;
}

/// <summary>
/// Builder for Account test data
/// </summary>
public class AccountBuilder
{
    private readonly Account _account = new Account();

    public AccountBuilder WithId(int id)
    {
        _account.Id = id;
        return this;
    }

    public AccountBuilder WithName(string name)
    {
        _account.Name = name;
        return this;
    }

    public AccountBuilder WithBalance(decimal balance)
    {
        _account.Balance = balance;
        return this;
    }

    public AccountBuilder WithType(AccountType type)
    {
        _account.Type = type;
        return this;
    }

    public AccountBuilder WithCurrency(string currency)
    {
        _account.Currency = currency;
        return this;
    }

    public AccountBuilder WithDescription(string description)
    {
        _account.Description = description;
        return this;
    }

    public AccountBuilder WithAccountNumber(string accountNumber)
    {
        _account.AccountNumber = accountNumber;
        return this;
    }

    public AccountBuilder WithInstitution(string institution)
    {
        _account.Institution = institution;
        return this;
    }

    public AccountBuilder WithInitialBalance(decimal initialBalance)
    {
        _account.InitialBalance = initialBalance;
        return this;
    }

    public AccountBuilder AsInactive()
    {
        _account.IsActive = false;
        return this;
    }

    public AccountBuilder WithCreditLimit(decimal creditLimit)
    {
        _account.CreditLimit = creditLimit;
        return this;
    }

    public AccountBuilder WithInterestRate(decimal interestRate)
    {
        _account.InterestRate = interestRate;
        return this;
    }

    public AccountBuilder WithTransactions(params Transaction[] transactions)
    {
        foreach (var transaction in transactions)
        {
            _account.Transactions.Add(transaction);
        }
        return this;
    }

    public Account Build() => _account;
}

/// <summary>
/// Builder for Category test data
/// </summary>
public class CategoryBuilder
{
    private readonly Category _category = new Category();

    public CategoryBuilder WithId(int id)
    {
        _category.Id = id;
        return this;
    }

    public CategoryBuilder WithName(string name)
    {
        _category.Name = name;
        return this;
    }

    public CategoryBuilder WithDescription(string description)
    {
        _category.Description = description;
        return this;
    }

    public CategoryBuilder WithColor(string color)
    {
        _category.Color = color;
        return this;
    }

    public CategoryBuilder WithIcon(string icon)
    {
        _category.Icon = icon;
        return this;
    }

    public CategoryBuilder WithParent(int parentCategoryId)
    {
        _category.ParentCategoryId = parentCategoryId;
        return this;
    }

    public CategoryBuilder AsInactive()
    {
        _category.IsActive = false;
        return this;
    }

    public CategoryBuilder AsSystem()
    {
        _category.IsSystem = true;
        return this;
    }

    public CategoryBuilder WithSortOrder(int sortOrder)
    {
        _category.SortOrder = sortOrder;
        return this;
    }

    public CategoryBuilder WithBudgetLimit(decimal budgetLimit)
    {
        _category.BudgetLimit = budgetLimit;
        return this;
    }

    public CategoryBuilder AsDeleted()
    {
        _category.MarkAsDeleted();
        return this;
    }

    public CategoryBuilder WithTransactions(params Transaction[] transactions)
    {
        foreach (var transaction in transactions)
        {
            _category.Transactions.Add(transaction);
        }
        return this;
    }

    public CategoryBuilder WithSubCategories(params Category[] subCategories)
    {
        foreach (var subCategory in subCategories)
        {
            _category.SubCategories.Add(subCategory);
        }
        return this;
    }

    public Category Build() => _category;
}

/// <summary>
/// Builder for Goal test data
/// </summary>
public class GoalBuilder
{
    private readonly Goal _goal = new Goal();

    public GoalBuilder WithId(int id)
    {
        _goal.Id = id;
        return this;
    }

    public GoalBuilder WithName(string name)
    {
        _goal.Name = name;
        return this;
    }

    public GoalBuilder WithDescription(string description)
    {
        _goal.Description = description;
        return this;
    }

    public GoalBuilder WithTargetAmount(decimal targetAmount)
    {
        _goal.TargetAmount = targetAmount;
        return this;
    }

    public GoalBuilder WithCurrentAmount(decimal currentAmount)
    {
        _goal.CurrentAmount = currentAmount;
        return this;
    }

    public GoalBuilder WithTargetDate(DateTime targetDate)
    {
        _goal.TargetDate = targetDate;
        return this;
    }

    public GoalBuilder WithPriority(int priority)
    {
        _goal.Priority = priority;
        return this;
    }

    public GoalBuilder WithCategory(string category)
    {
        _goal.Category = category;
        return this;
    }

    public GoalBuilder WithColor(string color)
    {
        _goal.Color = color;
        return this;
    }

    public GoalBuilder AsCompleted(DateTime? completedDate = null)
    {
        _goal.IsCompleted = true;
        _goal.CompletedDate = completedDate ?? DateTime.UtcNow;
        return this;
    }

    public GoalBuilder WithMilestone(string name, decimal targetAmount, string description = "")
    {
        _goal.AddMilestone(name, targetAmount, description);
        return this;
    }

    public GoalBuilder WithMilestones(params GoalMilestone[] milestones)
    {
        foreach (var milestone in milestones)
        {
            _goal.Milestones.Add(milestone);
        }
        return this;
    }

    public Goal Build() => _goal;
}

/// <summary>
/// Common test data scenarios
/// </summary>
public static class TestScenarios
{
    /// <summary>
    /// Creates a typical checking account with some transactions
    /// </summary>
    public static Account TypicalCheckingAccount()
    {
        return TestDataBuilder.Account()
            .WithName("Main Checking")
            .WithType(AccountType.Checking)
            .WithBalance(2500.00m)
            .WithInitialBalance(1000.00m)
            .WithCurrency("USD")
            .WithInstitution("Test Bank")
            .WithAccountNumber("****1234")
            .Build();
    }

    /// <summary>
    /// Creates a credit card account with debt
    /// </summary>
    public static Account CreditCardWithDebt()
    {
        return TestDataBuilder.Account()
            .WithName("Credit Card")
            .WithType(AccountType.CreditCard)
            .WithBalance(-1500.00m) // Negative balance indicates debt
            .WithCreditLimit(5000.00m)
            .WithCurrency("USD")
            .Build();
    }

    /// <summary>
    /// Creates a typical expense transaction
    /// </summary>
    public static Transaction TypicalExpenseTransaction()
    {
        return TestDataBuilder.Transaction()
            .WithAmount(85.50m)
            .WithDescription("Grocery Shopping")
            .WithType(TransactionType.Expense)
            .WithDate(DateTime.Today)
            .WithCategoryId(1)
            .WithAccountId(1)
            .Build();
    }

    /// <summary>
    /// Creates a typical income transaction
    /// </summary>
    public static Transaction TypicalIncomeTransaction()
    {
        return TestDataBuilder.Transaction()
            .WithAmount(3500.00m)
            .WithDescription("Monthly Salary")
            .WithType(TransactionType.Income)
            .WithDate(DateTime.Today.AddDays(-1))
            .WithCategoryId(2)
            .WithAccountId(1)
            .Build();
    }

    /// <summary>
    /// Creates a transfer transaction between accounts
    /// </summary>
    public static Transaction TypicalTransferTransaction()
    {
        return TestDataBuilder.Transaction()
            .WithAmount(500.00m)
            .WithDescription("Transfer to Savings")
            .WithType(TransactionType.Transfer)
            .WithDate(DateTime.Today)
            .WithCategoryId(3)
            .WithAccountId(1)
            .AsTransfer(2)
            .Build();
    }

    /// <summary>
    /// Creates a food category with budget limit
    /// </summary>
    public static Category FoodCategory()
    {
        return TestDataBuilder.Category()
            .WithName("Food & Dining")
            .WithDescription("Groceries, restaurants, and food delivery")
            .WithColor("#FF5722")
            .WithIcon("🍽️")
            .WithBudgetLimit(800.00m)
            .Build();
    }

    /// <summary>
    /// Creates an emergency fund goal
    /// </summary>
    public static Goal EmergencyFundGoal()
    {
        return TestDataBuilder.Goal()
            .WithName("Emergency Fund")
            .WithDescription("6 months of expenses saved")
            .WithTargetAmount(15000.00m)
            .WithCurrentAmount(7500.00m)
            .WithTargetDate(DateTime.Today.AddMonths(12))
            .WithPriority(1)
            .WithCategory("Emergency")
            .WithColor("#FF9800")
            .WithMilestone("25% Complete", 3750.00m, "Quarter of the way there!")
            .WithMilestone("50% Complete", 7500.00m, "Halfway to goal!")
            .WithMilestone("75% Complete", 11250.00m, "Three quarters done!")
            .Build();
    }
}