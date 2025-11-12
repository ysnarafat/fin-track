# MAUI App Tests

This directory contains unit tests for the FinTrack MAUI application components.

## Test Coverage

### AppTests.cs
Tests for the main `App` class that handles application initialization and database setup.

**Covered Scenarios:**
- Constructor validation with valid and null AppShell
- MainPage property assignment
- Type inheritance verification
- Database initialization logic (simulated)
- Exception handling during database initialization
- Service provider interaction patterns

**Testing Approach:**
- Uses mocked dependencies to avoid MAUI runtime requirements
- Simulates database initialization logic separately to test error handling
- Focuses on testable aspects without relying on reflection or platform-specific code

### AppShellTests.cs
Tests for the `AppShell` class that manages navigation and sync status display.

**Covered Scenarios:**
- Constructor validation with dependencies
- Type inheritance verification
- Null parameter validation
- Basic class structure validation

**Limitations:**
- XAML initialization may fail in unit test environment (expected)
- Navigation and UI interaction testing requires integration tests
- Event handler testing would need MAUI runtime context

### ViewModels/SyncStatusViewModelTests.cs
Comprehensive tests for the `SyncStatusViewModel` that manages sync status display.

**Covered Scenarios:**
- Property change notifications (INotifyPropertyChanged)
- Connectivity state changes
- Sync state transitions
- Pending changes count updates
- Status text and icon calculations
- Feature flag integration
- Last sync time formatting
- Event handling for sync service events

## Testing Strategy

### Unit Tests vs Integration Tests

**Unit Tests (Current):**
- Test business logic and data transformations
- Mock external dependencies
- Verify property change notifications
- Test error handling and edge cases
- Fast execution, no external dependencies

**Integration Tests (Recommended for Future):**
- Test actual MAUI application lifecycle
- Verify database initialization with real services
- Test navigation and UI interactions
- Validate platform-specific behavior
- Use MAUI test host or UI testing frameworks

### Mocking Strategy

The tests use Moq framework to mock:
- `ISyncService` - Sync operations and state management
- `IConnectivityService` - Network connectivity monitoring
- `IFeatureFlagService` - Feature flag management
- `DatabaseService` - Database operations
- `IServiceProvider` - Dependency injection container

### Test Data Patterns

Tests follow the AAA pattern (Arrange, Act, Assert) and include:
- Theory tests for multiple input scenarios
- Edge case testing (null values, exceptions)
- Property change notification verification
- Event handling validation

## Running the Tests

```bash
# Run all MAUI tests
dotnet test src/frontend/tests/FinTrack.Tests.Unit/Maui/

# Run specific test class
dotnet test --filter "FullyQualifiedName~AppTests"

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Known Limitations

1. **XAML Initialization**: Some tests may skip XAML-dependent operations when running in test environment
2. **Platform Services**: Tests mock platform-specific services rather than testing actual implementations
3. **UI Interactions**: Navigation and user interaction testing requires integration test approach
4. **Async Lifecycle**: Some MAUI lifecycle events are difficult to test in isolation

## Future Improvements

1. **Integration Tests**: Add tests that run in MAUI test host environment
2. **UI Testing**: Implement Appium or similar for end-to-end UI testing
3. **Platform Testing**: Add platform-specific test projects for Android/iOS/Windows
4. **Performance Testing**: Add tests for app startup time and memory usage
5. **Accessibility Testing**: Verify accessibility compliance in UI components

## Dependencies

The test projects require:
- xUnit testing framework
- Moq mocking library
- .NET MAUI framework (for type references)
- Microsoft.Extensions.DependencyInjection (for service provider mocking)

## Best Practices

1. **Isolation**: Each test is independent and doesn't rely on shared state
2. **Descriptive Names**: Test method names clearly describe the scenario being tested
3. **Single Responsibility**: Each test verifies one specific behavior
4. **Mocking**: External dependencies are mocked to ensure unit test isolation
5. **Exception Testing**: Error conditions and edge cases are thoroughly tested