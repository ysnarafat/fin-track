using FinTrack.Core.Entities;
using FinTrack.Core.Enums;
using Xunit;

namespace FinTrack.Tests.Unit.Domain;

public class AccountTests
{
    [Fact]
    public void Constructor_SetsDefaultValues()
    {
        // Act
        var account = new Account();
        
        // Assert
        Assert.Equal(AccountType.Checking, account.Type);
        Assert.Equal("USD", account.Currency);
        Assert.True(account.IsActive);
        Assert.Equal(0m, account.Balance);
        Assert.Equal(0m, account.InitialBalance);
        Assert.Equal(SyncStatus.PendingCreate, account.SyncStatus);
        Assert.NotEmpty(account.SyncId);
    }
    
    [Fact]
    public void AvailableBalance_ReturnsBalance_ForNonCreditCardAccount()
    {
        // Arrange
        var account = new Account
        {
            Type = AccountType.Checking,
            Balance = 1000m
        };
        
        // Act & Assert
        Assert.Equal(1000m, account.AvailableBalance);
    }
    
    [Fact]
    public void AvailableBalance_ReturnsBalancePlusCreditLimit_ForCreditCardAccount()
    {
        // Arrange
        var account = new Account
        {
            Type = AccountType.CreditCard,
            Balance = -500m, // Debt
            CreditLimit = 2000m
        };
        
        // Act & Assert
        Assert.Equal(1500m, account.AvailableBalance); // 2000 - 500
    }
    
    [Fact]
    public void IsOverdrawn_ReturnsFalse_ForPositiveBalance()
    {
        // Arrange
        var account = new Account { Balance = 100m };
        
        // Act & Assert
        Assert.False(account.IsOverdrawn);
    }
    
    [Fact]
    public void IsOverdrawn_ReturnsTrue_ForNegativeBalance()
    {
        // Arrange
        var account = new Account { Balance = -100m };
        
        // Act & Assert
        Assert.True(account.IsOverdrawn);
    }
    
    [Fact]
    public void IsOverdrawn_ReturnsTrue_ForCreditCardOverLimit()
    {
        // Arrange
        var account = new Account
        {
            Type = AccountType.CreditCard,
            Balance = -2500m,
            CreditLimit = 2000m
        };
        
        // Act & Assert
        Assert.True(account.IsOverdrawn);
    }    

    [Fact]
    public void UpdateBalance_UpdatesBalanceAndMarksAsModified()
    {
        // Arrange
        var account = new Account { Balance = 100m };
        account.MarkAsSynced(); // Reset sync status
        
        // Act
        account.UpdateBalance(50m);
        
        // Assert
        Assert.Equal(150m, account.Balance);
        Assert.Equal(SyncStatus.PendingUpdate, account.SyncStatus);
    }
    
    [Fact]
    public void SetBalance_SetsBalanceAndMarksAsModified()
    {
        // Arrange
        var account = new Account { Balance = 100m };
        account.MarkAsSynced(); // Reset sync status
        
        // Act
        account.SetBalance(200m);
        
        // Assert
        Assert.Equal(200m, account.Balance);
        Assert.Equal(SyncStatus.PendingUpdate, account.SyncStatus);
    }
    
    [Theory]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("Valid Account", true)]
    public void IsValid_ValidatesName(string name, bool expected)
    {
        // Arrange
        var account = new Account { Name = name };
        
        // Act & Assert
        Assert.Equal(expected, account.IsValid());
    }
    
    [Theory]
    [InlineData("", false)]
    [InlineData("US", false)]
    [InlineData("USD", true)]
    [InlineData("USDD", false)]
    public void IsValid_ValidatesCurrency(string currency, bool expected)
    {
        // Arrange
        var account = new Account 
        { 
            Name = "Test Account",
            Currency = currency 
        };
        
        // Act & Assert
        Assert.Equal(expected, account.IsValid());
    }
    
    [Fact]
    public void FormattedBalance_ReturnsFormattedString()
    {
        // Arrange
        var account = new Account { Balance = 1234.56m };
        
        // Act & Assert
        Assert.Contains("1,234.56", account.FormattedBalance);
    }
    
    [Fact]
    public void CalculateIncome_ReturnsCorrectSum()
    {
        // Arrange
        var account = new Account();
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 1, 31);
        
        account.Transactions.Add(new Transaction 
        { 
            Amount = 1000m, 
            Type = TransactionType.Income, 
            Date = new DateTime(2024, 1, 15) 
        });
        account.Transactions.Add(new Transaction 
        { 
            Amount = 500m, 
            Type = TransactionType.Income, 
            Date = new DateTime(2024, 1, 20) 
        });
        account.Transactions.Add(new Transaction 
        { 
            Amount = 200m, 
            Type = TransactionType.Expense, 
            Date = new DateTime(2024, 1, 10) 
        });
        
        // Act
        var income = account.CalculateIncome(startDate, endDate);
        
        // Assert
        Assert.Equal(1500m, income);
    }
    
    [Fact]
    public void CalculateExpenses_ReturnsCorrectSum()
    {
        // Arrange
        var account = new Account();
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 1, 31);
        
        account.Transactions.Add(new Transaction 
        { 
            Amount = 200m, 
            Type = TransactionType.Expense, 
            Date = new DateTime(2024, 1, 10) 
        });
        account.Transactions.Add(new Transaction 
        { 
            Amount = 150m, 
            Type = TransactionType.Expense, 
            Date = new DateTime(2024, 1, 15) 
        });
        account.Transactions.Add(new Transaction 
        { 
            Amount = 1000m, 
            Type = TransactionType.Income, 
            Date = new DateTime(2024, 1, 20) 
        });
        
        // Act
        var expenses = account.CalculateExpenses(startDate, endDate);
        
        // Assert
        Assert.Equal(350m, expenses);
    }
}