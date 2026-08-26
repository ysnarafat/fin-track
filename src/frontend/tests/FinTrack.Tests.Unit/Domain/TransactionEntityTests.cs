using FinTrack.Core.Entities;
using FinTrack.Core.Enums;
using FinTrack.Tests.Unit.Helpers;

namespace FinTrack.Tests.Unit.Domain;

/// <summary>
/// Comprehensive unit tests for Transaction entity
/// </summary>
public class TransactionEntityTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Act
        var transaction = new Transaction();

        // Assert
        Assert.Equal(0m, transaction.Amount);
        Assert.Equal(string.Empty, transaction.Description);
        Assert.Equal(DateTime.Today, transaction.Date);
        Assert.Equal(0, transaction.CategoryId);
        Assert.Equal(0, transaction.AccountId);
        Assert.Equal(TransactionType.Expense, transaction.Type);
        Assert.Null(transaction.ReferenceNumber);
        Assert.Null(transaction.Notes);
        Assert.Null(transaction.TransferToAccountId);
        Assert.Null(transaction.LinkedTransactionId);
        Assert.False(transaction.IsReconciled);
        Assert.Null(transaction.ReconciledAt);
        
        // Verify BaseEntity properties are initialized
        Assert.Equal(SyncStatus.PendingCreate, transaction.SyncStatus);
        Assert.NotEmpty(transaction.SyncId);
    }

    [Theory]
    [InlineData(100.50)]
    [InlineData(0.01)]
    [InlineData(9999999.99)]
    public void AbsoluteAmount_WithPositiveAmount_ShouldReturnSameValue(decimal amount)
    {
        // Arrange
        var transaction = TestDataBuilder.Transaction()
            .WithAmount(amount)
            .Build();

        // Act
        var absoluteAmount = transaction.AbsoluteAmount;

        // Assert
        Assert.Equal(amount, absoluteAmount);
    }

    [Theory]
    [InlineData(-100.50, 100.50)]
    [InlineData(-0.01, 0.01)]
    [InlineData(-9999999.99, 9999999.99)]
    public void AbsoluteAmount_WithNegativeAmount_ShouldReturnPositiveValue(decimal amount, decimal expectedAbsolute)
    {
        // Arrange
        var transaction = TestDataBuilder.Transaction()
            .WithAmount(amount)
            .Build();

        // Act
        var absoluteAmount = transaction.AbsoluteAmount;

        // Assert
        Assert.Equal(expectedAbsolute, absoluteAmount);
    }

    [Theory]
    [InlineData(TransactionType.Income, 100.50, 100.50)]
    [InlineData(TransactionType.Income, -100.50, 100.50)] // Income should always be positive
    [InlineData(TransactionType.Expense, 100.50, -100.50)]
    [InlineData(TransactionType.Expense, -100.50, -100.50)] // Expense should always be negative
    [InlineData(TransactionType.Transfer, 100.50, 100.50)]
    [InlineData(TransactionType.Transfer, -100.50, -100.50)] // Transfer preserves sign
    public void SignedAmount_WithDifferentTypes_ShouldReturnCorrectSign(
        TransactionType type, decimal amount, decimal expectedSigned)
    {
        // Arrange
        var transaction = TestDataBuilder.Transaction()
            .WithAmount(amount)
            .WithType(type)
            .Build();

        // Act
        var signedAmount = transaction.SignedAmount;

        // Assert
        Assert.Equal(expectedSigned, signedAmount);
    }

    [Fact]
    public void IsValid_WithValidTransaction_ShouldReturnTrue()
    {
        // Arrange
        var transaction = TestDataBuilder.Transaction()
            .WithAmount(100m)
            .WithDescription("Valid Transaction")
            .WithDate(DateTime.Today)
            .WithCategoryId(1)
            .WithAccountId(1)
            .WithType(TransactionType.Expense)
            .Build();

        // Act
        var isValid = transaction.IsValid();

        // Assert
        Assert.True(isValid);
    }

    [Theory]
    [InlineData(0)] // Zero amount
    [InlineData(-100)] // Negative amount
    public void IsValid_WithInvalidAmount_ShouldReturnFalse(decimal amount)
    {
        // Arrange
        var transaction = TestDataBuilder.Transaction()
            .WithAmount(amount)
            .WithDescription("Valid Description")
            .WithCategoryId(1)
            .WithAccountId(1)
            .Build();

        // Act
        var isValid = transaction.IsValid();

        // Assert
        Assert.False(isValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void IsValid_WithInvalidDescription_ShouldReturnFalse(string description)
    {
        // Arrange
        var transaction = TestDataBuilder.Transaction()
            .WithAmount(100m)
            .WithDescription(description)
            .WithCategoryId(1)
            .WithAccountId(1)
            .Build();

        // Act
        var isValid = transaction.IsValid();

        // Assert
        Assert.False(isValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void IsValid_WithInvalidCategoryId_ShouldReturnFalse(int categoryId)
    {
        // Arrange
        var transaction = TestDataBuilder.Transaction()
            .WithAmount(100m)
            .WithDescription("Valid Description")
            .WithCategoryId(categoryId)
            .WithAccountId(1)
            .Build();

        // Act
        var isValid = transaction.IsValid();

        // Assert
        Assert.False(isValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void IsValid_WithInvalidAccountId_ShouldReturnFalse(int accountId)
    {
        // Arrange
        var transaction = TestDataBuilder.Transaction()
            .WithAmount(100m)
            .WithDescription("Valid Description")
            .WithCategoryId(1)
            .WithAccountId(accountId)
            .Build();

        // Act
        var isValid = transaction.IsValid();

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void IsValid_WithFutureDateBeyondTomorrow_ShouldReturnFalse()
    {
        // Arrange
        var futureDate = DateTime.Today.AddDays(2);
        var transaction = TestDataBuilder.Transaction()
            .WithAmount(100m)
            .WithDescription("Future Transaction")
            .WithDate(futureDate)
            .WithCategoryId(1)
            .WithAccountId(1)
            .Build();

        // Act
        var isValid = transaction.IsValid();

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void IsValid_WithTomorrowDate_ShouldReturnTrue()
    {
        // Arrange
        var tomorrowDate = DateTime.Today.AddDays(1);
        var transaction = TestDataBuilder.Transaction()
            .WithAmount(100m)
            .WithDescription("Tomorrow Transaction")
            .WithDate(tomorrowDate)
            .WithCategoryId(1)
            .WithAccountId(1)
            .Build();

        // Act
        var isValid = transaction.IsValid();

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void IsValid_TransferWithoutDestinationAccount_ShouldReturnFalse()
    {
        // Arrange
        var transaction = TestDataBuilder.Transaction()
            .WithAmount(100m)
            .WithDescription("Transfer Transaction")
            .WithType(TransactionType.Transfer)
            .WithCategoryId(1)
            .WithAccountId(1)
            .Build(); // No TransferToAccountId set

        // Act
        var isValid = transaction.IsValid();

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void IsValid_TransferWithInvalidDestinationAccount_ShouldReturnFalse()
    {
        // Arrange
        var transaction = TestDataBuilder.Transaction()
            .WithAmount(100m)
            .WithDescription("Transfer Transaction")
            .WithType(TransactionType.Transfer)
            .WithCategoryId(1)
            .WithAccountId(1)
            .AsTransfer(0) // Invalid destination account ID
            .Build();

        // Act
        var isValid = transaction.IsValid();

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void IsValid_TransferToSameAccount_ShouldReturnFalse()
    {
        // Arrange
        var transaction = TestDataBuilder.Transaction()
            .WithAmount(100m)
            .WithDescription("Transfer Transaction")
            .WithType(TransactionType.Transfer)
            .WithCategoryId(1)
            .WithAccountId(1)
            .AsTransfer(1) // Same as source account
            .Build();

        // Act
        var isValid = transaction.IsValid();

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void IsValid_ValidTransfer_ShouldReturnTrue()
    {
        // Arrange
        var transaction = TestDataBuilder.Transaction()
            .WithAmount(100m)
            .WithDescription("Valid Transfer")
            .WithType(TransactionType.Transfer)
            .WithCategoryId(1)
            .WithAccountId(1)
            .AsTransfer(2) // Different destination account
            .Build();

        // Act
        var isValid = transaction.IsValid();

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void CreateTransferCounterpart_WithValidTransfer_ShouldCreateCounterpart()
    {
        // Arrange
        var sourceAccount = TestDataBuilder.Account()
            .WithId(1)
            .WithName("Checking Account")
            .Build();

        var originalTransaction = TestDataBuilder.Transaction()
            .WithId(10)
            .WithAmount(500m)
            .WithDescription("Transfer to Savings")
            .WithType(TransactionType.Transfer)
            .WithCategoryId(1)
            .WithAccountId(1)
            .WithReferenceNumber("TXN123")
            .WithNotes("Monthly savings transfer")
            .AsTransfer(2)
            .AsReconciled()
            .Build();

        originalTransaction.Account = sourceAccount;

        // Act
        var counterpart = originalTransaction.CreateTransferCounterpart(2);

        // Assert
        Assert.NotNull(counterpart);
        Assert.Equal(500m, counterpart.Amount);
        Assert.Equal("Transfer from Checking Account", counterpart.Description);
        Assert.Equal(originalTransaction.Date, counterpart.Date);
        Assert.Equal(originalTransaction.CategoryId, counterpart.CategoryId);
        Assert.Equal(2, counterpart.AccountId);
        Assert.Equal(TransactionType.Transfer, counterpart.Type);
        Assert.Equal(originalTransaction.ReferenceNumber, counterpart.ReferenceNumber);
        Assert.Equal(originalTransaction.Notes, counterpart.Notes);
        Assert.Equal(1, counterpart.TransferToAccountId);
        Assert.Equal(10, counterpart.LinkedTransactionId);
        Assert.Equal(originalTransaction.IsReconciled, counterpart.IsReconciled);
        Assert.Equal(originalTransaction.ReconciledAt, counterpart.ReconciledAt);
    }

    [Fact]
    public void CreateTransferCounterpart_WithNonTransferTransaction_ShouldThrowException()
    {
        // Arrange
        var transaction = TestDataBuilder.Transaction()
            .WithType(TransactionType.Expense)
            .Build();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => transaction.CreateTransferCounterpart(2));
        
        Assert.Contains("Can only create counterpart for transfer transactions", exception.Message);
    }

    [Fact]
    public void CreateTransferCounterpart_WithoutSourceAccount_ShouldUseGenericDescription()
    {
        // Arrange
        var originalTransaction = TestDataBuilder.Transaction()
            .WithAmount(300m)
            .WithType(TransactionType.Transfer)
            .WithCategoryId(1)
            .WithAccountId(1)
            .AsTransfer(2)
            .Build();
        // Account navigation property is null

        // Act
        var counterpart = originalTransaction.CreateTransferCounterpart(2);

        // Assert
        Assert.Equal("Transfer from Account", counterpart.Description);
    }

    [Fact]
    public void NavigationProperties_ShouldBeSettableAndGettable()
    {
        // Arrange
        var transaction = new Transaction();
        var category = TestDataBuilder.Category().WithName("Test Category").Build();
        var account = TestDataBuilder.Account().WithName("Test Account").Build();
        var transferAccount = TestDataBuilder.Account().WithName("Transfer Account").Build();
        var linkedTransaction = TestDataBuilder.Transaction().WithDescription("Linked").Build();

        // Act
        transaction.Category = category;
        transaction.Account = account;
        transaction.TransferToAccount = transferAccount;
        transaction.LinkedTransaction = linkedTransaction;

        // Assert
        Assert.Equal(category, transaction.Category);
        Assert.Equal(account, transaction.Account);
        Assert.Equal(transferAccount, transaction.TransferToAccount);
        Assert.Equal(linkedTransaction, transaction.LinkedTransaction);
    }

    [Fact]
    public void ReconciledProperties_ShouldWorkCorrectly()
    {
        // Arrange
        var transaction = TestDataBuilder.Transaction().Build();
        var reconciledDate = DateTime.UtcNow;

        // Act
        transaction.IsReconciled = true;
        transaction.ReconciledAt = reconciledDate;

        // Assert
        Assert.True(transaction.IsReconciled);
        Assert.Equal(reconciledDate, transaction.ReconciledAt);
    }

    [Fact]
    public void AsReconciled_TestDataBuilderMethod_ShouldSetReconciledProperties()
    {
        // Arrange & Act
        var transaction = TestDataBuilder.Transaction()
            .AsReconciled()
            .Build();

        // Assert
        Assert.True(transaction.IsReconciled);
        Assert.NotNull(transaction.ReconciledAt);
        Assert.True(transaction.ReconciledAt <= DateTime.UtcNow);
    }

    [Fact]
    public void AsReconciled_WithSpecificDate_ShouldUseProvidedDate()
    {
        // Arrange
        var specificDate = DateTime.UtcNow.AddDays(-5);

        // Act
        var transaction = TestDataBuilder.Transaction()
            .AsReconciled(specificDate)
            .Build();

        // Assert
        Assert.True(transaction.IsReconciled);
        Assert.Equal(specificDate, transaction.ReconciledAt);
    }

    [Theory]
    [InlineData("REF123")]
    [InlineData("CHK-456")]
    [InlineData("")]
    [InlineData(null)]
    public void ReferenceNumber_ShouldAcceptValidValues(string referenceNumber)
    {
        // Arrange
        var transaction = TestDataBuilder.Transaction()
            .WithReferenceNumber(referenceNumber)
            .Build();

        // Act & Assert
        Assert.Equal(referenceNumber, transaction.ReferenceNumber);
    }

    [Theory]
    [InlineData("Short note")]
    [InlineData("This is a much longer note that contains more detailed information about the transaction")]
    [InlineData("")]
    [InlineData(null)]
    public void Notes_ShouldAcceptValidValues(string notes)
    {
        // Arrange
        var transaction = TestDataBuilder.Transaction()
            .WithNotes(notes)
            .Build();

        // Act & Assert
        Assert.Equal(notes, transaction.Notes);
    }

    [Fact]
    public void Transaction_ShouldInheritFromBaseEntity()
    {
        // Arrange & Act
        var transaction = new Transaction();

        // Assert
        Assert.IsAssignableFrom<BaseEntity>(transaction);
        Assert.True(transaction.Id >= 0);
        Assert.NotEmpty(transaction.SyncId);
        Assert.Equal(SyncStatus.PendingCreate, transaction.SyncStatus);
    }

    [Fact]
    public void Transaction_ShouldSupportModificationTracking()
    {
        // Arrange
        var transaction = TestDataBuilder.Transaction()
            .WithAmount(100m)
            .WithDescription("Original Description")
            .Build();

        transaction.MarkAsSynced(); // Start with synced status
        var originalVersion = transaction.Version;

        // Act
        transaction.Description = "Modified Description";
        transaction.MarkAsModified();

        // Assert
        Assert.Equal("Modified Description", transaction.Description);
        Assert.Equal(SyncStatus.PendingUpdate, transaction.SyncStatus);
        Assert.True(transaction.Version > originalVersion);
    }

    [Fact]
    public void Transaction_CompleteLifecycle_ShouldWorkCorrectly()
    {
        // Arrange & Act - Test complete transaction lifecycle
        
        // 1. Create new transaction
        var transaction = TestDataBuilder.Transaction()
            .WithAmount(250m)
            .WithDescription("Grocery Shopping")
            .WithType(TransactionType.Expense)
            .WithCategoryId(1)
            .WithAccountId(1)
            .Build();

        Assert.Equal(SyncStatus.PendingCreate, transaction.SyncStatus);
        Assert.True(transaction.IsValid());

        // 2. Sync to server
        transaction.MarkAsSynced();
        Assert.Equal(SyncStatus.Synced, transaction.SyncStatus);

        // 3. Modify transaction
        transaction.Amount = 275m;
        transaction.MarkAsModified();
        Assert.Equal(SyncStatus.PendingUpdate, transaction.SyncStatus);

        // 4. Reconcile transaction
        transaction.IsReconciled = true;
        transaction.ReconciledAt = DateTime.UtcNow;

        // 5. Sync changes
        transaction.MarkAsSynced();
        Assert.Equal(SyncStatus.Synced, transaction.SyncStatus);

        // 6. Delete transaction
        transaction.MarkAsDeleted();
        Assert.True(transaction.IsDeleted);
        Assert.Equal(SyncStatus.PendingDelete, transaction.SyncStatus);

        // Final assertions
        Assert.Equal(275m, transaction.Amount);
        Assert.True(transaction.IsReconciled);
        Assert.True(transaction.IsDeleted);
    }
}