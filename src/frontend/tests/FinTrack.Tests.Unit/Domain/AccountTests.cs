using FinTrack.Core.Entities;
using FinTrack.Core.Enums;

namespace FinTrack.Tests.Unit.Domain;

/// <summary>
/// Unit tests for Account entity
/// </summary>
public class AccountTests
{
    [Fact]
    public void Constructor_ShouldInitializeDefaultValues()
    {
        // Arrange & Act
        var account = new Account();

        // Assert
        Assert.Equal(AccountType.Checking, account.Type);
        Assert.Equal("USD", account.Currency);
        Assert.True(account.IsActive);
        Assert.Equal(0, account.Balance);
        Assert.Equal(0, account.InitialBalance);
        Assert.Empty(account.Name);
        Assert.Null(account.Description);
        Assert.Null(account.AccountNumber);
        Assert.Null(account.Institution);
        Assert.Null(account.CreditLimit);
        Assert.Null(account.InterestRate);
        Assert.NotNull(account.Transactions);
        Assert.NotNull(account.IncomingTransfers);
    }

    [Theory]
    [InlineData(AccountType.Checking, 1000, null, 1000)]
    [InlineData(AccountType.Savings, 5000, null, 5000)]
    [InlineData(AccountType.CreditCard, -500, 2000.0, 1500)] // Credit card with debt
    [InlineData(AccountType.CreditCard, 0, 1000.0, 1000)] // Credit card with no debt
    [InlineData(AccountType.CreditCard, -1000, null, -1000)] // Credit card without limit
    public void AvailableBalance_ShouldCalculateCorrectly(AccountType type, decimal balance, double? creditLimit, decimal expected)
    {
        // Arrange
        var account = new Account
        {
            Type = type,
            Balance = balance,
            CreditLimit = creditLimit.HasValue ? (decimal)creditLimit.Value : null
        };

        // Act
        var result = account.AvailableBalance;

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(1000.50, "$1,000.50")]
    [InlineData(-500.25, "($500.25)")]
    [InlineData(0, "$0.00")]
    public void FormattedBalance_ShouldFormatCorrectly(decimal balance, string expected)
    {
        // Arrange
        var account = new Account { Balance = balance };

        // Act
        var result = account.FormattedBalance;

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(AccountType.Checking, -100, null, true)] // Checking account overdrawn
    [InlineData(AccountType.Checking, 100, null, false)] // Checking account positive
    [InlineData(AccountType.Savings, -50, null, true)] // Savings account overdrawn
    [InlineData(AccountType.CreditCard, -1000, 500.0, true)] // Credit card over limit
    [InlineData(AccountType.CreditCard, -400, 500.0, false)] // Credit card within limit
    [InlineData(AccountType.CreditCard, -500, null, false)] // Credit card without limit
    public void IsOverdrawn_ShouldCalculateCorrectly(AccountType type, decimal balance, double? creditLimit, bool expected)
    {
        // Arrange
        var account = new Account
        {
            Type = type,
            Balance = balance,
            CreditLimit = creditLimit.HasValue ? (decimal)creditLimit.Value : null
        };

        // Act
        var result = account.IsOverdrawn;

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void UpdateBalance_ShouldAddAmountAndMarkAsModified()
    {
        // Arrange
        var account = new Account { Balance = 1000 };
        account.MarkAsSynced(); // Set to synced to test modification tracking
        var originalVersion = account.Version;

        // Act
        account.UpdateBalance(250.50m);

        // Assert
        Assert.Equal(1250.50m, account.Balance);
        Assert.Equal(SyncStatus.PendingUpdate, account.SyncStatus);
        Assert.Equal(originalVersion + 1, account.Version);
    }

    [Fact]
    public void UpdateBalance_WithNegativeAmount_ShouldSubtractFromBalance()
    {
        // Arrange
        var account = new Account { Balance = 1000 };

        // Act
        account.UpdateBalance(-150.25m);

        // Assert
        Assert.Equal(849.75m, account.Balance);
    }

    [Fact]
    public void SetBalance_ShouldSetExactAmountAndMarkAsModified()
    {
        // Arrange
        var account = new Account { Balance = 1000 };
        account.MarkAsSynced(); // Set to synced to test modification tracking
        var originalVersion = account.Version;

        // Act
        account.SetBalance(2500.75m);

        // Assert
        Assert.Equal(2500.75m, account.Balance);
        Assert.Equal(SyncStatus.PendingUpdate, account.SyncStatus);
        Assert.Equal(originalVersion + 1, account.Version);
    }

    [Fact]
    public void IsValid_WithValidData_ShouldReturnTrue()
    {
        // Arrange
        var account = new Account
        {
            Name = "Test Account",
            Currency = "USD",
            Type = AccountType.Checking
        };

        // Act
        var result = account.IsValid();

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void IsValid_WithInvalidName_ShouldReturnFalse(string name)
    {
        // Arrange
        var account = new Account
        {
            Name = name,
            Currency = "USD"
        };

        // Act
        var result = account.IsValid();

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    [InlineData("US")] // Too short
    [InlineData("USDD")] // Too long
    public void IsValid_WithInvalidCurrency_ShouldReturnFalse(string currency)
    {
        // Arrange
        var account = new Account
        {
            Name = "Test Account",
            Currency = currency
        };

        // Act
        var result = account.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_CreditCardWithNegativeCreditLimit_ShouldReturnFalse()
    {
        // Arrange
        var account = new Account
        {
            Name = "Test Credit Card",
            Currency = "USD",
            Type = AccountType.CreditCard,
            CreditLimit = -1000
        };

        // Act
        var result = account.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_WithNegativeInterestRate_ShouldReturnFalse()
    {
        // Arrange
        var account = new Account
        {
            Name = "Test Account",
            Currency = "USD",
            InterestRate = -0.05m
        };

        // Act
        var result = account.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CalculateIncome_ShouldSumIncomeTransactionsInDateRange()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 1, 31);
        
        var account = new Account();
        account.Transactions.Add(new Transaction 
        { 
            Type = TransactionType.Income, 
            Amount = 1000, 
            Date = new DateTime(2024, 1, 15),
            IsDeleted = false 
        });
        account.Transactions.Add(new Transaction 
        { 
            Type = TransactionType.Income, 
            Amount = 500, 
            Date = new DateTime(2024, 1, 20),
            IsDeleted = false 
        });
        account.Transactions.Add(new Transaction 
        { 
            Type = TransactionType.Expense, 
            Amount = 200, 
            Date = new DateTime(2024, 1, 10),
            IsDeleted = false 
        }); // Should not be included
        account.Transactions.Add(new Transaction 
        { 
            Type = TransactionType.Income, 
            Amount = 300, 
            Date = new DateTime(2024, 2, 1),
            IsDeleted = false 
        }); // Outside date range
        account.Transactions.Add(new Transaction 
        { 
            Type = TransactionType.Income, 
            Amount = 100, 
            Date = new DateTime(2024, 1, 25),
            IsDeleted = true 
        }); // Deleted transaction

        // Act
        var result = account.CalculateIncome(startDate, endDate);

        // Assert
        Assert.Equal(1500, result);
    }

    [Fact]
    public void CalculateExpenses_ShouldSumExpenseTransactionsInDateRange()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 1, 31);
        
        var account = new Account();
        account.Transactions.Add(new Transaction 
        { 
            Type = TransactionType.Expense, 
            Amount = 200, 
            Date = new DateTime(2024, 1, 10),
            IsDeleted = false 
        });
        account.Transactions.Add(new Transaction 
        { 
            Type = TransactionType.Expense, 
            Amount = 150, 
            Date = new DateTime(2024, 1, 25),
            IsDeleted = false 
        });
        account.Transactions.Add(new Transaction 
        { 
            Type = TransactionType.Income, 
            Amount = 1000, 
            Date = new DateTime(2024, 1, 15),
            IsDeleted = false 
        }); // Should not be included
        account.Transactions.Add(new Transaction 
        { 
            Type = TransactionType.Expense, 
            Amount = 100, 
            Date = new DateTime(2024, 2, 1),
            IsDeleted = false 
        }); // Outside date range
        account.Transactions.Add(new Transaction 
        { 
            Type = TransactionType.Expense, 
            Amount = 75, 
            Date = new DateTime(2024, 1, 20),
            IsDeleted = true 
        }); // Deleted transaction

        // Act
        var result = account.CalculateExpenses(startDate, endDate);

        // Assert
        Assert.Equal(350, result);
    }

    [Fact]
    public void CalculateIncome_WithNoTransactions_ShouldReturnZero()
    {
        // Arrange
        var account = new Account();
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 1, 31);

        // Act
        var result = account.CalculateIncome(startDate, endDate);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void CalculateExpenses_WithNoTransactions_ShouldReturnZero()
    {
        // Arrange
        var account = new Account();
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 1, 31);

        // Act
        var result = account.CalculateExpenses(startDate, endDate);

        // Assert
        Assert.Equal(0, result);
    }
}