using FinTrack.Core.Entities;
using FinTrack.Core.Enums;

namespace FinTrack.Tests.Unit.Domain;

/// <summary>
/// Unit tests for Transaction entity
/// </summary>
public class TransactionTests
{
    [Fact]
    public void Constructor_ShouldInitializeDefaultValues()
    {
        // Arrange & Act
        var transaction = new Transaction();

        // Assert
        Assert.Equal(DateTime.Today, transaction.Date);
        Assert.Equal(TransactionType.Expense, transaction.Type);
        Assert.False(transaction.IsReconciled);
        Assert.Equal(0, transaction.Amount);
        Assert.Empty(transaction.Description);
        Assert.Equal(0, transaction.CategoryId);
        Assert.Equal(0, transaction.AccountId);
        Assert.Null(transaction.ReferenceNumber);
        Assert.Null(transaction.Notes);
        Assert.Null(transaction.TransferToAccountId);
        Assert.Null(transaction.LinkedTransactionId);
        Assert.Null(transaction.ReconciledAt);
    }

    [Theory]
    [InlineData(100.50, 100.50)]
    [InlineData(-75.25, 75.25)]
    [InlineData(0, 0)]
    public void AbsoluteAmount_ShouldReturnPositiveValue(decimal amount, decimal expected)
    {
        // Arrange
        var transaction = new Transaction { Amount = amount };

        // Act
        var result = transaction.AbsoluteAmount;

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(TransactionType.Income, 100.50, 100.50)]
    [InlineData(TransactionType.Income, -100.50, 100.50)] // Should convert negative to positive for income
    [InlineData(TransactionType.Expense, 100.50, -100.50)] // Should convert positive to negative for expense
    [InlineData(TransactionType.Expense, -100.50, -100.50)]
    [InlineData(TransactionType.Transfer, 100.50, 100.50)] // Transfer preserves sign
    [InlineData(TransactionType.Transfer, -100.50, -100.50)]
    public void SignedAmount_ShouldReturnCorrectSignBasedOnType(TransactionType type, decimal amount, decimal expected)
    {
        // Arrange
        var transaction = new Transaction 
        { 
            Type = type, 
            Amount = amount 
        };

        // Act
        var result = transaction.SignedAmount;

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void IsValid_WithValidData_ShouldReturnTrue()
    {
        // Arrange
        var transaction = new Transaction
        {
            Amount = 100.50m,
            Description = "Test Transaction",
            CategoryId = 1,
            AccountId = 1,
            Date = DateTime.Today,
            Type = TransactionType.Expense
        };

        // Act
        var result = transaction.IsValid();

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData(0, "Valid Description", 1, 1)] // Zero amount
    [InlineData(-10, "Valid Description", 1, 1)] // Negative amount
    public void IsValid_WithInvalidAmount_ShouldReturnFalse(decimal amount, string description, int categoryId, int accountId)
    {
        // Arrange
        var transaction = new Transaction
        {
            Amount = amount,
            Description = description,
            CategoryId = categoryId,
            AccountId = accountId,
            Date = DateTime.Today
        };

        // Act
        var result = transaction.IsValid();

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void IsValid_WithInvalidDescription_ShouldReturnFalse(string description)
    {
        // Arrange
        var transaction = new Transaction
        {
            Amount = 100.50m,
            Description = description,
            CategoryId = 1,
            AccountId = 1,
            Date = DateTime.Today
        };

        // Act
        var result = transaction.IsValid();

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void IsValid_WithInvalidCategoryId_ShouldReturnFalse(int categoryId)
    {
        // Arrange
        var transaction = new Transaction
        {
            Amount = 100.50m,
            Description = "Valid Description",
            CategoryId = categoryId,
            AccountId = 1,
            Date = DateTime.Today
        };

        // Act
        var result = transaction.IsValid();

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void IsValid_WithInvalidAccountId_ShouldReturnFalse(int accountId)
    {
        // Arrange
        var transaction = new Transaction
        {
            Amount = 100.50m,
            Description = "Valid Description",
            CategoryId = 1,
            AccountId = accountId,
            Date = DateTime.Today
        };

        // Act
        var result = transaction.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_WithFutureDateBeyondTomorrow_ShouldReturnFalse()
    {
        // Arrange
        var transaction = new Transaction
        {
            Amount = 100.50m,
            Description = "Valid Description",
            CategoryId = 1,
            AccountId = 1,
            Date = DateTime.Today.AddDays(2) // Day after tomorrow
        };

        // Act
        var result = transaction.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_WithTomorrowDate_ShouldReturnTrue()
    {
        // Arrange
        var transaction = new Transaction
        {
            Amount = 100.50m,
            Description = "Valid Description",
            CategoryId = 1,
            AccountId = 1,
            Date = DateTime.Today.AddDays(1) // Tomorrow
        };

        // Act
        var result = transaction.IsValid();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_TransferWithoutDestinationAccount_ShouldReturnFalse()
    {
        // Arrange
        var transaction = new Transaction
        {
            Amount = 100.50m,
            Description = "Transfer Transaction",
            CategoryId = 1,
            AccountId = 1,
            Date = DateTime.Today,
            Type = TransactionType.Transfer,
            TransferToAccountId = null
        };

        // Act
        var result = transaction.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_TransferWithInvalidDestinationAccount_ShouldReturnFalse()
    {
        // Arrange
        var transaction = new Transaction
        {
            Amount = 100.50m,
            Description = "Transfer Transaction",
            CategoryId = 1,
            AccountId = 1,
            Date = DateTime.Today,
            Type = TransactionType.Transfer,
            TransferToAccountId = 0
        };

        // Act
        var result = transaction.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_TransferToSameAccount_ShouldReturnFalse()
    {
        // Arrange
        var transaction = new Transaction
        {
            Amount = 100.50m,
            Description = "Transfer Transaction",
            CategoryId = 1,
            AccountId = 1,
            Date = DateTime.Today,
            Type = TransactionType.Transfer,
            TransferToAccountId = 1 // Same as AccountId
        };

        // Act
        var result = transaction.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_ValidTransfer_ShouldReturnTrue()
    {
        // Arrange
        var transaction = new Transaction
        {
            Amount = 100.50m,
            Description = "Transfer Transaction",
            CategoryId = 1,
            AccountId = 1,
            Date = DateTime.Today,
            Type = TransactionType.Transfer,
            TransferToAccountId = 2
        };

        // Act
        var result = transaction.IsValid();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CreateTransferCounterpart_WithValidTransfer_ShouldCreateCorrectCounterpart()
    {
        // Arrange
        var originalTransaction = new Transaction
        {
            Id = 1,
            Amount = 100.50m,
            Description = "Transfer to Savings",
            CategoryId = 1,
            AccountId = 1,
            Date = DateTime.Today,
            Type = TransactionType.Transfer,
            ReferenceNumber = "REF123",
            Notes = "Monthly savings",
            TransferToAccountId = 2,
            IsReconciled = true,
            ReconciledAt = DateTime.Today,
            Account = new Account { Name = "Checking" }
        };

        // Act
        var counterpart = originalTransaction.CreateTransferCounterpart(2);

        // Assert
        Assert.Equal(originalTransaction.Amount, counterpart.Amount);
        Assert.Equal("Transfer from Checking", counterpart.Description);
        Assert.Equal(originalTransaction.Date, counterpart.Date);
        Assert.Equal(originalTransaction.CategoryId, counterpart.CategoryId);
        Assert.Equal(2, counterpart.AccountId);
        Assert.Equal(TransactionType.Transfer, counterpart.Type);
        Assert.Equal(originalTransaction.ReferenceNumber, counterpart.ReferenceNumber);
        Assert.Equal(originalTransaction.Notes, counterpart.Notes);
        Assert.Equal(1, counterpart.TransferToAccountId);
        Assert.Equal(1, counterpart.LinkedTransactionId);
        Assert.Equal(originalTransaction.IsReconciled, counterpart.IsReconciled);
        Assert.Equal(originalTransaction.ReconciledAt, counterpart.ReconciledAt);
    }

    [Fact]
    public void CreateTransferCounterpart_WithNonTransferTransaction_ShouldThrowException()
    {
        // Arrange
        var transaction = new Transaction
        {
            Type = TransactionType.Expense
        };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            transaction.CreateTransferCounterpart(2));
        
        Assert.Equal("Can only create counterpart for transfer transactions", exception.Message);
    }

    [Fact]
    public void CreateTransferCounterpart_WithNullAccount_ShouldUseDefaultAccountName()
    {
        // Arrange
        var originalTransaction = new Transaction
        {
            Id = 1,
            Amount = 100.50m,
            Description = "Transfer to Savings",
            CategoryId = 1,
            AccountId = 1,
            Date = DateTime.Today,
            Type = TransactionType.Transfer,
            TransferToAccountId = 2,
            Account = null // No account navigation property
        };

        // Act
        var counterpart = originalTransaction.CreateTransferCounterpart(2);

        // Assert
        Assert.Equal("Transfer from Account", counterpart.Description);
    }
}