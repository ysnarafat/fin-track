# FinTrack Unit Tests

This project contains comprehensive unit tests for the FinTrack application, focusing on ViewModels, Services, and business logic.

## Test Structure

```
FinTrack.Tests.Unit/
├── ViewModels/              # ViewModel tests
│   └── SyncStatusViewModelTests.cs
├── Services/                # Service layer tests
│   └── FeatureFlagServiceTests.cs
├── Views/                   # View integration tests
│   └── AppShellSyncTests.cs
├── Mocks/                   # Mock implementations
│   ├── MockSyncService.cs
│   └── MockConnectivityService.cs
└── Helpers/                 # Test helpers and utilities
    └── SyncTestHelpers.cs
```

## Testing Framework

- **xUnit** - Primary testing framework
- **Moq** - Mocking framework for dependencies
- **Coverlet** - Code coverage analysis

## Running Tests

### Run all tests
```bash
dotnet test
```

### Run with code coverage
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### Run specific test class
```bash
dotnet test --filter "FullyQualifiedName~SyncStatusViewModelTests"
```

### Run tests in watch mode
```bash
dotnet watch test
```

## Test Categories

### ViewModel Tests
Tests for MVVM ViewModels including:
- Property change notifications (INotifyPropertyChanged)
- Command execution
- Data binding scenarios
- State management
- Event handling

### Service Tests
Tests for application services including:
- Business logic validation
- Feature flag management
- Connectivity monitoring
- Sync operations
- Error handling

### Integration Tests
Tests for component integration including:
- ViewModel-Service interactions
- Event propagation
- Workflow scenarios
- Feature flag integration

## Test Naming Convention

Tests follow the pattern: `MethodName_Scenario_ExpectedBehavior`

Examples:
- `Constructor_InitializesPropertiesFromServices`
- `StatusText_WhenOffline_ReturnsOffline`
- `SyncStateChanged_UpdatesSyncStateProperty`

## Mocking Strategy

- Use Moq for interface mocking
- Create custom mock implementations for complex scenarios
- Verify method calls and event subscriptions
- Setup default behaviors in test constructors

## Code Coverage Goals

- **Minimum**: 80% code coverage
- **Target**: 90%+ code coverage for critical paths
- Focus on:
  - ViewModels: 95%+
  - Services: 90%+
  - Business logic: 95%+

## Best Practices

1. **Arrange-Act-Assert (AAA)** pattern for all tests
2. **Isolated tests** - No dependencies between tests
3. **Descriptive names** - Test names explain what is being tested
4. **Single responsibility** - Each test validates one behavior
5. **Mock external dependencies** - No real database or network calls
6. **Test edge cases** - Include null checks, boundary values, error conditions

## Common Test Patterns

### Testing Property Change Notifications
```csharp
[Fact]
public void Property_WhenChanged_RaisesPropertyChanged()
{
    // Arrange
    var propertyChangedRaised = false;
    viewModel.PropertyChanged += (s, e) =>
    {
        if (e.PropertyName == nameof(ViewModel.Property))
            propertyChangedRaised = true;
    };

    // Act
    viewModel.Property = newValue;

    // Assert
    Assert.True(propertyChangedRaised);
}
```

### Testing Event Subscriptions
```csharp
[Fact]
public void Constructor_SubscribesToEvent()
{
    // Arrange & Act - constructor called

    // Assert
    mockService.VerifyAdd(
        x => x.Event += It.IsAny<EventHandler>(),
        Times.Once);
}
```

### Testing Async Methods
```csharp
[Fact]
public async Task AsyncMethod_WithValidInput_ReturnsExpectedResult()
{
    // Arrange
    mockService.Setup(x => x.MethodAsync())
        .ReturnsAsync(expectedResult);

    // Act
    var result = await service.MethodAsync();

    // Assert
    Assert.Equal(expectedResult, result);
}
```

## Continuous Integration

Tests are automatically run on:
- Pull request creation
- Commits to main branch
- Scheduled nightly builds

## Contributing

When adding new features:
1. Write tests first (TDD approach recommended)
2. Ensure all tests pass before committing
3. Maintain or improve code coverage
4. Follow existing test patterns and naming conventions
5. Add tests for edge cases and error conditions

## Troubleshooting

### Tests fail with "Type not found"
- Ensure all project references are correct
- Rebuild the solution: `dotnet build`

### Mock setup not working
- Verify interface methods are virtual or abstract
- Check that Setup() matches the actual method signature

### Property change events not firing
- Ensure ViewModel implements INotifyPropertyChanged
- Check that OnPropertyChanged is called in property setters

## Resources

- [xUnit Documentation](https://xunit.net/)
- [Moq Documentation](https://github.com/moq/moq4)
- [.NET Testing Best Practices](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)
