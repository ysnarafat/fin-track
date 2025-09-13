using FinTrack.Core.Exceptions;

namespace FinTrack.Tests.Unit.Core.Exceptions;

/// <summary>
/// Unit tests for DomainException base class
/// </summary>
public class DomainExceptionTests
{
    // Test implementation of abstract DomainException
    private class TestDomainException : DomainException
    {
        public TestDomainException(string errorCode, string message, Exception? innerException = null)
            : base(errorCode, message, innerException)
        {
        }
    }

    [Fact]
    public void Constructor_ShouldInitializePropertiesCorrectly()
    {
        // Arrange
        var errorCode = "TEST_ERROR";
        var message = "Test error message";
        var innerException = new Exception("Inner exception");

        // Act
        var exception = new TestDomainException(errorCode, message, innerException);

        // Assert
        Assert.Equal(errorCode, exception.ErrorCode);
        Assert.Equal(message, exception.Message);
        Assert.Equal(innerException, exception.InnerException);
        Assert.NotNull(exception.Context);
        Assert.Empty(exception.Context);
    }

    [Fact]
    public void Constructor_WithoutInnerException_ShouldInitializeCorrectly()
    {
        // Arrange
        var errorCode = "TEST_ERROR";
        var message = "Test error message";

        // Act
        var exception = new TestDomainException(errorCode, message);

        // Assert
        Assert.Equal(errorCode, exception.ErrorCode);
        Assert.Equal(message, exception.Message);
        Assert.Null(exception.InnerException);
        Assert.NotNull(exception.Context);
        Assert.Empty(exception.Context);
    }

    [Fact]
    public void WithContext_ShouldAddContextData()
    {
        // Arrange
        var exception = new TestDomainException("TEST_ERROR", "Test message");

        // Act
        var result = exception.WithContext("TestKey", "TestValue");

        // Assert
        Assert.Same(exception, result); // Should return same instance for method chaining
        Assert.True(exception.Context.ContainsKey("TestKey"));
        Assert.Equal("TestValue", exception.Context["TestKey"]);
    }

    [Fact]
    public void WithContext_ShouldAllowMultipleContextEntries()
    {
        // Arrange
        var exception = new TestDomainException("TEST_ERROR", "Test message");

        // Act
        exception.WithContext("Key1", "Value1")
                 .WithContext("Key2", 42)
                 .WithContext("Key3", true);

        // Assert
        Assert.Equal(3, exception.Context.Count);
        Assert.Equal("Value1", exception.Context["Key1"]);
        Assert.Equal(42, exception.Context["Key2"]);
        Assert.Equal(true, exception.Context["Key3"]);
    }

    [Fact]
    public void WithContext_ShouldOverwriteExistingKey()
    {
        // Arrange
        var exception = new TestDomainException("TEST_ERROR", "Test message");
        exception.WithContext("TestKey", "OriginalValue");

        // Act
        exception.WithContext("TestKey", "NewValue");

        // Assert
        Assert.Equal("NewValue", exception.Context["TestKey"]);
        Assert.Single(exception.Context);
    }

    [Fact]
    public void GetDetailedMessage_WithoutContext_ShouldReturnBasicFormat()
    {
        // Arrange
        var exception = new TestDomainException("TEST_ERROR", "Test message");

        // Act
        var result = exception.GetDetailedMessage();

        // Assert
        Assert.Equal("[TEST_ERROR] Test message", result);
    }

    [Fact]
    public void GetDetailedMessage_WithContext_ShouldIncludeContextData()
    {
        // Arrange
        var exception = new TestDomainException("TEST_ERROR", "Test message");
        exception.WithContext("UserId", 123)
                 .WithContext("Action", "CreateAccount")
                 .WithContext("Timestamp", "2024-01-15");

        // Act
        var result = exception.GetDetailedMessage();

        // Assert
        Assert.StartsWith("[TEST_ERROR] Test message", result);
        Assert.Contains("Context:", result);
        Assert.Contains("UserId: 123", result);
        Assert.Contains("Action: CreateAccount", result);
        Assert.Contains("Timestamp: 2024-01-15", result);
    }

    [Fact]
    public void GetDetailedMessage_WithSingleContext_ShouldFormatCorrectly()
    {
        // Arrange
        var exception = new TestDomainException("TEST_ERROR", "Test message");
        exception.WithContext("EntityId", 456);

        // Act
        var result = exception.GetDetailedMessage();

        // Assert
        Assert.Equal("[TEST_ERROR] Test message | Context: EntityId: 456", result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_WithEmptyOrNullErrorCode_ShouldStillWork(string errorCode)
    {
        // Arrange & Act
        var exception = new TestDomainException(errorCode, "Test message");

        // Assert
        Assert.Equal(errorCode, exception.ErrorCode);
        Assert.Equal("Test message", exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyMessage_ShouldStillWork(string message)
    {
        // Arrange & Act
        var exception = new TestDomainException("TEST_ERROR", message);

        // Assert
        Assert.Equal("TEST_ERROR", exception.ErrorCode);
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void Constructor_WithNullMessage_ShouldUseDefaultMessage()
    {
        // Arrange & Act
        var exception = new TestDomainException("TEST_ERROR", null);

        // Assert
        Assert.Equal("TEST_ERROR", exception.ErrorCode);
        Assert.NotNull(exception.Message); // .NET generates a default message for null
        Assert.NotEmpty(exception.Message);
    }

    [Fact]
    public void WithContext_WithNullKey_ShouldThrowArgumentNullException()
    {
        // Arrange
        var exception = new TestDomainException("TEST_ERROR", "Test message");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => exception.WithContext(null!, "value"));
    }

    [Fact]
    public void WithContext_WithNullValue_ShouldStoreNull()
    {
        // Arrange
        var exception = new TestDomainException("TEST_ERROR", "Test message");

        // Act
        exception.WithContext("TestKey", null!);

        // Assert
        Assert.True(exception.Context.ContainsKey("TestKey"));
        Assert.Null(exception.Context["TestKey"]);
    }

    [Fact]
    public void Context_ShouldSupportComplexObjects()
    {
        // Arrange
        var exception = new TestDomainException("TEST_ERROR", "Test message");
        var complexObject = new { Id = 1, Name = "Test", Items = new[] { 1, 2, 3 } };

        // Act
        exception.WithContext("ComplexObject", complexObject);

        // Assert
        Assert.Equal(complexObject, exception.Context["ComplexObject"]);
    }

    [Fact]
    public void GetDetailedMessage_WithComplexContextValues_ShouldHandleToString()
    {
        // Arrange
        var exception = new TestDomainException("TEST_ERROR", "Test message");
        var complexObject = new { Id = 1, Name = "Test" };
        exception.WithContext("Object", complexObject);

        // Act
        var result = exception.GetDetailedMessage();

        // Assert
        Assert.Contains("Object: { Id = 1, Name = Test }", result);
    }
}