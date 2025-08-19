# FinTrack Documentation

Welcome to the FinTrack documentation. This section provides comprehensive information about the architecture, APIs, and development practices for the FinTrack personal finance management application.

## 📚 Documentation Index

### Getting Started
- [Main README](../README.md) - Project overview, setup, and quick start guide
- [Architecture Overview](architecture.md) - System architecture and design principles
- [API Reference](api-reference.md) - Detailed API documentation and interfaces
- [Feature Flags](feature-flags.md) - Runtime feature toggling system documentation

### Development Guides
- [Technology Stack](../README.md#-technology-stack) - Technologies and frameworks used
- [Project Structure](../README.md#project-structure) - Code organization and conventions
- [Building and Running](../README.md#-getting-started) - Build and deployment instructions
- [Sync Architecture](sync-architecture.md) - Detailed synchronization system documentation
- [Testing Strategy](testing-strategy.md) - Comprehensive testing approach and utilities
- [Value Objects](value-objects.md) - Domain value objects (Money, DateRange, SyncMetadata)

### Architecture Documentation

#### Core Components
- **[IFeatureFlagService](api-reference.md#ifeatureflagservice)** - Runtime feature flag management and toggling
- **[IConnectivityService](api-reference.md#iconnectivityservice)** - Network connectivity detection and monitoring
- **[IRepository<T>](api-reference.md#irepositoryt)** - Generic repository pattern for data access
- **[ISyncService](api-reference.md#isyncservice)** - Data synchronization coordination and management
- **[Sync Architecture](sync-architecture.md)** - Comprehensive offline-first synchronization system

#### Design Patterns
- **Clean Architecture** - Separation of concerns across layers
- **MVVM Pattern** - Model-View-ViewModel for XAML UI
- **Repository Pattern** - Data access abstraction
- **Command Pattern** - User action handling

### Key Features

#### Offline-First Functionality
The application is designed to work seamlessly offline with automatic synchronization:

- **Local SQLite Database** - All data stored locally for instant access
- **Real-time Connectivity Monitoring** - `IConnectivityService` provides network status
- **Automatic Sync Queue** - Changes are queued and synchronized when online
- **Conflict Resolution** - Smart conflict resolution using timestamps

#### Cross-Platform Support
Built with .NET MAUI for native performance across platforms:

- **Android** (API 24+)
- **iOS** (15.0+) 
- **macOS** (Mac Catalyst 15.0+)
- **Windows** (Windows 10 version 19041+)

#### Modern XAML UI
- **Dark Theme** - Consistent dark theme across all platforms
- **Responsive Design** - Adapts to different screen sizes and orientations
- **Touch Optimization** - 44px minimum touch targets for mobile
- **Accessibility** - WCAG 2.1 AA compliance features

## 🔧 Recent Updates

### Feature Flag Service Implementation
The `IFeatureFlagService` interface has been added to provide runtime feature toggling capabilities:

```csharp
public interface IFeatureFlagService
{
    bool IsFeatureEnabled(string flagName);
    void SetFeatureFlag(string flagName, bool enabled);
    Dictionary<string, bool> GetAllFeatureFlags();
}
```

**Key Features:**
- Runtime feature toggling without app restarts
- Built-in feature flags for sync functionality
- Settings UI for feature flag management
- Integration with sync status indicators

**Built-in Feature Flags:**
- **OfflineSync**: Controls offline synchronization functionality
- **SyncStatusIndicators**: Toggles sync status display in UI
- **AutomaticSync**: Enables/disables automatic background sync
- **ConflictResolution**: Controls conflict resolution dialog availability

**Implementation:**
- Registered as singleton in dependency injection container
- Integrated with sync service and UI components
- Persistent storage of feature flag states
- Feature flags page for user configuration

### Connectivity Service Implementation
The `IConnectivityService` interface has been added to provide robust network connectivity detection:

```csharp
public interface IConnectivityService
{
    bool IsConnected { get; }
    event EventHandler<bool> ConnectivityChanged;
    void StartMonitoring();
    void StopMonitoring();
}
```

**Key Features:**
- Real-time connectivity monitoring across all platforms
- Event-driven notifications for connectivity changes
- Proper resource management with lifecycle methods
- Thread-safe operation with duplicate call protection

**Implementation:**
- Uses MAUI's native `Connectivity.Current` APIs
- Registered as singleton in dependency injection container
- Integrated with sync service for automatic synchronization

## 🏗️ Architecture Highlights

### Layer Separation
```
┌─────────────────────────────────────────┐
│           Presentation Layer            │
│     (XAML Pages, ViewModels, UI)       │
└─────────────────────────────────────────┘
┌─────────────────────────────────────────┐
│          Application Layer              │
│   (Services, Commands, Sync Logic)     │
└─────────────────────────────────────────┘
┌─────────────────────────────────────────┐
│            Domain Layer                 │
│  (Entities, Interfaces, Business Rules)│
└─────────────────────────────────────────┘
┌─────────────────────────────────────────┐
│         Infrastructure Layer            │
│ (Database, Repositories, External APIs)│
└─────────────────────────────────────────┘
```

### Data Flow
1. **User Interaction** → XAML UI captures input
2. **Command Execution** → ViewModel processes commands
3. **Business Logic** → Application services handle operations
4. **Data Persistence** → Repository pattern manages data access
5. **Sync Management** → Connectivity service triggers synchronization

## 🧪 Testing Strategy

### Test Coverage
- **Unit Tests** - Domain logic, services, and repositories
- **Integration Tests** - Database operations and sync functionality  
- **UI Tests** - XAML navigation and user interactions
- **Platform Tests** - Platform-specific service implementations

### Test Organization
```
tests/
├── FinTrack.Tests.Unit/       # Isolated unit tests
└── FinTrack.Tests.Integration/ # Database and API integration tests
```

## 📱 Platform-Specific Features

### Android
- Material Design components
- Android-specific permissions
- Background sync capabilities
- Local notifications

### iOS
- iOS design guidelines compliance
- Keychain integration
- Background app refresh
- Push notifications

### Windows
- Windows 11 design system
- File system integration
- Windows notifications
- Desktop-specific layouts

### macOS
- macOS design patterns
- Menu bar integration
- Dock notifications
- Native file dialogs

## 🔐 Security Considerations

- **Local Data Encryption** - SQLite database encryption
- **Secure Token Storage** - Platform keychain/keystore integration
- **HTTPS Communication** - All API calls over secure connections
- **Input Validation** - Comprehensive data validation and sanitization

## 🚀 Performance Optimizations

- **Lazy Loading** - On-demand data loading
- **Connection Pooling** - Efficient database connections
- **Image Caching** - Optimized image loading and storage
- **Background Sync** - Non-blocking synchronization operations

## 📞 Support and Contributing

- **Issues** - Report bugs and feature requests on GitHub
- **Pull Requests** - Contribute code improvements and new features
- **Documentation** - Help improve and expand documentation
- **Testing** - Add test coverage and improve test quality

For detailed information on any topic, please refer to the specific documentation files linked above.