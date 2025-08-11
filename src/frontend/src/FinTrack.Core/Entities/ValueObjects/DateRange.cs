namespace FinTrack.Core.Entities.ValueObjects;

/// <summary>
/// Value object representing a date range
/// </summary>
public record DateRange
{
    /// <summary>
    /// The start date of the range (inclusive)
    /// </summary>
    public DateTime StartDate { get; init; }
    
    /// <summary>
    /// The end date of the range (inclusive)
    /// </summary>
    public DateTime EndDate { get; init; }
    
    /// <summary>
    /// Constructor for DateRange value object
    /// </summary>
    /// <param name="startDate">The start date</param>
    /// <param name="endDate">The end date</param>
    public DateRange(DateTime startDate, DateTime endDate)
    {
        if (startDate > endDate)
            throw new ArgumentException("Start date cannot be after end date");
        
        StartDate = startDate.Date; // Normalize to date only
        EndDate = endDate.Date;
    }
    
    /// <summary>
    /// Creates a date range for the current month
    /// </summary>
    public static DateRange CurrentMonth()
    {
        var now = DateTime.Now;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
        return new DateRange(startOfMonth, endOfMonth);
    }
    
    /// <summary>
    /// Creates a date range for the current year
    /// </summary>
    public static DateRange CurrentYear()
    {
        var now = DateTime.Now;
        var startOfYear = new DateTime(now.Year, 1, 1);
        var endOfYear = new DateTime(now.Year, 12, 31);
        return new DateRange(startOfYear, endOfYear);
    }
    
    /// <summary>
    /// Creates a date range for the last N days
    /// </summary>
    public static DateRange LastDays(int days)
    {
        if (days <= 0)
            throw new ArgumentException("Days must be positive", nameof(days));
        
        var endDate = DateTime.Now.Date;
        var startDate = endDate.AddDays(-days + 1);
        return new DateRange(startDate, endDate);
    }
    
    /// <summary>
    /// Creates a date range for a specific month and year
    /// </summary>
    public static DateRange ForMonth(int year, int month)
    {
        var startOfMonth = new DateTime(year, month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
        return new DateRange(startOfMonth, endOfMonth);
    }
    
    /// <summary>
    /// Checks if a date falls within this range
    /// </summary>
    public bool Contains(DateTime date)
    {
        var dateOnly = date.Date;
        return dateOnly >= StartDate && dateOnly <= EndDate;
    }
    
    /// <summary>
    /// Gets the number of days in this range
    /// </summary>
    public int DayCount => (EndDate - StartDate).Days + 1;
    
    /// <summary>
    /// Checks if this range overlaps with another range
    /// </summary>
    public bool OverlapsWith(DateRange other)
    {
        return StartDate <= other.EndDate && EndDate >= other.StartDate;
    }
    
    /// <summary>
    /// String representation of the date range
    /// </summary>
    public override string ToString() => $"{StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}";
}