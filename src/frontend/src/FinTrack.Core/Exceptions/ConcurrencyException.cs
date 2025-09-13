namespace FinTrack.Core.Exceptions;

/// <summary>
/// Exception thrown when a concurrency conflict occurs during entity updates
/// </summary>
public class ConcurrencyException : Exception
{
    /// <summary>
    /// The type of entity that had the concurrency conflict
    /// </summary>
    public string EntityType { get; }
    
    /// <summary>
    /// The ID of the entity that had the concurrency conflict
    /// </summary>
    public object EntityId { get; }
    
    /// <summary>
    /// The version that was expected
    /// </summary>
    public byte[] ExpectedVersion { get; }
    
    /// <summary>
    /// The actual version in the database
    /// </summary>
    public byte[] ActualVersion { get; }
    
    /// <summary>
    /// Initializes a new instance of the ConcurrencyException class
    /// </summary>
    /// <param name="entityType">The type of entity that had the concurrency conflict</param>
    /// <param name="entityId">The ID of the entity that had the concurrency conflict</param>
    /// <param name="expectedVersion">The version that was expected</param>
    /// <param name="actualVersion">The actual version in the database</param>
    public ConcurrencyException(string entityType, object entityId, byte[] expectedVersion, byte[] actualVersion)
        : base($"Concurrency conflict occurred for {entityType} with ID '{entityId}'. The entity has been modified by another process.")
    {
        EntityType = entityType;
        EntityId = entityId;
        ExpectedVersion = expectedVersion;
        ActualVersion = actualVersion;
    }
    
    /// <summary>
    /// Initializes a new instance of the ConcurrencyException class with a custom message
    /// </summary>
    /// <param name="entityType">The type of entity that had the concurrency conflict</param>
    /// <param name="entityId">The ID of the entity that had the concurrency conflict</param>
    /// <param name="expectedVersion">The version that was expected</param>
    /// <param name="actualVersion">The actual version in the database</param>
    /// <param name="message">Custom error message</param>
    public ConcurrencyException(string entityType, object entityId, byte[] expectedVersion, byte[] actualVersion, string message)
        : base(message)
    {
        EntityType = entityType;
        EntityId = entityId;
        ExpectedVersion = expectedVersion;
        ActualVersion = actualVersion;
    }
    
    /// <summary>
    /// Initializes a new instance of the ConcurrencyException class with a custom message and inner exception
    /// </summary>
    /// <param name="entityType">The type of entity that had the concurrency conflict</param>
    /// <param name="entityId">The ID of the entity that had the concurrency conflict</param>
    /// <param name="expectedVersion">The version that was expected</param>
    /// <param name="actualVersion">The actual version in the database</param>
    /// <param name="message">Custom error message</param>
    /// <param name="innerException">Inner exception</param>
    public ConcurrencyException(string entityType, object entityId, byte[] expectedVersion, byte[] actualVersion, string message, Exception innerException)
        : base(message, innerException)
    {
        EntityType = entityType;
        EntityId = entityId;
        ExpectedVersion = expectedVersion;
        ActualVersion = actualVersion;
    }
}