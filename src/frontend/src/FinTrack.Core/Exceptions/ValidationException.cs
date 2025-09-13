namespace FinTrack.Core.Exceptions;

/// <summary>
/// Exception thrown when domain entity validation fails
/// </summary>
public class ValidationException : DomainException
{
    /// <summary>
    /// Dictionary of validation errors by property name
    /// </summary>
    public Dictionary<string, string[]> Errors { get; }
    
    /// <summary>
    /// Constructor for ValidationException with multiple errors
    /// </summary>
    /// <param name="errors">Dictionary of validation errors by property name</param>
    public ValidationException(Dictionary<string, string[]> errors)
        : base("VALIDATION_ERROR", "One or more validation errors occurred")
    {
        Errors = errors ?? new Dictionary<string, string[]>();
    }
    
    /// <summary>
    /// Constructor for ValidationException with a single error
    /// </summary>
    /// <param name="propertyName">Name of the property that failed validation</param>
    /// <param name="errorMessage">Validation error message</param>
    public ValidationException(string propertyName, string errorMessage)
        : base("VALIDATION_ERROR", $"Validation failed for {propertyName}: {errorMessage}")
    {
        Errors = new Dictionary<string, string[]>
        {
            { propertyName, new[] { errorMessage } }
        };
    }
    
    /// <summary>
    /// Constructor for ValidationException with a general message
    /// </summary>
    /// <param name="message">General validation error message</param>
    public ValidationException(string message)
        : base("VALIDATION_ERROR", message)
    {
        Errors = new Dictionary<string, string[]>();
    }
    
    /// <summary>
    /// Gets all error messages as a flat list
    /// </summary>
    public IEnumerable<string> GetAllErrorMessages()
    {
        return Errors.SelectMany(kvp => kvp.Value);
    }
    
    /// <summary>
    /// Gets a formatted string of all validation errors
    /// </summary>
    public string GetFormattedErrors()
    {
        if (!Errors.Any())
            return Message;
            
        var errorMessages = Errors.SelectMany(kvp => 
            kvp.Value.Select(error => $"{kvp.Key}: {error}"));
            
        return string.Join(Environment.NewLine, errorMessages);
    }
    
    /// <summary>
    /// Checks if there are validation errors for a specific property
    /// </summary>
    /// <param name="propertyName">Property name to check</param>
    /// <returns>True if there are errors for the property</returns>
    public bool HasErrorsFor(string propertyName)
    {
        return Errors.ContainsKey(propertyName) && Errors[propertyName].Any();
    }
    
    /// <summary>
    /// Gets validation errors for a specific property
    /// </summary>
    /// <param name="propertyName">Property name</param>
    /// <returns>Array of error messages for the property</returns>
    public string[] GetErrorsFor(string propertyName)
    {
        return Errors.TryGetValue(propertyName, out var errors) ? errors : Array.Empty<string>();
    }
}