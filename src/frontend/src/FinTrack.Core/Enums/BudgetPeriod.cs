namespace FinTrack.Core.Enums;

/// <summary>
/// Represents the period type for a budget
/// </summary>
public enum BudgetPeriod
{
    /// <summary>
    /// Weekly budget period
    /// </summary>
    Weekly = 1,
    
    /// <summary>
    /// Monthly budget period
    /// </summary>
    Monthly = 2,
    
    /// <summary>
    /// Quarterly budget period (3 months)
    /// </summary>
    Quarterly = 3,
    
    /// <summary>
    /// Annual budget period (12 months)
    /// </summary>
    Annual = 4,
    
    /// <summary>
    /// Custom budget period with user-defined dates
    /// </summary>
    Custom = 5
}