namespace FinTrack.Core.Entities.ValueObjects;

/// <summary>
/// Value object representing a monetary amount with currency
/// </summary>
public record Money
{
    /// <summary>
    /// The monetary amount
    /// </summary>
    public decimal Amount { get; init; }
    
    /// <summary>
    /// The currency code (e.g., USD, EUR, GBP)
    /// </summary>
    public string Currency { get; init; }
    
    /// <summary>
    /// Constructor for Money value object
    /// </summary>
    /// <param name="amount">The monetary amount</param>
    /// <param name="currency">The currency code</param>
    public Money(decimal amount, string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be null or empty", nameof(currency));
        
        if (currency.Length != 3)
            throw new ArgumentException("Currency must be a 3-letter code", nameof(currency));
        
        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }
    
    /// <summary>
    /// Creates a Money object with zero amount
    /// </summary>
    /// <param name="currency">The currency code</param>
    /// <returns>A Money object with zero amount</returns>
    public static Money Zero(string currency) => new(0, currency);
    
    /// <summary>
    /// Adds two Money objects (must have same currency)
    /// </summary>
    public static Money operator +(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException("Cannot add money with different currencies");
        
        return new Money(left.Amount + right.Amount, left.Currency);
    }
    
    /// <summary>
    /// Subtracts two Money objects (must have same currency)
    /// </summary>
    public static Money operator -(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException("Cannot subtract money with different currencies");
        
        return new Money(left.Amount - right.Amount, left.Currency);
    }
    
    /// <summary>
    /// Multiplies Money by a decimal factor
    /// </summary>
    public static Money operator *(Money money, decimal factor)
    {
        return new Money(money.Amount * factor, money.Currency);
    }
    
    /// <summary>
    /// Divides Money by a decimal factor
    /// </summary>
    public static Money operator /(Money money, decimal factor)
    {
        if (factor == 0)
            throw new DivideByZeroException("Cannot divide money by zero");
        
        return new Money(money.Amount / factor, money.Currency);
    }
    
    /// <summary>
    /// Checks if the amount is positive
    /// </summary>
    public bool IsPositive => Amount > 0;
    
    /// <summary>
    /// Checks if the amount is negative
    /// </summary>
    public bool IsNegative => Amount < 0;
    
    /// <summary>
    /// Checks if the amount is zero
    /// </summary>
    public bool IsZero => Amount == 0;
    
    /// <summary>
    /// Returns the absolute value of the money
    /// </summary>
    public Money Abs() => new(Math.Abs(Amount), Currency);
    
    /// <summary>
    /// Returns the negated value of the money
    /// </summary>
    public Money Negate() => new(-Amount, Currency);
    
    /// <summary>
    /// String representation of the money
    /// </summary>
    public override string ToString() => $"{Amount:F2} {Currency}";
}