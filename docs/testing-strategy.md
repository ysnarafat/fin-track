# Testing Strategy

## Overview

FinTrack employs a comprehensive testing strategy with clear separation between unit tests, integration tests, and dedicated test utilities. The testing architecture emphasizes proper isolation, fast execution, and type safety through the use of actual Core interfaces in test helpers.

## Test Project Structure

### FinTrack.Tests.Unit
Contains fast-running unit tests with no external dependencies:

```
FinTrack.Tests.Unit/
├── Domain/                     # Entity and domain logic tests
│   ├── Entities/               # Domain entity tests
│   └── ValueObjects/           # Value object tests (Money, DateRange, SyncMetadata)
├── Services/                   # Application service tests
├── Repositories/               # Repository pattern tests
├── Sync/                       # Synchronization logic tests
└── Helpers/                    # Test utility classes
    └── SyncTestHelpers.cs      # Sync-specific test utilities
```

### FinTrack.Tests.Integration
Contains integration tests that verify component interactions:

```
FinTrack.Tests.Integration/
├── Database/                   # SQLite integration tests
├── Sync/                       # End-to-end sync scenarios
├── Platform/                   # Platform-specific service tests
└── UI/                         # XAML navigation and interaction tests
```

## Test Utilities and Helpers

### SyncTestHelpers
The `SyncTestHelpers` class provides utilities for testing synchronization functionality:

```csharp
public static class SyncTestHelpers
{
    // Creates test sync state change events
    public static SyncStateChangedEventArgs CreateSyncStateChangedEventArgs(
        SyncState previousState, 
        SyncState currentState,
        string? errorMessage = null);
    
    // Creates test sync conflicts for resolution testing
    public static SyncConflict CreateSyncConflict(
        string id,
        string entityType,
        string entityId,
        string localData,
        string remoteData);
}
```

**Key Features:**
- **Type Safety**: Uses actual `FinTrack.Core.Interfaces` types rather than test duplicates
- **Consistency**: Ensures test objects match production interface contracts
- **Convenience**: Simplifies creation of complex sync-related test scenarios
- **Maintainability**: Changes to Core interfaces automatically propagate to tests

### Usage Examples

```csharp
// Testing sync state changes
var eventArgs = SyncTestHelpers.CreateSyncStateChangedEventArgs(
    SyncState.Idle, 
    SyncState.Syncing);

// Testing conflict resolution
var conflict = SyncTestHelpers.CreateSyncConflict(
    "conflict-1",
    "Transaction",
    "txn-123",
    "local-data",
    "remote-data");
```

## Testing Principles

### Dependency Isolation
- Unit tests reference only Core, Shared, and Infrastructure projects
- No dependencies on FinTrack.Maui to maintain clean separation
- Use of interfaces and dependency injection for easy mocking

### Fast Execution
- Unit tests run in milliseconds without external dependencies
- In-memory databases for repository testing
- Mocked services for business logic testing

### Type Safety
- Test helpers use actual production interfaces
- Compile-time verification of test object compatibility
- Automatic detection of interface changes through build failures

### Comprehensive Coverage
- **Domain Logic**: Entity validation, business rules, and domain operations
- **Value Objects**: Money arithmetic, DateRange validation, SyncMetadata state management
- **Repository Patterns**: Data access patterns with in-memory databases
- **Synchronization**: Sync scenarios, conflict resolution, and retry logic
- **Platform Services**: Platform-specific service implementations
- **UI Navigation**: XAML page navigation and user interactions

### Value Object Testing
The test suite includes comprehensive coverage for all value objects:

#### Money Tests
- Currency validation (3-letter ISO codes, case normalization)
- Arithmetic operations (addition, subtraction, multiplication, division)
- Currency consistency enforcement
- Edge cases (null currency, division by zero, different currencies)
- Utility methods (`Abs()`, `Negate()`, property checks)

#### DateRange Tests
- Date validation (start date before end date)
- Factory method functionality (`CurrentMonth()`, `LastDays()`, etc.)
- Range operations (`Contains()`, `OverlapsWith()`, `DayCount`)
- Edge cases and boundary conditions

#### SyncMetadata Tests
- State transitions (new → modified → synced → conflict)
- Version management and optimistic concurrency
- Retry logic and error handling
- Device tracking and conflict resolution