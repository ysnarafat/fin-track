# API Reference

## Core Interfaces

### IFeatureFlagService

The `IFeatureFlagService` interface provides runtime feature flag management for controlling application functionality.

#### Namespace
```csharp
FinTrack.Core.Interfaces
```

#### Interface Definition
```csharp
public interface IFeatureFlagService
{
    bool IsFeatureEnabled(string flagName);
    void SetFeatureFlag(string flagName, bool enabled);
    Dictionary<string, bool> GetAllFeatureFlags();
}
```

#### Methods

##### IsFeatureEnabled(string flagName)
```csharp
bool IsFeatureEnabled(string flagName);
```
Gets the value of a feature flag.
- **Parameters**: `flagName` - Name of the feature flag
- **Returns**: `true` if the feature is enabled, `false` otherwise
- **Usage**: Check if a specific feature should be available to the user

##### SetFeatureFlag(string flagName, bool enabled)
```csharp
void SetFeatureFlag(string flagName, bool enabled);
```
Sets the value of a feature flag.
- **Parameters**: 
  - `flagName` - Name of the feature flag
  - `enabled` - Whether the feature should be enabled
- **Usage**: Toggle features on or off at runtime

##### GetAllFeatureFlags()
```csharp
Dictionary<string, bool> GetAllFeatureFlags();
```
Gets all feature flags and their current values.
- **Returns**: Dictionary of feature flag names and their boolean values
- **Usage**: Display all available feature flags in settings UI

#### Built-in Feature Flags

The `FeatureFlags` static class provides constants for built-in feature flags:

```csharp
public static class FeatureFlags
{
    public const string OfflineSync = "OfflineSync";
    public const string SyncStatusIndicators = "SyncStatusIndicators";
    public const string AutomaticSync = "AutomaticSync";
    public const string ConflictResolution = "ConflictResolution";
}
```

- **OfflineSync**: Controls offline data synchronization functionality
- **SyncStatusIndicators**: Toggles sync status display in UI headers and pages
- **AutomaticSync**: Enables/disables automatic background synchronization
- **ConflictResolution**: Controls availability of conflict resolution dialogs

#### Usage Example

```csharp
public class SyncService
{
    private readonly IFeatureFlagService _featureFlagService;
    
    public SyncService(IFeatureFlagService featureFlagService)
    {
        _featureFlagService = featureFlagService;
    }
    
    public async Task SyncAsync()
    {
        if (!_featureFlagService.IsFeatureEnabled(FeatureFlags.OfflineSync))
        {
            return; // Sync is disabled
        }
        
        // Perform sync operations...
    }
}
```

### IConnectivityService

The `IConnectivityService` interface provides network connectivity detection and monitoring capabilities for the offline-first architecture.

#### Namespace
```csharp
FinTrack.Core.Interfaces
```

#### Interface Definition
```csharp
public interface IConnectivityService
{
    bool IsConnected { get; }
    event EventHandler<bool> ConnectivityChanged;
    void StartMonitoring();
    void StopMonitoring();
}
```

#### Properties

##### IsConnected
```csharp
bool IsConnected { get; }
```
Gets the current connectivity status.
- **Returns**: `true` if the device has internet connectivity, `false` otherwise
- **Type**: `bool`

#### Events

##### ConnectivityChanged
```csharp
event EventHandler<bool> ConnectivityChanged;
```
Event raised when connectivity status changes.
- **Event Args**: `bool` - The new connectivity status
- **Usage**: Subscribe to receive real-time connectivity change notifications

#### Methods

##### StartMonitoring()
```csharp
void StartMonitoring();
```
Starts monitoring connectivity changes.
- **Behavior**: Begins listening for network connectivity changes and raises `ConnectivityChanged` events
- **Thread Safety**: Safe to call multiple times; subsequent calls are ignored if already monitoring

##### StopMonitoring()
```csharp
void StopMonitoring();
```
Stops monitoring connectivity changes.
- **Behavior**: Stops listening for network connectivity changes
- **Thread Safety**: Safe to call multiple times; subsequent calls are ignored if not monitoring

#### Implementation

The MAUI implementation (`FinTrack.Maui.Services.ConnectivityService`) uses the platform's native connectivity APIs through MAUI's `Connectivity.Current` service.

```csharp
public class ConnectivityService : IConnectivityService
{
    public bool IsConnected => Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
    
    // Implementation details...
}
```

#### Usage Example

```csharp
public class SyncService
{
    private readonly IConnectivityService _connectivityService;
    
    public SyncService(IConnectivityService connectivityService)
    {
        _connectivityService = connectivityService;
        _connectivityService.ConnectivityChanged += OnConnectivityChanged;
        _connectivityService.StartMonitoring();
    }
    
    private async void OnConnectivityChanged(object sender, bool isConnected)
    {
        if (isConnected)
        {
            await SyncPendingChangesAsync();
        }
    }
}
```

### IRepository<T>

Generic repository interface for basic CRUD operations with sync support.

#### Interface Definition
```csharp
public interface IRepository<T> where T : BaseEntity
{
    // Basic CRUD operations
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<T?> GetBySyncIdAsync(string syncId, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    
    // Sync-related operations
    Task<IEnumerable<T>> GetPendingSyncAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetBySyncStatusAsync(SyncStatus syncStatus, CancellationToken cancellationToken = default);
    Task<int> MarkAsSyncedAsync(IEnumerable<string> syncIds, CancellationToken cancellationToken = default);
    
    // Query operations
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetPagedAsync(int skip, int take, Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default);
}
```

## Enums

### SyncStatus

Represents the synchronization status of an entity.

```csharp
public enum SyncStatus
{
    Synced = 0,         // Entity is synchronized with the remote server
    PendingCreate = 1,  // Entity needs to be created on the server
    PendingUpdate = 2,  // Entity needs to be updated on the server
    PendingDelete = 3,  // Entity needs to be deleted on the server
    SyncFailed = 4,     // Entity sync failed and needs to be retried
    Conflict = 5        // Entity has a sync conflict that needs resolution
}
```

### SyncOperation

Represents the type of synchronization operation.

```csharp
public enum SyncOperation
{
    Create = 0,  // Create operation - entity needs to be created on the server
    Update = 1,  // Update operation - entity needs to be updated on the server
    Delete = 2,  // Delete operation - entity needs to be deleted on the server
    None = 3     // No operation - entity is already synced
}
```

## Domain Entities

### BaseEntity

Abstract base class for all domain entities.

```csharp
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public SyncStatus SyncStatus { get; set; }
    public string SyncId { get; set; }
}
```

### Transaction

Represents a financial transaction.

```csharp
public class Transaction : BaseEntity
{
    public decimal Amount { get; set; }
    public string Description { get; set; }
    public DateTime Date { get; set; }
    public int CategoryId { get; set; }
    public int AccountId { get; set; }
    public TransactionType Type { get; set; }
    
    // Navigation properties
    public Category Category { get; set; }
    public Account Account { get; set; }
}
```

### Account

Represents a user account.

```csharp
public class Account : BaseEntity
{
    public string Name { get; set; }
    public decimal Balance { get; set; }
    public AccountType Type { get; set; }
    public string Currency { get; set; }
    
    // Navigation properties
    public ICollection<Transaction> Transactions { get; set; }
}
```

### Category

Represents a transaction category.

```csharp
public class Category : BaseEntity
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Color { get; set; }
    public string Icon { get; set; }
    
    // Navigation properties
    public ICollection<Transaction> Transactions { get; set; }
}
```

### Goal

Represents a financial goal.

```csharp
public class Goal : BaseEntity
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; }
    public DateTime TargetDate { get; set; }
    public GoalStatus Status { get; set; }
}
```

## Service Registration

Services are registered in `MauiProgram.cs`:

```csharp
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        
        // Register Connectivity Service
        builder.Services.AddSingleton<IConnectivityService, ConnectivityService>();
        
        // Register Feature Flag Service
        builder.Services.AddSingleton<IFeatureFlagService, FeatureFlagService>();
        
        // Register Sync Service
        builder.Services.AddSingleton<ISyncService, SyncService>();
        
        // Register other services...
        
        return builder.Build();
    }
}
```

## Platform-Specific Implementations

### ConnectivityService (MAUI)

The MAUI implementation uses the platform's native connectivity detection:

```csharp
public class ConnectivityService : IConnectivityService
{
    private bool _isMonitoring;

    public bool IsConnected => Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

    public event EventHandler<bool>? ConnectivityChanged;

    public void StartMonitoring()
    {
        if (_isMonitoring) return;
        
        Connectivity.Current.ConnectivityChanged += OnConnectivityChanged;
        _isMonitoring = true;
    }

    public void StopMonitoring()
    {
        if (!_isMonitoring) return;
        
        Connectivity.Current.ConnectivityChanged -= OnConnectivityChanged;
        _isMonitoring = false;
    }

    private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {
        var isConnected = e.NetworkAccess == NetworkAccess.Internet;
        ConnectivityChanged?.Invoke(this, isConnected);
    }
}
```

This implementation provides:
- Real-time connectivity monitoring across all MAUI platforms
- Event-driven notifications for connectivity changes
- Proper resource management with start/stop monitoring
- Thread-safe operation with duplicate call protection