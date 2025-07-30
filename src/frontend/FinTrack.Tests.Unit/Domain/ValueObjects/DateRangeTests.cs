using FinTrack.Core.Entities.ValueObjects;
using Xunit;

namespace FinTrack.Tests.Unit.Domain.ValueObjects;

/// <summary>
/// Unit tests for DateRange value object
/// </summary>
public class DateRangeTests
{
    [Fact]
    public void Constructor_WithValidDates_ShouldCreateDateRange()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 1, 31);

        // Act
        var dateRange = new DateRange(startDate, endDate);

        // Assert
        Assert.Equal(startDate.Date, dateRange.StartDate);
        Assert.Equal(endDate.Date, dateRange.EndDate);
    }

    [Fact]
    public void Constructor_WithStartDateAfterEndDate_ShouldThrowException()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 31);
        var endDate = new DateTime(2024, 1, 1);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new DateRange(startDate, endDate));
    }

    [Fact]
    public void Constructor_ShouldNormalizeDatesToDateOnly()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 1, 15, 30, 45);
        var endDate = new DateTime(2024, 1, 31, 23, 59, 59);

        // Act
        var dateRange = new DateRange(startDate, endDate);

        // Assert
        Assert.Equal(new DateTime(2024, 1, 1), dateRange.StartDate);
        Assert.Equal(new DateTime(2024, 1, 31), dateRange.EndDate);
    }

    [Fact]
    public void CurrentMonth_ShouldReturnCurrentMonthRange()
    {
        // Arrange
        var now = DateTime.Now;
        var expectedStart = new DateTime(now.Year, now.Month, 1);
        var expectedEnd = expectedStart.AddMonths(1).AddDays(-1);

        // Act
        var dateRange = DateRange.CurrentMonth();

        // Assert
        Assert.Equal(expectedStart, dateRange.StartDate);
        Assert.Equal(expectedEnd, dateRange.EndDate);
    }

    [Fact]
    public void CurrentYear_ShouldReturnCurrentYearRange()
    {
        // Arrange
        var now = DateTime.Now;
        var expectedStart = new DateTime(now.Year, 1, 1);
        var expectedEnd = new DateTime(now.Year, 12, 31);

        // Act
        var dateRange = DateRange.CurrentYear();

        // Assert
        Assert.Equal(expectedStart, dateRange.StartDate);
        Assert.Equal(expectedEnd, dateRange.EndDate);
    }

    [Fact]
    public void LastDays_WithValidDays_ShouldReturnCorrectRange()
    {
        // Arrange
        var days = 7;
        var today = DateTime.Now.Date;
        var expectedStart = today.AddDays(-6); // 7 days including today

        // Act
        var dateRange = DateRange.LastDays(days);

        // Assert
        Assert.Equal(expectedStart, dateRange.StartDate);
        Assert.Equal(today, dateRange.EndDate);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void LastDays_WithInvalidDays_ShouldThrowException(int days)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => DateRange.LastDays(days));
    }

    [Fact]
    public void ForMonth_ShouldReturnCorrectMonthRange()
    {
        // Arrange
        var year = 2024;
        var month = 2; // February
        var expectedStart = new DateTime(2024, 2, 1);
        var expectedEnd = new DateTime(2024, 2, 29); // 2024 is a leap year

        // Act
        var dateRange = DateRange.ForMonth(year, month);

        // Assert
        Assert.Equal(expectedStart, dateRange.StartDate);
        Assert.Equal(expectedEnd, dateRange.EndDate);
    }

    [Theory]
    [InlineData("2024-01-15", true)]
    [InlineData("2024-01-01", true)]
    [InlineData("2024-01-31", true)]
    [InlineData("2023-12-31", false)]
    [InlineData("2024-02-01", false)]
    public void Contains_ShouldReturnCorrectResult(string dateString, bool expected)
    {
        // Arrange
        var dateRange = new DateRange(new DateTime(2024, 1, 1), new DateTime(2024, 1, 31));
        var testDate = DateTime.Parse(dateString);

        // Act
        var result = dateRange.Contains(testDate);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void DayCount_ShouldReturnCorrectCount()
    {
        // Arrange
        var dateRange = new DateRange(new DateTime(2024, 1, 1), new DateTime(2024, 1, 31));

        // Act
        var dayCount = dateRange.DayCount;

        // Assert
        Assert.Equal(31, dayCount);
    }

    [Theory]
    [InlineData("2024-01-01", "2024-01-31", "2024-01-15", "2024-02-15", true)]
    [InlineData("2024-01-01", "2024-01-31", "2024-02-01", "2024-02-28", false)]
    [InlineData("2024-01-01", "2024-01-31", "2023-12-15", "2024-01-15", true)]
    public void OverlapsWith_ShouldReturnCorrectResult(string start1, string end1, string start2, string end2, bool expected)
    {
        // Arrange
        var range1 = new DateRange(DateTime.Parse(start1), DateTime.Parse(end1));
        var range2 = new DateRange(DateTime.Parse(start2), DateTime.Parse(end2));

        // Act
        var result = range1.OverlapsWith(range2);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var dateRange = new DateRange(new DateTime(2024, 1, 1), new DateTime(2024, 1, 31));

        // Act
        var result = dateRange.ToString();

        // Assert
        Assert.Equal("2024-01-01 to 2024-01-31", result);
    }
}