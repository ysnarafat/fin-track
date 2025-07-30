# FinTrack .NET MAUI Blazor Hybrid Application

## Project Structure

This solution contains the FinTrack application built using .NET MAUI Blazor Hybrid technology, enabling cross-platform deployment to multiple platforms.

### Projects

- **FinTrack.Maui** - Main MAUI Blazor Hybrid project with platform-specific implementations

### Platform Targets

The application supports the following platforms:

- **Android** (API 24+)
- **iOS** (15.0+)
- **macOS** (Mac Catalyst 15.0+)
- **Windows** (Windows 10 version 19041+)

### Technology Stack

- .NET 8.0
- .NET MAUI Blazor Hybrid
- Entity Framework Core with SQLite
- Blazor Server Components

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
dotnet build -f net8.0-android    # Android
dotnet build -f net8.0-ios        # iOS
dotnet build -f net8.0-maccatalyst # macOS
dotnet build -f net8.0-windows10.0.19041.0 # Windows
```

### Prerequisites

- .NET 8.0 SDK
- MAUI workloads installed (`dotnet workload install maui`)
- Platform-specific development tools (Android SDK, Xcode for iOS/macOS, Visual Studio for Windows)

## Next Steps

This project structure is ready for the implementation of:

1. Shared library projects (Core, Shared, Infrastructure)
2. Testing infrastructure
3. Domain entities and business logic
4. Data persistence with Entity Framework Core
5. Platform-specific services and UI components