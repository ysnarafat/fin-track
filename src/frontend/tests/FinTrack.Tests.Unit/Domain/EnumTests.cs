using FinTrack.Core.Enums;

namespace FinTrack.Tests.Unit.Domain;

/// <summary>
/// Unit tests for Core enums to ensure they have expected values
/// </summary>
public class EnumTests
{
    [Fact]
    public void SyncStatus_ShouldHaveExpectedValues()
    {
        // Assert
        Assert.Equal(0, (int)SyncStatus.Synced);
        Assert.Equal(1, (int)SyncStatus.PendingCreate);
        Assert.Equal(2, (int)SyncStatus.PendingUpdate);
        Assert.Equal(3, (int)SyncStatus.PendingDelete);
        Assert.Equal(4, (int)SyncStatus.SyncFailed);
        Assert.Equal(5, (int)SyncStatus.Conflict);
    }

    [Fact]
    public void SyncStatus_ShouldHaveAllExpectedMembers()
    {
        // Arrange
        var expectedValues = new[]
        {
            SyncStatus.Synced,
            SyncStatus.PendingCreate,
            SyncStatus.PendingUpdate,
            SyncStatus.PendingDelete,
            SyncStatus.SyncFailed,
            SyncStatus.Conflict
        };

        // Act
        var actualValues = Enum.GetValues<SyncStatus>();

        // Assert
        Assert.Equal(expectedValues.Length, actualValues.Length);
        foreach (var expectedValue in expectedValues)
        {
            Assert.Contains(expectedValue, actualValues);
        }
    }

    [Fact]
    public void SyncOperation_ShouldHaveExpectedValues()
    {
        // Assert
        Assert.Equal(0, (int)SyncOperation.Create);
        Assert.Equal(1, (int)SyncOperation.Update);
        Assert.Equal(2, (int)SyncOperation.Delete);
        Assert.Equal(3, (int)SyncOperation.None);
    }

    [Fact]
    public void SyncOperation_ShouldHaveAllExpectedMembers()
    {
        // Arrange
        var expectedValues = new[]
        {
            SyncOperation.Create,
            SyncOperation.Update,
            SyncOperation.Delete,
            SyncOperation.None
        };

        // Act
        var actualValues = Enum.GetValues<SyncOperation>();

        // Assert
        Assert.Equal(expectedValues.Length, actualValues.Length);
        foreach (var expectedValue in expectedValues)
        {
            Assert.Contains(expectedValue, actualValues);
        }
    }

    [Theory]
    [InlineData(SyncStatus.Synced, "Synced")]
    [InlineData(SyncStatus.PendingCreate, "PendingCreate")]
    [InlineData(SyncStatus.PendingUpdate, "PendingUpdate")]
    [InlineData(SyncStatus.PendingDelete, "PendingDelete")]
    [InlineData(SyncStatus.SyncFailed, "SyncFailed")]
    [InlineData(SyncStatus.Conflict, "Conflict")]
    public void SyncStatus_ToString_ShouldReturnCorrectName(SyncStatus status, string expectedName)
    {
        // Act
        var result = status.ToString();

        // Assert
        Assert.Equal(expectedName, result);
    }

    [Theory]
    [InlineData(SyncOperation.Create, "Create")]
    [InlineData(SyncOperation.Update, "Update")]
    [InlineData(SyncOperation.Delete, "Delete")]
    [InlineData(SyncOperation.None, "None")]
    public void SyncOperation_ToString_ShouldReturnCorrectName(SyncOperation operation, string expectedName)
    {
        // Act
        var result = operation.ToString();

        // Assert
        Assert.Equal(expectedName, result);
    }

    [Fact]
    public void SyncStatus_Parse_ShouldWorkCorrectly()
    {
        // Arrange & Act & Assert
        Assert.Equal(SyncStatus.Synced, Enum.Parse<SyncStatus>("Synced"));
        Assert.Equal(SyncStatus.PendingCreate, Enum.Parse<SyncStatus>("PendingCreate"));
        Assert.Equal(SyncStatus.PendingUpdate, Enum.Parse<SyncStatus>("PendingUpdate"));
        Assert.Equal(SyncStatus.PendingDelete, Enum.Parse<SyncStatus>("PendingDelete"));
        Assert.Equal(SyncStatus.SyncFailed, Enum.Parse<SyncStatus>("SyncFailed"));
        Assert.Equal(SyncStatus.Conflict, Enum.Parse<SyncStatus>("Conflict"));
    }

    [Fact]
    public void SyncOperation_Parse_ShouldWorkCorrectly()
    {
        // Arrange & Act & Assert
        Assert.Equal(SyncOperation.Create, Enum.Parse<SyncOperation>("Create"));
        Assert.Equal(SyncOperation.Update, Enum.Parse<SyncOperation>("Update"));
        Assert.Equal(SyncOperation.Delete, Enum.Parse<SyncOperation>("Delete"));
        Assert.Equal(SyncOperation.None, Enum.Parse<SyncOperation>("None"));
    }

    [Fact]
    public void SyncStatus_IsDefined_ShouldWorkForValidValues()
    {
        // Act & Assert
        Assert.True(Enum.IsDefined(typeof(SyncStatus), SyncStatus.Synced));
        Assert.True(Enum.IsDefined(typeof(SyncStatus), SyncStatus.PendingCreate));
        Assert.True(Enum.IsDefined(typeof(SyncStatus), SyncStatus.PendingUpdate));
        Assert.True(Enum.IsDefined(typeof(SyncStatus), SyncStatus.PendingDelete));
        Assert.True(Enum.IsDefined(typeof(SyncStatus), SyncStatus.SyncFailed));
        Assert.True(Enum.IsDefined(typeof(SyncStatus), SyncStatus.Conflict));
    }

    [Fact]
    public void SyncOperation_IsDefined_ShouldWorkForValidValues()
    {
        // Act & Assert
        Assert.True(Enum.IsDefined(typeof(SyncOperation), SyncOperation.Create));
        Assert.True(Enum.IsDefined(typeof(SyncOperation), SyncOperation.Update));
        Assert.True(Enum.IsDefined(typeof(SyncOperation), SyncOperation.Delete));
        Assert.True(Enum.IsDefined(typeof(SyncOperation), SyncOperation.None));
    }

    [Theory]
    [InlineData(99)]
    [InlineData(-1)]
    public void SyncStatus_IsDefined_ShouldReturnFalseForInvalidValues(int invalidValue)
    {
        // Act & Assert
        Assert.False(Enum.IsDefined(typeof(SyncStatus), invalidValue));
    }

    [Theory]
    [InlineData(99)]
    [InlineData(-1)]
    public void SyncOperation_IsDefined_ShouldReturnFalseForInvalidValues(int invalidValue)
    {
        // Act & Assert
        Assert.False(Enum.IsDefined(typeof(SyncOperation), invalidValue));
    }

    [Fact]
    public void TransactionType_ShouldHaveExpectedValues()
    {
        // Assert
        Assert.Equal(0, (int)TransactionType.Income);
        Assert.Equal(1, (int)TransactionType.Expense);
        Assert.Equal(2, (int)TransactionType.Transfer);
    }

    [Fact]
    public void TransactionType_ShouldHaveAllExpectedMembers()
    {
        // Arrange
        var expectedValues = new[]
        {
            TransactionType.Income,
            TransactionType.Expense,
            TransactionType.Transfer
        };

        // Act
        var actualValues = Enum.GetValues<TransactionType>();

        // Assert
        Assert.Equal(expectedValues.Length, actualValues.Length);
        foreach (var expectedValue in expectedValues)
        {
            Assert.Contains(expectedValue, actualValues);
        }
    }

    [Fact]
    public void AccountType_ShouldHaveExpectedValues()
    {
        // Assert
        Assert.Equal(0, (int)AccountType.Checking);
        Assert.Equal(1, (int)AccountType.Savings);
        Assert.Equal(2, (int)AccountType.CreditCard);
        Assert.Equal(3, (int)AccountType.Investment);
        Assert.Equal(4, (int)AccountType.Loan);
        Assert.Equal(5, (int)AccountType.Cash);
        Assert.Equal(6, (int)AccountType.Other);
    }

    [Fact]
    public void AccountType_ShouldHaveAllExpectedMembers()
    {
        // Arrange
        var expectedValues = new[]
        {
            AccountType.Checking,
            AccountType.Savings,
            AccountType.CreditCard,
            AccountType.Investment,
            AccountType.Loan,
            AccountType.Cash,
            AccountType.Other
        };

        // Act
        var actualValues = Enum.GetValues<AccountType>();

        // Assert
        Assert.Equal(expectedValues.Length, actualValues.Length);
        foreach (var expectedValue in expectedValues)
        {
            Assert.Contains(expectedValue, actualValues);
        }
    }

    [Theory]
    [InlineData(TransactionType.Income, "Income")]
    [InlineData(TransactionType.Expense, "Expense")]
    [InlineData(TransactionType.Transfer, "Transfer")]
    public void TransactionType_ToString_ShouldReturnCorrectName(TransactionType type, string expectedName)
    {
        // Act
        var result = type.ToString();

        // Assert
        Assert.Equal(expectedName, result);
    }

    [Theory]
    [InlineData(AccountType.Checking, "Checking")]
    [InlineData(AccountType.Savings, "Savings")]
    [InlineData(AccountType.CreditCard, "CreditCard")]
    [InlineData(AccountType.Investment, "Investment")]
    [InlineData(AccountType.Loan, "Loan")]
    [InlineData(AccountType.Cash, "Cash")]
    [InlineData(AccountType.Other, "Other")]
    public void AccountType_ToString_ShouldReturnCorrectName(AccountType type, string expectedName)
    {
        // Act
        var result = type.ToString();

        // Assert
        Assert.Equal(expectedName, result);
    }
}