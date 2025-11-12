using FluentValidation.Results;
using FinTrack.Core.Exceptions;

namespace FinTrack.Shared.Services;

/// <summary>
/// Service for handling validation errors and converting them to user-friendly messages
/// </summary>
public interface IValidationErrorService
{
    string GetUserFriendlyMessage(FluentValidation.Results.ValidationResult validationResult);
    string GetUserFriendlyMessage(BusinessRuleException businessRuleException);
    string GetUserFriendlyMessage(Exception exception);
    Dictionary<string, List<string>> GetErrorsByProperty(FluentValidation.Results.ValidationResult validationResult);
    bool HasErrors(FluentValidation.Results.ValidationResult validationResult);
    string GetFirstError(FluentValidation.Results.ValidationResult validationResult);
}

/// <summary>
/// Implementation of validation error service
/// </summary>
public class ValidationErrorService : IValidationErrorService
{
    public string GetUserFriendlyMessage(FluentValidation.Results.ValidationResult validationResult)
    {
        if (validationResult.IsValid)
            return string.Empty;

        if (validationResult.Errors.Count == 1)
        {
            return validationResult.Errors[0].ErrorMessage;
        }

        return $"Please fix the following issues:\n• {string.Join("\n• ", validationResult.Errors.Select(e => e.ErrorMessage))}";
    }

    public string GetUserFriendlyMessage(BusinessRuleException businessRuleException)
    {
        return businessRuleException.Message;
    }

    public string GetUserFriendlyMessage(Exception exception)
    {
        return exception switch
        {
            BusinessRuleException bre => GetUserFriendlyMessage(bre),
            FluentValidation.ValidationException ve => GetUserFriendlyMessage(new FluentValidation.Results.ValidationResult(ve.Errors)),
            ArgumentException ae => $"Invalid input: {ae.Message}",
            InvalidOperationException ioe => $"Operation not allowed: {ioe.Message}",
            UnauthorizedAccessException => "You don't have permission to perform this action.",
            TimeoutException => "The operation timed out. Please try again.",
            _ => "An unexpected error occurred. Please try again or contact support if the problem persists."
        };
    }

    public Dictionary<string, List<string>> GetErrorsByProperty(FluentValidation.Results.ValidationResult validationResult)
    {
        var errorsByProperty = new Dictionary<string, List<string>>();

        foreach (var error in validationResult.Errors)
        {
            var propertyName = string.IsNullOrEmpty(error.PropertyName) ? "General" : error.PropertyName;
            
            if (!errorsByProperty.ContainsKey(propertyName))
            {
                errorsByProperty[propertyName] = new List<string>();
            }
            
            errorsByProperty[propertyName].Add(error.ErrorMessage);
        }

        return errorsByProperty;
    }

    public bool HasErrors(FluentValidation.Results.ValidationResult validationResult)
    {
        return !validationResult.IsValid;
    }

    public string GetFirstError(FluentValidation.Results.ValidationResult validationResult)
    {
        return validationResult.Errors.FirstOrDefault()?.ErrorMessage ?? string.Empty;
    }
}

/// <summary>
/// Validation result wrapper with user-friendly error handling
/// </summary>
public class ValidationResultWrapper
{
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public Dictionary<string, List<string>> ErrorsByProperty { get; set; } = new();
    public FluentValidation.Results.ValidationResult? OriginalResult { get; set; }

    public static ValidationResultWrapper Success()
    {
        return new ValidationResultWrapper { IsValid = true };
    }

    public static ValidationResultWrapper FromValidationResult(FluentValidation.Results.ValidationResult result, IValidationErrorService errorService)
    {
        return new ValidationResultWrapper
        {
            IsValid = result.IsValid,
            ErrorMessage = errorService.GetUserFriendlyMessage(result),
            ErrorsByProperty = errorService.GetErrorsByProperty(result),
            OriginalResult = result
        };
    }

    public static ValidationResultWrapper FromException(Exception exception, IValidationErrorService errorService)
    {
        return new ValidationResultWrapper
        {
            IsValid = false,
            ErrorMessage = errorService.GetUserFriendlyMessage(exception),
            ErrorsByProperty = new Dictionary<string, List<string>>
            {
                ["General"] = new List<string> { errorService.GetUserFriendlyMessage(exception) }
            }
        };
    }
}

