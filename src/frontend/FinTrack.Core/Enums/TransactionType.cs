namespace FinTrack.Core.Enums;

/// <summary>
/// Represents the type of financial transaction
/// </summary>
public enum TransactionType
{
    /// <summary>
    /// Money coming into an account (positive amount)
    /// </summary>
    Income = 0,
    
    /// <summary>
    /// Money going out of an account (negative amount)
    /// </summary>
    Expense = 1,
    
    /// <summary>
    /// Money moved between accounts
    /// </summary>
    Transfer = 2
}