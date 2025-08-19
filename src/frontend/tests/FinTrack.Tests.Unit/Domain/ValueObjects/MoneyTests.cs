using FinTrack.Core.Entities.ValueObjects;
using Xunit;

namespace FinTrack.Tests.Unit.Domain.ValueObjects;

/// <summary>
/// Unit tests for Money value object
/// </summary>
public class MoneyTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateMoney()
    {
        // Act
        var money = new Money(100.50m, "USD");

        // Assert
        Assert.Equal(100.50m, money.Amount);
        Assert.Equal("USD", money.Currency);
    }

    [Fact]
    public void Constructor_WithLowercaseCurrency_ShouldConvertToUppercase()
    {
        // Act
        var money = new Money(50.00m, "eur");

        // Assert
        Assert.Equal("EUR", money.Currency);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidCurrency_ShouldThrowException(string currency)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Money(100m, currency));
    }

    [Theory]
    [InlineData("US")]
    [InlineData("USDD")]
    public void Constructor_WithInvalidCurrencyLength_ShouldThrowException(string currency)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Money(100m, currency));
    }

    [Fact]
    public void Zero_ShouldCreateZeroMoney()
    {
        // Act
        var money = Money.Zero("USD");

        // Assert
        Assert.Equal(0m, money.Amount);
        Assert.Equal("USD", money.Currency);
    }

    [Fact]
    public void Addition_WithSameCurrency_ShouldAddAmounts()
    {
        // Arrange
        var money1 = new Money(100m, "USD");
        var money2 = new Money(50m, "USD");

        // Act
        var result = money1 + money2;

        // Assert
        Assert.Equal(150m, result.Amount);
        Assert.Equal("USD", result.Currency);
    }

    [Fact]
    public void Addition_WithDifferentCurrency_ShouldThrowException()
    {
        // Arrange
        var money1 = new Money(100m, "USD");
        var money2 = new Money(50m, "EUR");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => money1 + money2);
    }

    [Fact]
    public void Subtraction_WithSameCurrency_ShouldSubtractAmounts()
    {
        // Arrange
        var money1 = new Money(100m, "USD");
        var money2 = new Money(30m, "USD");

        // Act
        var result = money1 - money2;

        // Assert
        Assert.Equal(70m, result.Amount);
        Assert.Equal("USD", result.Currency);
    }

    [Fact]
    public void Subtraction_WithDifferentCurrency_ShouldThrowException()
    {
        // Arrange
        var money1 = new Money(100m, "USD");
        var money2 = new Money(50m, "EUR");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => money1 - money2);
    }

    [Fact]
    public void Multiplication_ShouldMultiplyAmount()
    {
        // Arrange
        var money = new Money(100m, "USD");

        // Act
        var result = money * 2.5m;

        // Assert
        Assert.Equal(250m, result.Amount);
        Assert.Equal("USD", result.Currency);
    }

    [Fact]
    public void Division_ShouldDivideAmount()
    {
        // Arrange
        var money = new Money(100m, "USD");

        // Act
        var result = money / 4m;

        // Assert
        Assert.Equal(25m, result.Amount);
        Assert.Equal("USD", result.Currency);
    }

    [Fact]
    public void Division_ByZero_ShouldThrowException()
    {
        // Arrange
        var money = new Money(100m, "USD");

        // Act & Assert
        Assert.Throws<DivideByZeroException>(() => money / 0m);
    }

    [Theory]
    [InlineData(100.50, true, false, false)]
    [InlineData(-50.25, false, true, false)]
    [InlineData(0, false, false, true)]
    public void Properties_ShouldReturnCorrectValues(decimal amount, bool isPositive, bool isNegative, bool isZero)
    {
        // Arrange
        var money = new Money(amount, "USD");

        // Assert
        Assert.Equal(isPositive, money.IsPositive);
        Assert.Equal(isNegative, money.IsNegative);
        Assert.Equal(isZero, money.IsZero);
    }

    [Fact]
    public void Abs_ShouldReturnAbsoluteValue()
    {
        // Arrange
        var money = new Money(-100.50m, "USD");

        // Act
        var result = money.Abs();

        // Assert
        Assert.Equal(100.50m, result.Amount);
        Assert.Equal("USD", result.Currency);
    }

    [Fact]
    public void Negate_ShouldReturnNegatedValue()
    {
        // Arrange
        var money = new Money(100.50m, "USD");

        // Act
        var result = money.Negate();

        // Assert
        Assert.Equal(-100.50m, result.Amount);
        Assert.Equal("USD", result.Currency);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var money = new Money(100.50m, "USD");

        // Act
        var result = money.ToString();

        // Assert
        Assert.Equal("100.50 USD", result);
    }

    [Fact]
    public void Constructor_WithNullCurrency_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Money(100m, null!));
    }
}