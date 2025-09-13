namespace FinTrack.Shared.Models;

/// <summary>
/// Business validation result
/// </summary>
public class BusinessValidationResult
{
    public bool IsValid { get; set; } = true;
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();

    public void AddError(string error)
    {
        Errors.Add(error);
        IsValid = false;
    }

    public void AddWarning(string warning)
    {
        Warnings.Add(warning);
    }

    public bool HasErrors => Errors.Any();
    public bool HasWarnings => Warnings.Any();
    
    public string GetErrorsAsString() => string.Join(", ", Errors);
    public string GetWarningsAsString() => string.Join(", ", Warnings);
}