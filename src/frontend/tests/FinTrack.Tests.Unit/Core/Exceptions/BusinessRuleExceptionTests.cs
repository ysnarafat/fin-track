using FinTrack.Core.Exceptions;

namespace FinTrack.Tests.Unit.Core.Exceptions;

/// <summary>
/// Unit tests for BusinessRuleException class
/// </summary>
public class BusinessRuleExceptionTests
{
    [Fact]
    public void Constructor_ShouldInitializePropertiesCorrectly()
    {
        // Arrange
        var ruleName = "TestRule";
        var message = "Test rule violation";
        var innerException = new Exception("Inner exception");

        // Act
        var exception = new BusinessRuleException(ruleName, message, innerException);

        // Assert
        Assert.Equal(ruleName, exception.RuleName);
        Assert.Equal(message, exception.Message);
        Assert.Equal("BUSINESS_RULE_VIOLATION", exception.ErrorCode);
        Assert.Equal(innerException, exception.InnerException);
        Assert.True(exception.Context.ContainsKey("RuleName"));
        Assert.Equal(ruleName, exception.Context["RuleName"]);
    }

    [Fact]
    public void Constructor_WithoutInnerException_ShouldInitializeCorrectly()
    {
        // Arrange
        var ruleName = "TestRule";
        var message = "Test rule violation";

        // Act
        var exception = new BusinessRuleException(ruleName, message);

        // Assert
        Assert.Equal(ruleName, exception.RuleName);
        Assert.Equal(message, exception.Message);
        Assert.Equal("BUSINESS_RULE_VIOLATION", exception.ErrorCode);
        Assert.Null(exception.InnerException);
    }

    [Fact]
    public void InsufficientFunds_ShouldCreateCorrectException()
    {
        // Arrange
        var accountName = "Checking Account";
        var availableBalance = 500.00m;
        var requestedAmount = 750.00m;

        // Act
        var exception = BusinessRuleException.InsufficientFunds(accountName, availableBalance, requestedAmount);

        // Assert
        Assert.Equal("InsufficientFunds", exception.RuleName);
        Assert.Contains(accountName, exception.Message);
        Assert.Contains("$500.00", exception.Message);
        Assert.Contains("$750.00", exception.Message);
        Assert.Equal(accountName, exception.Context["AccountName"]);
        Assert.Equal(availableBalance, exception.Context["AvailableBalance"]);
        Assert.Equal(requestedAmount, exception.Context["RequestedAmount"]);
    }

    [Fact]
    public void InvalidTransfer_ShouldCreateCorrectException()
    {
        // Arrange
        var reason = "Cannot transfer to the same account";

        // Act
        var exception = BusinessRuleException.InvalidTransfer(reason);

        // Assert
        Assert.Equal("InvalidTransfer", exception.RuleName);
        Assert.Contains(reason, exception.Message);
        Assert.Equal("BUSINESS_RULE_VIOLATION", exception.ErrorCode);
    }

    [Fact]
    public void BudgetExceeded_ShouldCreateCorrectException()
    {
        // Arrange
        var budgetName = "Groceries";
        var budgetLimit = 400.00m;
        var currentSpending = 450.00m;

        // Act
        var exception = BusinessRuleException.BudgetExceeded(budgetName, budgetLimit, currentSpending);

        // Assert
        Assert.Equal("BudgetExceeded", exception.RuleName);
        Assert.Contains(budgetName, exception.Message);
        Assert.Contains("$400.00", exception.Message);
        Assert.Contains("$450.00", exception.Message);
        Assert.Equal(budgetName, exception.Context["BudgetName"]);
        Assert.Equal(budgetLimit, exception.Context["BudgetLimit"]);
        Assert.Equal(currentSpending, exception.Context["CurrentSpending"]);
    }

    [Fact]
    public void DuplicateEntity_ShouldCreateCorrectException()
    {
        // Arrange
        var entityType = "Account";
        var identifier = "ACC-001";

        // Act
        var exception = BusinessRuleException.DuplicateEntity(entityType, identifier);

        // Assert
        Assert.Equal("DuplicateEntity", exception.RuleName);
        Assert.Contains(entityType, exception.Message);
        Assert.Contains(identifier, exception.Message);
        Assert.Equal(entityType, exception.Context["EntityType"]);
        Assert.Equal(identifier, exception.Context["Identifier"]);
    }

    [Fact]
    public void InvalidDateRange_ShouldCreateCorrectException()
    {
        // Arrange
        var startDate = new DateTime(2024, 2, 1);
        var endDate = new DateTime(2024, 1, 1);

        // Act
        var exception = BusinessRuleException.InvalidDateRange(startDate, endDate);

        // Assert
        Assert.Equal("InvalidDateRange", exception.RuleName);
        Assert.Contains("2024-02-01", exception.Message);
        Assert.Contains("2024-01-01", exception.Message);
        Assert.Equal(startDate, exception.Context["StartDate"]);
        Assert.Equal(endDate, exception.Context["EndDate"]);
    }

    [Fact]
    public void InactiveEntityOperation_ShouldCreateCorrectException()
    {
        // Arrange
        var entityType = "Account";
        var entityName = "Old Savings";
        var operation = "create transaction";

        // Act
        var exception = BusinessRuleException.InactiveEntityOperation(entityType, entityName, operation);

        // Assert
        Assert.Equal("InactiveEntityOperation", exception.RuleName);
        Assert.Contains(entityType, exception.Message);
        Assert.Contains(entityName, exception.Message);
        Assert.Contains(operation, exception.Message);
        Assert.Equal(entityType, exception.Context["EntityType"]);
        Assert.Equal(entityName, exception.Context["EntityName"]);
        Assert.Equal(operation, exception.Context["Operation"]);
    }

    [Fact]
    public void WithContext_ShouldAddContextData()
    {
        // Arrange
        var exception = new BusinessRuleException("TestRule", "Test message");

        // Act
        var result = exception.WithContext("TestKey", "TestValue");

        // Assert
        Assert.Same(exception, result); // Should return same instance for chaining
        Assert.True(exception.Context.ContainsKey("TestKey"));
        Assert.Equal("TestValue", exception.Context["TestKey"]);
    }

    [Fact]
    public void GetDetailedMessage_WithoutContext_ShouldReturnBasicMessage()
    {
        // Arrange
        var exception = new BusinessRuleException("TestRule", "Test message");
        exception.Context.Clear(); // Remove the default RuleName context

        // Act
        var result = exception.GetDetailedMessage();

        // Assert
        Assert.Equal("[BUSINESS_RULE_VIOLATION] Test message", result);
    }

    [Fact]
    public void GetDetailedMessage_WithContext_ShouldIncludeContextData()
    {
        // Arrange
        var exception = new BusinessRuleException("TestRule", "Test message");
        exception.WithContext("Key1", "Value1");
        exception.WithContext("Key2", 42);

        // Act
        var result = exception.GetDetailedMessage();

        // Assert
        Assert.Contains("[BUSINESS_RULE_VIOLATION] Test message", result);
        Assert.Contains("Context:", result);
        Assert.Contains("RuleName: TestRule", result);
        Assert.Contains("Key1: Value1", result);
        Assert.Contains("Key2: 42", result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithEmptyOrNullRuleName_ShouldStillWork(string ruleName)
    {
        // Arrange & Act
        var exception = new BusinessRuleException(ruleName, "Test message");

        // Assert
        Assert.Equal(ruleName, exception.RuleName);
        Assert.Equal("BUSINESS_RULE_VIOLATION", exception.ErrorCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyMessage_ShouldStillWork(string message)
    {
        // Arrange & Act
        var exception = new BusinessRuleException("TestRule", message);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Equal("TestRule", exception.RuleName);
    }

    [Fact]
    public void Constructor_WithNullMessage_ShouldUseDefaultMessage()
    {
        // Arrange & Act
        var exception = new BusinessRuleException("TestRule", null);

        // Assert
        Assert.NotNull(exception.Message); // .NET generates a default message for null
        Assert.NotEmpty(exception.Message);
        Assert.Equal("TestRule", exception.RuleName);
    }
}