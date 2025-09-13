namespace FinTrack.Core.Exceptions;

/// <summary>
/// Base exception class for domain-specific errors
/// </summary>
public abstract class DomainException : Exception
{
    /// <summary>
    /// Error code for categorizing the exception
    /// </summary>
    public string ErrorCode { get; }
    
    /// <summary>
    /// Additional context data for the exception
    /// </summary>
    public Dictionary<string, object> Context { get; }
    
    /// <summary>
    /// Constructor for DomainException
    /// </summary>
    /// <param name="errorCode">Error code for categorizing the exception</param>
    /// <param name="message">Exception message</param>
    /// <param name="innerException">Inner exception</param>
    protected DomainException(string errorCode, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        Context = new Dictionary<string, object>();
    }
    
    /// <summary>
    /// Adds context data to the exception
    /// </summary>
    /// <param name="key">Context key</param>
    /// <param name="value">Context value</param>
    /// <returns>The exception instance for method chaining</returns>
    public DomainException WithContext(string key, object value)
    {
        Context[key] = value;
        return this;
    }
    
    /// <summary>
    /// Gets a formatted error message including context
    /// </summary>
    public string GetDetailedMessage()
    {
        var message = $"[{ErrorCode}] {Message}";
        
        if (Context.Any())
        {
            var contextString = string.Join(", ", Context.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
            message += $" | Context: {contextString}";
        }
        
        return message;
    }
}