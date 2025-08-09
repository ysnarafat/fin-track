using FinTrack.Core.Entities;
using FinTrack.Core.Enums;
using Xunit;

namespace FinTrack.Tests.Unit.Domain;

public class TransactionTests
{
    [Fact]
    public void Constructor_SetsDefaultValues()
    {
        // Act
        var transaction = new Transaction();
        
        // Assert
        Assert.Equal(DateTime.Today, transaction.Date);
        Assert.Equal(TransactionType.Expense, transaction.Type);
        Assert.False(transaction.IsReconciled);
        Assert.Equal(SyncStatus.PendingCreate, transaction.SyncStatus);
        Assert.NotEmpty(transaction.SyncId);
    }
    
    [Fact]
    public void SignedAmount_ReturnsCorrectValue_ForIncomeTransaction()
    {
        // Arrange
        var transaction = new Transaction
        {
            Amount = 100m,
            Type = TransactionType.Income
        };
        
        // Act & Assert
        Assert.Equal(100m, transaction.SignedAmount);
    }
    
    [Fact]
    public void SignedAmount_ReturnsCorrectValue_ForExpenseTransaction()
    {
        // Arrange
        var transaction = new Transaction
        {
            Amount = 100m,
            Type = TransactionType.Expense
        };
        
        // Act & Assert
        Assert.Equal(-100m, transaction.SignedAmount);
    }
    
    [Fact]
    public void AbsoluteAmount_ReturnsPositiveValue()
    {
        // Arrange
        var transaction = new Transaction { Amount = -100m };
        
        // Act & Assert
        Assert.Equal(100m, transaction.AbsoluteAmount);
    }
    
    [Theory]
    [InlineData(0, false)]
    [InlineData(-50, false)]
    [InlineData(100, true)]
    public void IsValid_ValidatesAmount(decimal amount, bool expected)
    {
        // Arrange
        var transaction = new Transaction
        {
            Amount = amount,
            Description = "Test",
            CategoryId = 1,
            AccountId = 1
        };
        
        // Act & Assert
        Assert.Equal(expected, transaction.IsValid());
    }    
    
[Theory]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("Valid Description", true)]
    public void IsValid_ValidatesDescription(string description, bool expected)
    {
        // Arrange
        var transaction = new Transaction
        {
            Amount = 100m,
            Description = description,
            CategoryId = 1,
            AccountId = 1
        };
        
        // Act & Assert
        Assert.Equal(expected, transaction.IsValid());
    }
    
    [Fact]
    public void IsValid_ValidatesTransferTransaction()
    {
        // Arrange
        var transaction = new Transaction
        {
            Amount = 100m,
            Description = "Transfer",
            CategoryId = 1,
            AccountId = 1,
            Type = TransactionType.Transfer,
            TransferToAccountId = 2
        };
        
        // Act & Assert
        Assert.True(transaction.IsValid());
    }
    
    [Fact]
    public void IsValid_FailsForTransferToSameAccount()
    {
        // Arrange
        var transaction = new Transaction
        {
            Amount = 100m,
            Description = "Transfer",
            CategoryId = 1,
            AccountId = 1,
            Type = TransactionType.Transfer,
            TransferToAccountId = 1 // Same as AccountId
        };
        
        // Act & Assert
        Assert.False(transaction.IsValid());
    }
    
    [Fact]
    public void CreateTransferCounterpart_CreatesCorrectTransaction()
    {
        // Arrange
        var originalTransaction = new Transaction
        {
            Id = 1,
            Amount = 100m,
            Description = "Transfer to savings",
            CategoryId = 1,
            AccountId = 1,
            Type = TransactionType.Transfer,
            Account = new Account { Name = "Checking" }
        };
        
        // Act
        var counterpart = originalTransaction.CreateTransferCounterpart(2);
        
        // Assert
        Assert.Equal(100m, counterpart.Amount);
        Assert.Equal("Transfer from Checking", counterpart.Description);
        Assert.Equal(1, counterpart.CategoryId);
        Assert.Equal(2, counterpart.AccountId);
        Assert.Equal(TransactionType.Transfer, counterpart.Type);
        Assert.Equal(1, counterpart.TransferToAccountId);
        Assert.Equal(1, counterpart.LinkedTransactionId);
    }
    
    [Fact]
    public void CreateTransferCounterpart_ThrowsForNonTransferTransaction()
    {
        // Arrange
        var transaction = new Transaction
        {
            Type = TransactionType.Expense
        };
        
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => 
            transaction.CreateTransferCounterpart(2));
    }
}