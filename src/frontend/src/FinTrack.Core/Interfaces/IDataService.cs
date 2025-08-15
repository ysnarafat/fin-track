using FinTrack.Core.Entities;

namespace FinTrack.Core.Interfaces;

/// <summary>
/// Generic data service interface for high-level data operations
/// </summary>
/// <typeparam name="T">Entity type that inherits from BaseEntity</typeparam>
public interface IDataService<T> where T : BaseEntity
{
    /// <summary>
    /// Gets an entity by its ID with related data
    /// </summary>
    /// <param name="id">Entity ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Entity with related data if found, null otherwise</returns>
    Task<T?> GetByIdWithRelatedAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all entities with related data
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of entities with related data</returns>
    Task<IEnumerable<T>> GetAllWithRelatedAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Creates a new entity with validation
    /// </summary>
    /// <param name="entity">Entity to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created entity</returns>
    Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates an entity with validation
    /// </summary>
    /// <param name="entity">Entity to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated entity</returns>
    Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes an entity by ID
    /// </summary>
    /// <param name="id">Entity ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted, false if not found</returns>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Validates an entity
    /// </summary>
    /// <param name="entity">Entity to validate</param>
    /// <returns>Validation result with errors if any</returns>
    Task<ValidationResult> ValidateAsync(T entity);
    
    /// <summary>
    /// Checks if an entity exists
    /// </summary>
    /// <param name="id">Entity ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if entity exists, false otherwise</returns>
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents the result of a validation operation
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Indicates if the validation was successful
    /// </summary>
    public bool IsValid { get; set; }
    
    /// <summary>
    /// Collection of validation error messages
    /// </summary>
    public List<string> Errors { get; set; } = new();
    
    /// <summary>
    /// Collection of validation warnings
    /// </summary>
    public List<string> Warnings { get; set; } = new();
    
    /// <summary>
    /// Creates a successful validation result
    /// </summary>
    public static ValidationResult Success() => new() { IsValid = true };
    
    /// <summary>
    /// Creates a failed validation result with errors
    /// </summary>
    /// <param name="errors">Validation errors</param>
    public static ValidationResult Failure(params string[] errors) => new()
    {
        IsValid = false,
        Errors = errors.ToList()
    };
    
    /// <summary>
    /// Creates a failed validation result with errors and warnings
    /// </summary>
    /// <param name="errors">Validation errors</param>
    /// <param name="warnings">Validation warnings</param>
    public static ValidationResult Failure(IEnumerable<string> errors, IEnumerable<string> warnings) => new()
    {
        IsValid = false,
        Errors = errors.ToList(),
        Warnings = warnings.ToList()
    };
}