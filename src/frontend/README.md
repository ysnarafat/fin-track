# FinTrack .NET MAUI Application

## Project Structure

This solution contains the FinTrack application built using .NET MAUI with XAML UI, enabling cross-platform deployment to multiple platforms.

### Projects

- **FinTrack.Maui** - Main MAUI project with XAML UI and platform-specific implementations
- **FinTrack.Core** - Domain entities and interfaces
- **FinTrack.Shared** - Shared business logic and services
- **FinTrack.Infrastructure** - Data access implementations
- **FinTrack.Tests.Unit** - Unit tests
- **FinTrack.Tests.Integration** - Integration tests

### Platform Targets

The application supports the following platforms:

- **Android** (API 24+)
- **iOS** (15.0+)
- **macOS** (Mac Catalyst 15.0+)
- **Windows** (Windows 10 version 19041+)

### Technology Stack

- .NET 10.0
- .NET MAUI with XAML UI
- Entity Framework Core 10.0 with SQLite
- CommunityToolkit.Mvvm for MVVM pattern
- Microsoft.Extensions.DependencyInjection

### Platform-Specific Folders

- `Platforms/Android/` - Android-specific implementations
- `Platforms/iOS/` - iOS-specific implementations
- `Platforms/MacCatalyst/` - macOS-specific implementations
- `Platforms/Windows/` - Windows-specific implementations

### Building the Application

To build the application for all platforms:

```bash
dotnet build
```

To build for a specific platform:

```bash
dotnet build -f net10.0-android    # Android
dotnet build -f net10.0-ios        # iOS
dotnet build -f net10.0-maccatalyst # macOS
dotnet build -f net10.0-windows10.0.19041.0 # Windows
```

### Prerequisites

- .NET 10.0 SDK
- MAUI workloads installed (`dotnet workload install maui`)
- Platform-specific development tools (Android SDK, Xcode for iOS/macOS, Visual Studio for Windows)

## Current Implementation Status

The project structure includes:

1. ✅ **Shared library projects** - Core, Shared, Infrastructure projects implemented
2. ✅ **Testing infrastructure** - Unit and integration test projects configured
3. ✅ **Domain entities and business logic** - BaseEntity, Transaction, Account, Category, Goal entities
4. ✅ **Data persistence** - Entity Framework Core with SQLite configured
5. ✅ **XAML UI components** - Dashboard, Transactions, Accounts, Reports, Budgets pages
6. ✅ **Offline-first architecture** - Sync services and connectivity monitoring
7. ✅ **Feature flag system** - Runtime feature toggling capabilities
8. ✅ **Cross-platform services** - Platform-specific implementations

## Key Features Implemented

- **Offline-First Functionality** - Full app functionality without internet
- **Real-time Sync** - Automatic synchronization when connectivity is restored
- **Feature Flags** - Runtime control over sync and UI features
- **Dark Theme UI** - Modern XAML-based dark theme across all platforms
- **MVVM Architecture** - Clean separation with ViewModels and data binding