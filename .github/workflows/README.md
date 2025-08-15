# GitHub Actions Workflows (Linux Self-Hosted Runner)

Simple CI/CD workflows designed for Linux self-hosted runners.

## Workflows

### 1. CI (`ci.yml`)
**Triggers:** Push/PR to main/develop/topic branches

**Supported Branch Patterns:**
- `main` - Main production branch
- `develop` - Development integration branch  
- `topic/{issue_no}/{issue_slug}` - Feature/topic branches

**What it does:**
- Builds core libraries (excludes MAUI project)
- Runs unit and integration tests
- Generates code coverage reports
- Performs code quality checks
- Comments on PRs with results

**Projects built:**
- ✅ FinTrack.Core
- ✅ FinTrack.Shared
- ✅ FinTrack.Infrastructure
- ✅ FinTrack.Tests.Unit
- ✅ FinTrack.Tests.Integration
- ❌ FinTrack.Maui (Linux limitation)

### 2. Dependency Update (`dependency-update.yml`)
**Triggers:** Weekly (Mondays 9 AM UTC) or manual

**What it does:**
- Checks for outdated NuGet packages
- Updates minor/patch versions only (safe updates)
- Tests all projects after updates
- Creates PR with changes if updates found

## Linux Runner Setup

### Prerequisites
```bash
# Your runner already has .NET 9.0.3 installed - perfect!
# Verify installation
dotnet --version  # Should show 9.0.3
dotnet --info

# Test global tool installation (important for CI)
dotnet tool install -g dotnet-format --version 5.1.250801
dotnet tool uninstall -g dotnet-format
echo "✅ Global tools work correctly"

# Note: Projects have been updated to target .NET 9.0 to match your runner
```

### Verify Setup
Run the test workflow to verify your runner is properly configured:
1. Go to Actions tab in GitHub
2. Run "Test Setup" workflow manually
3. Check that all tests pass

### Global Tools (Auto-installed by workflows)
- `dotnet-format` - Code formatting
- `dotnet-reportgenerator-globaltool` - Coverage reports
- `dotnet-outdated-tool` - Dependency updates

## What Works on Linux

✅ **Fully Supported:**
- Core business logic development
- Unit and integration testing
- Code coverage and quality checks
- Dependency management
- Static analysis

❌ **Not Supported:**
- MAUI project builds
- Android/iOS/Windows app builds
- Platform-specific testing

## Usage

### Running CI Locally
```bash
# Clean
rm -rf src/frontend/*/bin src/frontend/*/obj

# Build core projects (now targeting .NET 9.0)
dotnet build src/frontend/FinTrack.Core/FinTrack.Core.csproj --configuration Release
dotnet build src/frontend/FinTrack.Shared/FinTrack.Shared.csproj --configuration Release
dotnet build src/frontend/FinTrack.Infrastructure/FinTrack.Infrastructure.csproj --configuration Release

# Run tests
dotnet test src/frontend/FinTrack.Tests.Unit/FinTrack.Tests.Unit.csproj --configuration Release
dotnet test src/frontend/FinTrack.Tests.Integration/FinTrack.Tests.Integration.csproj --configuration Release
```

### Manual Dependency Updates
```bash
# Install tool
dotnet tool install --global dotnet-outdated-tool

# Check for updates
cd src/frontend
dotnet outdated

# Update (minor/patch only)
dotnet outdated --upgrade --version-lock Major
```

## Artifacts

- **test-results-{run_number}**: Test execution results and coverage
- **outdated-packages-report**: Dependency update reports

Retention: 30 days

## Development Workflow

### Branch-Based Development
1. **Create topic branch**: `git checkout -b topic/123/add-new-feature`
2. **Develop** business logic in Core/Shared/Infrastructure
3. **Push** to trigger CI on your topic branch
4. **Create PR** to main/develop to trigger CI validation
5. **Review** PR comments and coverage reports
6. **Merge** when CI passes

### Supported Workflows
- **Direct push** to main/develop/topic branches triggers CI
- **Pull requests** to main/develop/topic branches trigger CI
- **All branches** get the same comprehensive validation

For MAUI/platform builds, you'll need separate Windows/macOS runners or local development.