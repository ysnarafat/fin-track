# GitHub Actions Workflows

This directory contains GitHub Actions workflows for the FinTrack MAUI application CI/CD pipeline.

## Workflows Overview

### 1. Build and Test (`build-and-test.yml`)
**Triggers:** Push/PR to main/develop branches
- Builds the entire solution
- Runs unit and integration tests
- Generates code coverage reports
- Uploads test results and coverage artifacts
- Comments on PRs with test results

### 2. Build Platforms (`build-platforms.yml`)
**Triggers:** Push to main, PR to main, manual dispatch
- Builds platform-specific versions (Android, Windows, iOS)
- Can be triggered manually with platform selection
- Uploads platform-specific build artifacts
- Supports conditional building based on input parameters

### 3. Code Quality (`code-quality.yml`)
**Triggers:** Push/PR to main/develop branches
- Runs static code analysis
- Checks code formatting with `dotnet format`
- Performs security scanning
- Checks for outdated packages
- Comments on PRs with formatting issues

### 4. Release (`release.yml`)
**Triggers:** Git tags (v*), manual dispatch
- Creates GitHub releases
- Builds release versions for all platforms
- Signs Android APKs (if configured)
- Uploads release assets
- Supports manual release creation

### 5. Dependency Update (`dependency-update.yml`)
**Triggers:** Weekly schedule (Mondays 9 AM UTC), manual dispatch
- Checks for outdated NuGet packages
- Updates packages automatically (minor/patch versions only)
- Runs tests to ensure compatibility
- Creates PRs with dependency updates

## Custom Runner Configuration

These workflows are configured to run on `self-hosted` runners. Make sure your custom runner has:

### Required Software
- .NET 8.0 SDK
- .NET MAUI workload
- Java JDK 11+ (for Android builds)
- Android SDK (for Android builds)
- Git
- 7-Zip (for Windows packaging)

### Environment Variables
Set these on your runner or in repository secrets:

#### Android Signing (Optional)
```bash
ANDROID_KEYSTORE_FILE=<base64-encoded-keystore>
ANDROID_KEYSTORE_PASSWORD=<keystore-password>
ANDROID_KEY_ALIAS=<key-alias>
ANDROID_KEY_PASSWORD=<key-password>
```

#### Android SDK
```bash
ANDROID_SDK_ROOT=<path-to-android-sdk>
```

### Runner Setup Commands
```bash
# Install .NET 8.0 SDK (REQUIRED - pre-install to avoid permission issues)
# Download from https://dotnet.microsoft.com/download/dotnet/8.0
# Or use your package manager:
# Ubuntu/Debian: wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb && sudo dpkg -i packages-microsoft-prod.deb && sudo apt-get update && sudo apt-get install -y dotnet-sdk-8.0
# CentOS/RHEL: sudo dnf install dotnet-sdk-8.0

# Install MAUI workload
dotnet workload install maui

# Install global tools
dotnet tool install -g dotnet-format
dotnet tool install -g dotnet-reportgenerator-globaltool
dotnet tool install -g security-scan
dotnet tool install -g dotnet-outdated-tool

# For Android builds, install Android SDK
# Follow instructions at: https://developer.android.com/studio#command-tools

# Verify installation
dotnet --version
dotnet workload list
```

## Workflow Customization

### Modifying Build Targets
Edit the `env` section in each workflow to change:
- .NET version
- Project paths
- Build configurations

### Adding New Platforms
To add macOS or other platforms:
1. Add a new job in `build-platforms.yml`
2. Configure the appropriate target framework
3. Add platform-specific build steps

### Customizing Test Execution
Modify test commands in `build-and-test.yml`:
- Add test filters
- Change test output formats
- Configure additional test settings

### Security Configuration
For production use, consider:
- Using GitHub secrets for sensitive data
- Implementing code signing for all platforms
- Adding security scanning tools
- Configuring branch protection rules

## Artifacts and Reports

### Generated Artifacts
- **test-results**: Test execution results (TRX format)
- **coverage-report**: Code coverage HTML reports
- **dependency-report**: Outdated packages JSON report
- **platform-builds**: Platform-specific build outputs

### Artifact Retention
- Test results: 30 days
- Coverage reports: 30 days
- Build artifacts: 30 days
- Dependency reports: 30 days

## Troubleshooting

### Common Issues

#### .NET Installation Permission Issues
If you see errors like "Permission denied" when installing .NET:
- The workflows now use `dotnet-install-dir: ${{ runner.temp }}/dotnet` to install to a user directory
- Alternatively, use the `build-and-test-simple.yml` workflow that assumes .NET is pre-installed
- Pre-install .NET 8.0 SDK on your self-hosted runner to avoid installation issues

#### Android Build Failures
- Ensure Android SDK is properly installed
- Check `ANDROID_SDK_ROOT` environment variable
- Verify Java JDK version compatibility

#### iOS Build Issues
- iOS builds require macOS runners
- Xcode and iOS SDK must be installed
- Apple Developer certificates needed for signing

#### Test Failures
- Check test project references
- Ensure all dependencies are restored
- Verify test database connections (for integration tests)

### Debug Steps
1. Check workflow logs in GitHub Actions tab
2. Verify runner configuration and installed software
3. Test builds locally on the runner machine
4. Check repository secrets and environment variables

## Best Practices

### Branch Strategy
- Use `main` for production releases
- Use `develop` for integration testing
- Create feature branches for new development

### Pull Request Workflow
1. Create feature branch
2. Push changes (triggers build-and-test)
3. Create PR (triggers all relevant workflows)
4. Review code quality and test results
5. Merge after approval

### Release Process
1. Create and push version tag (e.g., `v1.0.0`)
2. Release workflow automatically triggers
3. Review generated release assets
4. Publish release when ready

### Maintenance
- Review dependency update PRs weekly
- Monitor workflow execution times
- Update runner software regularly
- Review and update security configurations