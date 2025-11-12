namespace FinTrack.Core.Exceptions;

/// <summary>
/// Exception thrown when an entity is not found in the repository
/// </summary>
public class EntityNotFoundException : Exception
{
    /// <summary>
    /// The type of entity that was not found
    /// </summary>
    public string EntityType { get; }
    
    /// <summary>
    /// The ID of the entity that was not found
    /// </summary>
    public object EntityId { get; }
    
    /// <summary>
    /// Initializes a new instance of the EntityNotFoundException class
    /// </summary>
    /// <param name="entityType">The type of entity that was not found</param>
    /// <param name="entityId">The ID of the entity that was not found</param>
    public EntityNotFoundException(string entityType, object entityId)
        : base($"{entityType} with ID '{entityId}' was not found.")
    {
        EntityType = entityType;
        EntityId = entityId;
    }
    
    /// <summary>
    /// Initializes a new instance of the EntityNotFoundException class with a custom message
    /// </summary>
    /// <param name="entityType">The type of entity that was not found</param>
    /// <param name="entityId">The ID of the entity that was not found</param>
    /// <param name="message">Custom error message</param>
    public EntityNotFoundException(string entityType, object entityId, string message)
        : base(message)
    {
        EntityType = entityType;
        EntityId = entityId;
    }
    
    /// <summary>
    /// Initializes a new instance of the EntityNotFoundException class with a custom message and inner exception
    /// </summary>
    /// <param name="entityType">The type of entity that was not found</param>
    /// <param name="entityId">The ID of the entity that was not found</param>
    /// <param name="message">Custom error message</param>
    /// <param name="innerException">Inner exception</param>
    public EntityNotFoundException(string entityType, object entityId, string message, Exception innerException)
        : base(message, innerException)
    {
        EntityType = entityType;
        EntityId = entityId;
    }
}