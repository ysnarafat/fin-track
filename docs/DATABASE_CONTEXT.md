# Database Context Architecture

## FinTrackDbContext Overview

The `FinTrackDbContext` is the central Entity Framework Core database context for the FinTrack application, providing comprehensive data access capabilities with automatic audit field management and synchronization support.

## Recent Enhancements

### Repository-Context Coordination Improvement

The database context now works more seamlessly with repository operations through simplified sync status handling that eliminates redundant logic between layers.

#### BaseRepository Sync Status Simplification

The latest enhancement removes duplicate sync status handling from the `BaseRepository.UpdateAsync()` method, allowing the database context's intelligent audit field management to handle all sync status logic:

- **Eliminated Redundant Logic**: Removed manual sync status preservation code from repository layer
- **Centralized Management**: All sync status handling now occurs in the database context's `UpdateAuditFields()` method
- **Improved Coordination**: Better separation of concerns between repository (data access) and context (audit management)
- **Enhanced Reliability**: Eliminates potential conflicts between repository and context sync status handling

### Enhanced Audit Field Management

The database context has been significantly improved with streamlined audit field handling that provides better coordination between repository operations and automatic database updates.

#### Recent Improvements

The latest enhancement improves the sync status handling logic with more sophisticated change detection:

- **Enhanced Sync Status Detection**: Uses Entity Framework's property modification flags instead of value comparison for more reliable change detection
- **Preserved Entity State Logic**: Maintains PendingCreate status for newly created entities that are subsequently modified
- **Improved Explicit Modification Handling**: Better detection of user-initiated sync status changes vs. automatic updates using `IsModified` property flag
- **Robust State Preservation**: Only updates sync status when it hasn't been explicitly modified by user code

#### Current Implementation

The `UpdateAuditFields()` method now uses a streamlined approach for better reliability:

```csharp
private void UpdateAuditFields()
{
    var entries = ChangeTracker.Entries<BaseEntity>();
    var now = DateTime.UtcNow;

    foreach (var entry in entries)
    {
        switch (entry.State)
        {
            case EntityState.Added:
                // Set CreatedAt and UpdatedAt if they are default values
                if (entry.Entity.CreatedAt == default)
                {
                    entry.Entity.CreatedAt = now;
                }
                // For new entities, always set UpdatedAt to the same time as CreatedAt or later
                if (entry.Entity.UpdatedAt == default || entry.Entity.UpdatedAt < entry.Entity.CreatedAt)
                {
                    entry.Entity.UpdatedAt = entry.Entity.CreatedAt;
                }
                if (string.IsNullOrEmpty(entry.Entity.SyncId))
                {
                    entry.Entity.SyncId = Guid.NewGuid().ToString();
                }
                // Set initial version
                if (entry.Entity.Version == 0)
                {
                    entry.Entity.Version = 1;
                }
                // Only set sync status if it's the default value
                if (entry.Entity.SyncStatus == default(SyncStatus))
                {
                    entry.Entity.SyncStatus = SyncStatus.PendingCreate;
                }
                break;
                
            case EntityState.Modified:
                // Always update UpdatedAt for modifications
                entry.Entity.UpdatedAt = now;
                entry.Entity.Version++;
                
                // Check if SyncStatus was explicitly modified by user code
                var syncStatusProperty = entry.Property(nameof(BaseEntity.SyncStatus));
                var originalSyncStatus = (SyncStatus)entry.OriginalValues[nameof(BaseEntity.SyncStatus)];
                var currentSyncStatus = entry.Entity.SyncStatus;
                
                // If the sync status hasn't been explicitly modified by user code,
                // then we should update it based on business rules
                if (!syncStatusProperty.IsModified)
                {
                    // Change sync status to PendingUpdate if it's currently Synced
                    if (entry.Entity.SyncStatus == SyncStatus.Synced)
                    {
                        entry.Entity.SyncStatus = SyncStatus.PendingUpdate;
                    }
                    // If it's PendingCreate, keep it as PendingCreate (new entity being modified)
                    // If it's other statuses, leave them unchanged
                }
                break;
                
            case EntityState.Deleted:
                // Check if this is a hard delete (marked with HardDelete sync status)
                if (entry.Entity.SyncStatus == SyncStatus.HardDelete)
                {
                    // Allow hard delete to proceed
                    break;
                }
                
                // For soft delete, change to modified and set IsDeleted
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.UpdatedAt = now;
                entry.Entity.Version++;
                entry.Entity.SyncStatus = SyncStatus.PendingDelete;
                break;
        }
    }
}
```

## Key Features

### 1. Timestamp Management for New Entities

For newly created entities, the audit system ensures consistent timestamp behavior:

- **CreatedAt**: Set to the current UTC time when the entity is first added
- **UpdatedAt**: Set to equal `CreatedAt` for new entities (not a later time)

This design decision ensures semantic correctness: a newly created entity that hasn't been modified should have identical creation and update timestamps. The audit logic explicitly enforces this:

```csharp
// For new entities, always set UpdatedAt to the same time as CreatedAt or later
if (entry.Entity.UpdatedAt == default || entry.Entity.UpdatedAt < entry.Entity.CreatedAt)
{
    entry.Entity.UpdatedAt = entry.Entity.CreatedAt;
}
```

**Test Expectation**: When testing new entity creation, tests should verify that `UpdatedAt` equals `CreatedAt`:

```csharp
[Fact]
public async Task SaveChangesAsync_NewEntity_SetsTimestampsEqual()
{
    var account = TestDataBuilder.CreateTestAccount();
    Context.Accounts.Add(account);
    await Context.SaveChangesAsync();
    
    // For new entities, UpdatedAt should equal CreatedAt
    Assert.Equal(account.CreatedAt, account.UpdatedAt);
}
```

### 2. Value-Based Property Management

The context now uses direct value checks for more reliable property management:

```csharp
// Set initial version if not already set
if (entry.Entity.Version == 0)
{
    entry.Entity.Version = 1;
}

// Set timestamps only if they are default values
if (entry.Entity.CreatedAt == default)
{
    entry.Entity.CreatedAt = now;
}

// For new entities, ensure UpdatedAt equals CreatedAt
if (entry.Entity.UpdatedAt == default || entry.Entity.UpdatedAt < entry.Entity.CreatedAt)
{
    entry.Entity.UpdatedAt = entry.Entity.CreatedAt;
}
```

**Important**: For newly created entities, `UpdatedAt` is always set to equal `CreatedAt` to maintain consistency. This ensures that new entities have identical creation and update timestamps, which is semantically correct since a newly created entity hasn't been modified yet.

### 3. Intelligent Sync Status Management

Sync status is managed with enhanced logic that uses Entity Framework's property modification tracking to determine if explicit modifications were made:

```csharp
// Check if SyncStatus was explicitly modified by user code
var syncStatusProperty = entry.Property(nameof(BaseEntity.SyncStatus));
var originalSyncStatus = (SyncStatus)entry.OriginalValues[nameof(BaseEntity.SyncStatus)];
var currentSyncStatus = entry.Entity.SyncStatus;

// If the sync status hasn't been explicitly modified by user code,
// then we should update it based on business rules
if (!syncStatusProperty.IsModified)
{
    // Change sync status to PendingUpdate if it's currently Synced
    if (entry.Entity.SyncStatus == SyncStatus.Synced)
    {
        entry.Entity.SyncStatus = SyncStatus.PendingUpdate;
    }
    // If it's PendingCreate, keep it as PendingCreate (new entity being modified)
    // If it's other statuses, leave them unchanged
}
```

### 4. Hard Delete Support

Enhanced support for hard delete operations with proper sync status handling:

```csharp
// Check if this is a hard delete (marked with HardDelete sync status)
if (entry.Entity.SyncStatus == SyncStatus.HardDelete)
{
    // Allow hard delete to proceed
    break;
}
```

### 5. Repository Coordination

Repositories can explicitly control audit properties by marking them as modified:

```csharp
// In BaseRepository.MarkAsSyncedAsync
entity.MarkAsSynced();
_context.Entry(entity).Property(nameof(BaseEntity.SyncStatus)).IsModified = true;
```

## Benefits

### 1. Enhanced and Reliable Logic
- Improved sync status change detection using Entity Framework's property modification flags instead of value comparison
- Consistent timestamp management with `UpdatedAt` equal to `CreatedAt` for new entities
- Always updates timestamps for accurate audit trails on modifications
- Proper initial version setting for new entities
- More reliable sync status management that preserves entity lifecycle states
- Prevents false positives when sync status values happen to match but weren't explicitly set

### 2. Enhanced Hard Delete Support
- Proper handling of hard delete operations with SyncStatus.HardDelete
- Bypasses soft delete logic when appropriate
- Better coordination with repository hard delete operations

### 3. Improved Sync Coordination
- Enhanced sync status updates using Entity Framework's property modification tracking
- Preserves entity lifecycle states (e.g., PendingCreate for new entities)
- Better coordination between repository operations and automatic updates
- Reduced sync conflicts through intelligent state management and explicit modification detection
- More accurate detection of intentional sync status changes vs. coincidental value matches

## Usage Patterns

### Repository Operations

When repositories need to control audit properties, they can mark properties as explicitly modified:

```csharp
public async Task<int> MarkAsSyncedAsync(IEnumerable<string> syncIds, CancellationToken cancellationToken = default)
{
    var entities = await _dbSet
        .Where(e => syncIds.Contains(e.SyncId))
        .ToListAsync(cancellationToken);
        
    foreach (var entity in entities)
    {
        entity.MarkAsSynced();
        // Prevent DbContext from overriding this change
        _context.Entry(entity).Property(nameof(BaseEntity.SyncStatus)).IsModified = true;
    }
    
    await _context.SaveChangesAsync(cancellationToken);
    return entities.Count;
}
```

### Business Logic Operations

Normal business operations rely entirely on the context's intelligent audit field management:

```csharp
public async Task<Account> UpdateAccountAsync(Account account, CancellationToken cancellationToken = default)
{
    var existingAccount = await _repository.GetByIdAsync(account.Id, cancellationToken);
    
    // Update entity properties
    _context.Entry(existingEntity).CurrentValues.SetValues(account);
    _context.Entry(existingEntity).State = EntityState.Modified;
    
    // DbContext automatically handles all audit fields:
    // - UpdatedAt timestamp
    // - Version increment  
    // - SyncStatus management (Synced -> PendingUpdate)
    await _context.SaveChangesAsync(cancellationToken);
    
    return existingEntity;
}
```

#### Simplified Repository Pattern

With the enhanced database context, repository update operations are now streamlined:

```csharp
public virtual async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
{
    // Validation and concurrency checks...
    
    // Simple entity update - let DbContext handle audit fields
    _context.Entry(existingEntity).CurrentValues.SetValues(entity);
    _context.Entry(existingEntity).State = EntityState.Modified;
    
    await _context.SaveChangesAsync(cancellationToken);
    return existingEntity;
}
```

## Testing Considerations

### Unit Tests

Tests should verify that the intelligent property management works correctly:

```csharp
[Fact]
public async Task SaveChangesAsync_WhenVersionAlreadyModified_ShouldNotDoubleIncrement()
{
    // Arrange
    var entity = new TestEntity();
    _context.TestEntities.Add(entity);
    await _context.SaveChangesAsync();
    
    var originalVersion = entity.Version;
    
    // Simulate repository modifying version
    entity.Version = originalVersion + 1;
    _context.Entry(entity).Property(nameof(BaseEntity.Version)).IsModified = true;
    
    // Act
    await _context.SaveChangesAsync();
    
    // Assert
    Assert.Equal(originalVersion + 1, entity.Version); // Should not be double-incremented
}
```

### Sync Status Testing Pattern

When testing sync status changes, use the two-phase pattern to work with the audit system correctly:

```csharp
[Fact]
public async Task GetBySyncStatusAsync_WithSpecificStatus_ShouldReturnMatchingEntities()
{
    // Phase 1: Create and persist the entity
    var transaction = TestDataBuilder.CreateTransaction();
    _context.Transactions.Add(transaction);
    await _context.SaveChangesAsync(); // Gets SyncStatus.PendingCreate automatically
    
    // Phase 2: Update sync status and mark property as modified
    transaction.SyncStatus = SyncStatus.Synced;
    _context.Entry(transaction).Property(nameof(Transaction.SyncStatus)).IsModified = true;
    await _context.SaveChangesAsync(); // Persists the explicit sync status
    
    // Act
    var result = await _repository.GetBySyncStatusAsync(SyncStatus.Synced);
    
    // Assert
    Assert.Single(result);
    Assert.Equal(SyncStatus.Synced, result.First().SyncStatus);
}
```

**Why This Pattern is Required:**
- The audit system automatically manages sync status during `SaveChanges`
- New entities get `SyncStatus.PendingCreate` automatically
- To set a specific sync status, you must first persist the entity, then explicitly set and mark the property as modified

**See Also:** [Entity Framework Sync Status Testing Pattern](TESTING.md#entity-framework-sync-status-testing-pattern) in TESTING.md for comprehensive guidance and examples.

### Integration Tests

Integration tests should verify the coordination between repositories and DbContext:

```csharp
[Fact]
public async Task MarkAsSynced_ShouldPreventDbContextOverride()
{
    // Arrange
    var entity = CreateTestEntity();
    await _repository.AddAsync(entity);
    
    // Act
    await _repository.MarkAsSyncedAsync(new[] { entity.SyncId });
    
    // Assert
    Assert.Equal(SyncStatus.Synced, entity.SyncStatus);
}
```

## Migration Considerations

This enhancement is backward compatible and doesn't require database schema changes. Existing applications will benefit from the improved behavior immediately upon upgrading.

## Performance Impact

The enhancement has minimal performance impact:
- Property modification checks are lightweight operations
- No additional database queries
- Slightly reduced unnecessary property updates

## Future Enhancements

Potential future improvements:
1. Configurable audit field behavior
2. Custom audit field strategies per entity type
3. Enhanced logging for audit field operations
4. Audit trail tracking for compliance scenarios