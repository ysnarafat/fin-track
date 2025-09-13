namespace FinTrack.Core.Enums;

/// <summary>
/// Represents the type of financial goal
/// </summary>
public enum GoalType
{
    /// <summary>
    /// Savings goal - accumulating money for a specific purpose
    /// </summary>
    Savings = 1,
    
    /// <summary>
    /// Debt payoff goal - paying down existing debt
    /// </summary>
    DebtPayoff = 2,
    
    /// <summary>
    /// Investment goal - building investment portfolio
    /// </summary>
    Investment = 3,
    
    /// <summary>
    /// Emergency fund goal - building emergency savings
    /// </summary>
    EmergencyFund = 4,
    
    /// <summary>
    /// Retirement goal - saving for retirement
    /// </summary>
    Retirement = 5,
    
    /// <summary>
    /// Education goal - saving for education expenses
    /// </summary>
    Education = 6,
    
    /// <summary>
    /// Home purchase goal - saving for home down payment
    /// </summary>
    HomePurchase = 7,
    
    /// <summary>
    /// Vacation goal - saving for travel and vacation
    /// </summary>
    Vacation = 8,
    
    /// <summary>
    /// Vehicle goal - saving for vehicle purchase
    /// </summary>
    Vehicle = 9,
    
    /// <summary>
    /// Other custom goal type
    /// </summary>
    Other = 10
}