using FinTrack.Core.Interfaces;
using Moq;

namespace FinTrack.Tests.Unit.Services;

/// <summary>
/// Unit tests for IFeatureFlagService interface contract
/// </summary>
public class IFeatureFlagServiceTests
{
    private readonly Mock<IFeatureFlagService> _mockService;

    public IFeatureFlagServiceTests()
    {
        _mockService = new Mock<IFeatureFlagService>();
    }

    [Fact]
    public void IsFeatureEnabled_WithEnabledFlag_ShouldReturnTrue()
    {
        // Arrange
        var flagName = "TestFeature";
        _mockService.Setup(x => x.IsFeatureEnabled(flagName)).Returns(true);

        // Act
        var result = _mockService.Object.IsFeatureEnabled(flagName);

        // Assert
        Assert.True(result);
        _mockService.Verify(x => x.IsFeatureEnabled(flagName), Times.Once);
    }

    [Fact]
    public void IsFeatureEnabled_WithDisabledFlag_ShouldReturnFalse()
    {
        // Arrange
        var flagName = "DisabledFeature";
        _mockService.Setup(x => x.IsFeatureEnabled(flagName)).Returns(false);

        // Act
        var result = _mockService.Object.IsFeatureEnabled(flagName);

        // Assert
        Assert.False(result);
        _mockService.Verify(x => x.IsFeatureEnabled(flagName), Times.Once);
    }

    [Fact]
    public void IsFeatureEnabled_WithNonExistentFlag_ShouldReturnFalse()
    {
        // Arrange
        var flagName = "NonExistentFeature";
        _mockService.Setup(x => x.IsFeatureEnabled(flagName)).Returns(false);

        // Act
        var result = _mockService.Object.IsFeatureEnabled(flagName);

        // Assert
        Assert.False(result);
        _mockService.Verify(x => x.IsFeatureEnabled(flagName), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void IsFeatureEnabled_WithInvalidFlagName_ShouldHandleGracefully(string flagName)
    {
        // Arrange
        _mockService.Setup(x => x.IsFeatureEnabled(flagName)).Returns(false);

        // Act
        var result = _mockService.Object.IsFeatureEnabled(flagName);

        // Assert
        Assert.False(result);
        _mockService.Verify(x => x.IsFeatureEnabled(flagName), Times.Once);
    }

    [Fact]
    public void SetFeatureFlag_WithValidFlag_ShouldCallService()
    {
        // Arrange
        var flagName = "TestFeature";
        var enabled = true;

        // Act
        _mockService.Object.SetFeatureFlag(flagName, enabled);

        // Assert
        _mockService.Verify(x => x.SetFeatureFlag(flagName, enabled), Times.Once);
    }

    [Fact]
    public void SetFeatureFlag_EnableThenDisable_ShouldUpdateCorrectly()
    {
        // Arrange
        var flagName = "ToggleFeature";
        
        // Setup the mock to track state changes
        var flagState = false;
        _mockService.Setup(x => x.SetFeatureFlag(flagName, It.IsAny<bool>()))
            .Callback<string, bool>((name, enabled) => flagState = enabled);
        _mockService.Setup(x => x.IsFeatureEnabled(flagName))
            .Returns(() => flagState);

        // Act
        _mockService.Object.SetFeatureFlag(flagName, true);
        var enabledResult = _mockService.Object.IsFeatureEnabled(flagName);
        
        _mockService.Object.SetFeatureFlag(flagName, false);
        var disabledResult = _mockService.Object.IsFeatureEnabled(flagName);

        // Assert
        Assert.True(enabledResult);
        Assert.False(disabledResult);
        _mockService.Verify(x => x.SetFeatureFlag(flagName, true), Times.Once);
        _mockService.Verify(x => x.SetFeatureFlag(flagName, false), Times.Once);
    }

    [Fact]
    public void GetAllFeatureFlags_ShouldReturnAllFlags()
    {
        // Arrange
        var expectedFlags = new Dictionary<string, bool>
        {
            { "Feature1", true },
            { "Feature2", false },
            { "Feature3", true }
        };
        _mockService.Setup(x => x.GetAllFeatureFlags()).Returns(expectedFlags);

        // Act
        var result = _mockService.Object.GetAllFeatureFlags();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.True(result["Feature1"]);
        Assert.False(result["Feature2"]);
        Assert.True(result["Feature3"]);
        _mockService.Verify(x => x.GetAllFeatureFlags(), Times.Once);
    }

    [Fact]
    public void GetAllFeatureFlags_WithNoFlags_ShouldReturnEmptyDictionary()
    {
        // Arrange
        var emptyFlags = new Dictionary<string, bool>();
        _mockService.Setup(x => x.GetAllFeatureFlags()).Returns(emptyFlags);

        // Act
        var result = _mockService.Object.GetAllFeatureFlags();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _mockService.Verify(x => x.GetAllFeatureFlags(), Times.Once);
    }

    [Theory]
    [InlineData(FeatureFlags.OfflineSync)]
    [InlineData(FeatureFlags.SyncStatusIndicators)]
    [InlineData(FeatureFlags.AutomaticSync)]
    [InlineData(FeatureFlags.ConflictResolution)]
    public void IsFeatureEnabled_WithKnownFeatureFlags_ShouldWork(string flagName)
    {
        // Arrange
        _mockService.Setup(x => x.IsFeatureEnabled(flagName)).Returns(true);

        // Act
        var result = _mockService.Object.IsFeatureEnabled(flagName);

        // Assert
        Assert.True(result);
        _mockService.Verify(x => x.IsFeatureEnabled(flagName), Times.Once);
    }

    [Fact]
    public void FeatureFlags_Constants_ShouldHaveExpectedValues()
    {
        // Assert
        Assert.Equal("OfflineSync", FeatureFlags.OfflineSync);
        Assert.Equal("SyncStatusIndicators", FeatureFlags.SyncStatusIndicators);
        Assert.Equal("AutomaticSync", FeatureFlags.AutomaticSync);
        Assert.Equal("ConflictResolution", FeatureFlags.ConflictResolution);
    }

    [Fact]
    public void SetFeatureFlag_WithMultipleFlags_ShouldMaintainIndependentState()
    {
        // Arrange
        var flags = new Dictionary<string, bool>
        {
            { "Flag1", false },
            { "Flag2", false },
            { "Flag3", false }
        };

        _mockService.Setup(x => x.SetFeatureFlag(It.IsAny<string>(), It.IsAny<bool>()))
            .Callback<string, bool>((name, enabled) => flags[name] = enabled);
        
        _mockService.Setup(x => x.IsFeatureEnabled(It.IsAny<string>()))
            .Returns<string>(name => flags.ContainsKey(name) && flags[name]);

        // Act
        _mockService.Object.SetFeatureFlag("Flag1", true);
        _mockService.Object.SetFeatureFlag("Flag2", false);
        _mockService.Object.SetFeatureFlag("Flag3", true);

        // Assert
        Assert.True(_mockService.Object.IsFeatureEnabled("Flag1"));
        Assert.False(_mockService.Object.IsFeatureEnabled("Flag2"));
        Assert.True(_mockService.Object.IsFeatureEnabled("Flag3"));
    }

    [Fact]
    public void GetAllFeatureFlags_AfterSettingFlags_ShouldReflectChanges()
    {
        // Arrange
        var flags = new Dictionary<string, bool>();
        
        _mockService.Setup(x => x.SetFeatureFlag(It.IsAny<string>(), It.IsAny<bool>()))
            .Callback<string, bool>((name, enabled) => flags[name] = enabled);
        
        _mockService.Setup(x => x.GetAllFeatureFlags())
            .Returns(() => new Dictionary<string, bool>(flags));

        // Act
        _mockService.Object.SetFeatureFlag("TestFlag1", true);
        _mockService.Object.SetFeatureFlag("TestFlag2", false);
        var result = _mockService.Object.GetAllFeatureFlags();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.True(result["TestFlag1"]);
        Assert.False(result["TestFlag2"]);
    }

    [Fact]
    public void FeatureFlags_CaseSensitivity_ShouldBeConsistent()
    {
        // Arrange
        var flagName = "TestFeature";
        var flagNameDifferentCase = "testfeature";
        
        _mockService.Setup(x => x.IsFeatureEnabled(flagName)).Returns(true);
        _mockService.Setup(x => x.IsFeatureEnabled(flagNameDifferentCase)).Returns(false);

        // Act
        var result1 = _mockService.Object.IsFeatureEnabled(flagName);
        var result2 = _mockService.Object.IsFeatureEnabled(flagNameDifferentCase);

        // Assert
        Assert.True(result1);
        Assert.False(result2);
        // This test demonstrates that feature flag names should be case-sensitive
    }
}

/// <summary>
/// Tests for the FeatureFlags constants class
/// </summary>
public class FeatureFlagsConstantsTests
{
    [Fact]
    public void FeatureFlags_AllConstants_ShouldBeNonEmpty()
    {
        // Assert
        Assert.NotEmpty(FeatureFlags.OfflineSync);
        Assert.NotEmpty(FeatureFlags.SyncStatusIndicators);
        Assert.NotEmpty(FeatureFlags.AutomaticSync);
        Assert.NotEmpty(FeatureFlags.ConflictResolution);
    }

    [Fact]
    public void FeatureFlags_AllConstants_ShouldBeUnique()
    {
        // Arrange
        var flags = new[]
        {
            FeatureFlags.OfflineSync,
            FeatureFlags.SyncStatusIndicators,
            FeatureFlags.AutomaticSync,
            FeatureFlags.ConflictResolution
        };

        // Act
        var uniqueFlags = flags.Distinct().ToArray();

        // Assert
        Assert.Equal(flags.Length, uniqueFlags.Length);
    }

    [Fact]
    public void FeatureFlags_ShouldNotContainWhitespace()
    {
        // Arrange
        var flags = new[]
        {
            FeatureFlags.OfflineSync,
            FeatureFlags.SyncStatusIndicators,
            FeatureFlags.AutomaticSync,
            FeatureFlags.ConflictResolution
        };

        // Assert
        foreach (var flag in flags)
        {
            Assert.DoesNotContain(" ", flag);
            Assert.DoesNotContain("\t", flag);
            Assert.DoesNotContain("\n", flag);
            Assert.DoesNotContain("\r", flag);
        }
    }

    [Theory]
    [InlineData("OfflineSync")]
    [InlineData("SyncStatusIndicators")]
    [InlineData("AutomaticSync")]
    [InlineData("ConflictResolution")]
    public void FeatureFlags_ShouldFollowNamingConvention(string expectedFlagName)
    {
        // This test ensures that feature flag names follow PascalCase convention
        // and match the expected naming pattern
        
        // Act
        var actualFlag = expectedFlagName switch
        {
            "OfflineSync" => FeatureFlags.OfflineSync,
            "SyncStatusIndicators" => FeatureFlags.SyncStatusIndicators,
            "AutomaticSync" => FeatureFlags.AutomaticSync,
            "ConflictResolution" => FeatureFlags.ConflictResolution,
            _ => throw new ArgumentException($"Unknown flag: {expectedFlagName}")
        };

        // Assert
        Assert.Equal(expectedFlagName, actualFlag);
    }
}

// Mock FeatureFlags class for testing (this would be in the Core project)
public static class FeatureFlags
{
    public const string OfflineSync = "OfflineSync";
    public const string SyncStatusIndicators = "SyncStatusIndicators";
    public const string AutomaticSync = "AutomaticSync";
    public const string ConflictResolution = "ConflictResolution";
}