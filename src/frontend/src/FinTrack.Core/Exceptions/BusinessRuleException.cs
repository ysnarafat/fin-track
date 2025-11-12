namespace FinTrack.Core.Exceptions;

/// <summary>
/// Exception thrown when a business rule is violated
/// </summary>
public class BusinessRuleException : DomainException
{
    /// <summary>
    /// The business rule that was violated
    /// </summary>
    public string RuleName { get; }
    
    /// <summary>
    /// Constructor for BusinessRuleException
    /// </summary>
    /// <param name="ruleName">Name of the business rule that was violated</param>
    /// <param name="message">Description of the rule violation</param>
    /// <param name="innerException">Inner exception</param>
    public BusinessRuleException(string ruleName, string message, Exception? innerException = null)
        : base("BUSINESS_RULE_VIOLATION", message, innerException)
    {
        RuleName = ruleName;
        WithContext("RuleName", ruleName);
    }
    
    /// <summary>
    /// Creates a BusinessRuleException for insufficient funds
    /// </summary>
    /// <param name="accountName">Name of the account</param>
    /// <param name="availableBalance">Available balance</param>
    /// <param name="requestedAmount">Requested transaction amount</param>
    /// <returns>BusinessRuleException instance</returns>
    public static BusinessRuleException InsufficientFunds(string accountName, decimal availableBalance, decimal requestedAmount)
    {
        var message = $"Insufficient funds in account '{accountName}'. Available: {availableBalance:C}, Requested: {requestedAmount:C}";
        var exception = new BusinessRuleException("InsufficientFunds", message);
        exception.WithContext("AccountName", accountName);
        exception.WithContext("AvailableBalance", availableBalance);
        exception.WithContext("RequestedAmount", requestedAmount);
        return exception;
    }
    
    /// <summary>
    /// Creates a BusinessRuleException for invalid transfer
    /// </summary>
    /// <param name="reason">Reason why the transfer is invalid</param>
    /// <returns>BusinessRuleException instance</returns>
    public static BusinessRuleException InvalidTransfer(string reason)
    {
        return new BusinessRuleException("InvalidTransfer", $"Transfer is not valid: {reason}");
    }
    
    /// <summary>
    /// Creates a BusinessRuleException for budget exceeded
    /// </summary>
    /// <param name="budgetName">Name of the budget</param>
    /// <param name="budgetLimit">Budget limit</param>
    /// <param name="currentSpending">Current spending amount</param>
    /// <returns>BusinessRuleException instance</returns>
    public static BusinessRuleException BudgetExceeded(string budgetName, decimal budgetLimit, decimal currentSpending)
    {
        var message = $"Budget '{budgetName}' would be exceeded. Limit: {budgetLimit:C}, Current: {currentSpending:C}";
        var exception = new BusinessRuleException("BudgetExceeded", message);
        exception.WithContext("BudgetName", budgetName);
        exception.WithContext("BudgetLimit", budgetLimit);
        exception.WithContext("CurrentSpending", currentSpending);
        return exception;
    }
    
    /// <summary>
    /// Creates a BusinessRuleException for duplicate entity
    /// </summary>
    /// <param name="entityType">Type of entity</param>
    /// <param name="identifier">Identifier that is duplicated</param>
    /// <returns>BusinessRuleException instance</returns>
    public static BusinessRuleException DuplicateEntity(string entityType, string identifier)
    {
        var message = $"A {entityType} with identifier '{identifier}' already exists";
        var exception = new BusinessRuleException("DuplicateEntity", message);
        exception.WithContext("EntityType", entityType);
        exception.WithContext("Identifier", identifier);
        return exception;
    }
    
    /// <summary>
    /// Creates a BusinessRuleException for invalid date range
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>BusinessRuleException instance</returns>
    public static BusinessRuleException InvalidDateRange(DateTime startDate, DateTime endDate)
    {
        var message = $"Invalid date range: start date ({startDate:yyyy-MM-dd}) must be before end date ({endDate:yyyy-MM-dd})";
        var exception = new BusinessRuleException("InvalidDateRange", message);
        exception.WithContext("StartDate", startDate);
        exception.WithContext("EndDate", endDate);
        return exception;
    }
    
    /// <summary>
    /// Creates a BusinessRuleException for inactive entity operation
    /// </summary>
    /// <param name="entityType">Type of entity</param>
    /// <param name="entityName">Name of the entity</param>
    /// <param name="operation">Operation being attempted</param>
    /// <returns>BusinessRuleException instance</returns>
    public static BusinessRuleException InactiveEntityOperation(string entityType, string entityName, string operation)
    {
        var message = $"Cannot {operation} on inactive {entityType} '{entityName}'";
        var exception = new BusinessRuleException("InactiveEntityOperation", message);
        exception.WithContext("EntityType", entityType);
        exception.WithContext("EntityName", entityName);
        exception.WithContext("Operation", operation);
        return exception;
    }
}