namespace FinTrack.Core.Enums;

/// <summary>
/// Represents the type of financial account
/// </summary>
public enum AccountType
{
    /// <summary>
    /// Checking account for daily transactions
    /// </summary>
    Checking = 0,
    
    /// <summary>
    /// Savings account for storing money
    /// </summary>
    Savings = 1,
    
    /// <summary>
    /// Credit card account (liability)
    /// </summary>
    CreditCard = 2,
    
    /// <summary>
    /// Investment account
    /// </summary>
    Investment = 3,
    
    /// <summary>
    /// Loan account (liability)
    /// </summary>
    Loan = 4,
    
    /// <summary>
    /// Cash account for physical money
    /// </summary>
    Cash = 5,
    
    /// <summary>
    /// Other type of account
    /// </summary>
    Other = 6
}