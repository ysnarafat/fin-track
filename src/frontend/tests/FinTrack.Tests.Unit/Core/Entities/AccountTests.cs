using FinTrack.Core.Entities;
using FinTrack.Core.Enums;

namespace FinTrack.Tests.Unit.Core.Entities;

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
        Assert.NotNull(account.Goals);
    }

    public static IEnumerable<object[]> AvailableBalanceTestData =>
        new List<object[]>
        {
            new object[] { AccountType.Checking, 1000m, null, 1000m },
            new object[] { AccountType.Savings, 5000m, null, 5000m },
            new object[] { AccountType.CreditCard, -500m, 2000m, 1500m },
            new object[] { AccountType.CreditCard, -2500m, 2000m, -500m },
            new object[] { AccountType.Investment, 10000m, null, 10000m }
        };

    [Theory]
    [MemberData(nameof(AvailableBalanceTestData))]
    public void AvailableBalance_ShouldCalculateCorrectly(AccountType type, decimal balance, decimal? creditLimit, decimal expected)
    {
        // Arrange
        var account = new Account
        {
            Type = type,
            Balance = balance,
            CreditLimit = creditLimit
        };

        // Act
        var result = account.AvailableBalance;

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(1000.50)]
    [InlineData(-500.25)]
    [InlineData(0)]
    public void FormattedBalance_ShouldFormatCorrectly(decimal balance)
    {
        // Arrange
        var account = new Account { Balance = balance };

        // Act
        var result = account.FormattedBalance;

        // Assert
        // Verify it's formatted as currency (not empty and contains numeric value)
        Assert.NotEmpty(result);
        // Remove any formatting characters and verify the numeric value is present
        var numericPart = result.Replace(",", "").Replace("$", "").Replace("¤", "").Replace("(", "").Replace(")", "").Trim();
        Assert.Contains(Math.Abs(balance).ToString("F2"), numericPart);
    }

    public static IEnumerable<object[]> IsOverdrawnTestData =>
        new List<object[]>
        {
            new object[] { AccountType.Checking, 1000m, null, false },
            new object[] { AccountType.Checking, -100m, null, true },
            new object[] { AccountType.Savings, -50m, null, true },
            new object[] { AccountType.CreditCard, -1500m, 2000m, false },
            new object[] { AccountType.CreditCard, -2500m, 2000m, true },
            new object[] { AccountType.CreditCard, -500m, null, false } // No credit limit set
        };

    [Theory]
    [MemberData(nameof(IsOverdrawnTestData))]
    public void IsOverdrawn_ShouldCalculateCorrectly(AccountType type, decimal balance, decimal? creditLimit, bool expected)
    {
        // Arrange
        var account = new Account
        {
            Type = type,
            Balance = balance,
            CreditLimit = creditLimit
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
        var originalVersion = account.Version;
        var originalUpdatedAt = account.UpdatedAt;
        
        Thread.Sleep(1); // Ensure timestamp difference

        // Act
        account.UpdateBalance(250.50m);

        // Assert
        Assert.Equal(1250.50m, account.Balance);
        Assert.Equal(originalVersion + 1, account.Version);
        Assert.True(account.UpdatedAt > originalUpdatedAt);
    }

    [Fact]
    public void UpdateBalance_WithNegativeAmount_ShouldSubtractFromBalance()
    {
        // Arrange
        var account = new Account { Balance = 1000 };

        // Act
        account.UpdateBalance(-250.50m);

        // Assert
        Assert.Equal(749.50m, account.Balance);
    }

    [Fact]
    public void SetBalance_ShouldSetExactAmountAndMarkAsModified()
    {
        // Arrange
        var account = new Account { Balance = 1000 };
        var originalVersion = account.Version;
        var originalUpdatedAt = account.UpdatedAt;
        
        Thread.Sleep(1);

        // Act
        account.SetBalance(2500.75m);

        // Assert
        Assert.Equal(2500.75m, account.Balance);
        Assert.Equal(originalVersion + 1, account.Version);
        Assert.True(account.UpdatedAt > originalUpdatedAt);
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
            Currency = "USD",
            Type = AccountType.Checking
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
            Currency = currency,
            Type = AccountType.Checking
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
            Type = AccountType.Savings,
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
            Date = new DateTime(2024, 1, 25),
            IsDeleted = false
        });
        account.Transactions.Add(new Transaction 
        { 
            Type = TransactionType.Expense, 
            Amount = 200, 
            Date = new DateTime(2024, 1, 20),
            IsDeleted = false
        });
        account.Transactions.Add(new Transaction 
        { 
            Type = TransactionType.Income, 
            Amount = 300, 
            Date = new DateTime(2024, 2, 5), // Outside date range
            IsDeleted = false
        });
        account.Transactions.Add(new Transaction 
        { 
            Type = TransactionType.Income, 
            Amount = 100, 
            Date = new DateTime(2024, 1, 10),
            IsDeleted = true // Deleted transaction
        });

        // Act
        var result = account.CalculateIncome(startDate, endDate);

        // Assert
        Assert.Equal(1500, result); // 1000 + 500
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
            Date = new DateTime(2024, 1, 15),
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
            Date = new DateTime(2024, 1, 20),
            IsDeleted = false
        });
        account.Transactions.Add(new Transaction 
        { 
            Type = TransactionType.Expense, 
            Amount = 100, 
            Date = new DateTime(2024, 2, 5), // Outside date range
            IsDeleted = false
        });
        account.Transactions.Add(new Transaction 
        { 
            Type = TransactionType.Expense, 
            Amount = 50, 
            Date = new DateTime(2024, 1, 10),
            IsDeleted = true // Deleted transaction
        });

        // Act
        var result = account.CalculateExpenses(startDate, endDate);

        // Assert
        Assert.Equal(350, result); // 200 + 150
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

    [Theory]
    [InlineData("Test Account")]
    [InlineData("My Savings Account")]
    [InlineData("Credit Card - Main")]
    public void Name_ShouldAcceptValidValues(string name)
    {
        // Arrange & Act
        var account = new Account { Name = name };

        // Assert
        Assert.Equal(name, account.Name);
    }

    [Theory]
    [InlineData("USD")]
    [InlineData("EUR")]
    [InlineData("GBP")]
    public void Currency_ShouldAcceptValidThreeLetterCodes(string currency)
    {
        // Arrange & Act
        var account = new Account { Currency = currency };

        // Assert
        Assert.Equal(currency, account.Currency);
    }
}