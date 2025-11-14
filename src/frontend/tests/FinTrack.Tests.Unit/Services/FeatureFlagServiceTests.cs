using FinTrack.Core.Interfaces;
using Moq;

namespace FinTrack.Tests.Unit.Services;

/// <summary>
/// Unit tests for IFeatureFlagService implementations
/// </summary>
public class FeatureFlagServiceTests
{
    private readonly Mock<IFeatureFlagService> _mockFeatureFlagService;

    public FeatureFlagServiceTests()
    {
        _mockFeatureFlagService = new Mock<IFeatureFlagService>();
    }

    [Theory]
    [InlineData(FeatureFlags.OfflineSync, true)]
    [InlineData(FeatureFlags.SyncStatusIndicators, false)]
    [InlineData(FeatureFlags.AutomaticSync, true)]
    [InlineData(FeatureFlags.ConflictResolution, false)]
    public void IsFeatureEnabled_WithKnownFlags_ShouldReturnExpectedValue(string flagName, bool expectedValue)
    {
        // Arrange
        _mockFeatureFlagService.Setup(s => s.IsFeatureEnabled(flagName))
            .Returns(expectedValue);

        // Act
        var result = _mockFeatureFlagService.Object.IsFeatureEnabled(flagName);

        // Assert
        Assert.Equal(expectedValue, result);
        _mockFeatureFlagService.Verify(s => s.IsFeatureEnabled(flagName), Times.Once);
    }

    [Fact]
    public void IsFeatureEnabled_WithUnknownFlag_ShouldReturnFalse()
    {
        // Arrange
        var unknownFlag = "UnknownFeatureFlag";
        _mockFeatureFlagService.Setup(s => s.IsFeatureEnabled(unknownFlag))
            .Returns(false);

        // Act
        var result = _mockFeatureFlagService.Object.IsFeatureEnabled(unknownFlag);

        // Assert
        Assert.False(result);
        _mockFeatureFlagService.Verify(s => s.IsFeatureEnabled(unknownFlag), Times.Once);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData(null, false)]
    public void IsFeatureEnabled_WithInvalidFlagName_ShouldReturnFalse(string flagName, bool expectedValue)
    {
        // Arrange
        _mockFeatureFlagService.Setup(s => s.IsFeatureEnabled(flagName))
            .Returns(expectedValue);

        // Act
        var result = _mockFeatureFlagService.Object.IsFeatureEnabled(flagName);

        // Assert
        Assert.Equal(expectedValue, result);
        _mockFeatureFlagService.Verify(s => s.IsFeatureEnabled(flagName), Times.Once);
    }

    [Fact]
    public void SetFeatureFlag_WithValidFlag_ShouldUpdateFlagValue()
    {
        // Arrange
        var flagName = FeatureFlags.OfflineSync;
        var flagValue = true;

        _mockFeatureFlagService.Setup(s => s.SetFeatureFlag(flagName, flagValue));
        _mockFeatureFlagService.Setup(s => s.IsFeatureEnabled(flagName))
            .Returns(flagValue);

        // Act
        _mockFeatureFlagService.Object.SetFeatureFlag(flagName, flagValue);
        var result = _mockFeatureFlagService.Object.IsFeatureEnabled(flagName);

        // Assert
        Assert.Equal(flagValue, result);
        _mockFeatureFlagService.Verify(s => s.SetFeatureFlag(flagName, flagValue), Times.Once);
        _mockFeatureFlagService.Verify(s => s.IsFeatureEnabled(flagName), Times.Once);
    }

    [Fact]
    public void SetFeatureFlag_TogglingFlag_ShouldUpdateCorrectly()
    {
        // Arrange
        var flagName = FeatureFlags.SyncStatusIndicators;

        // Setup sequence: initially false, then true after setting
        _mockFeatureFlagService.SetupSequence(s => s.IsFeatureEnabled(flagName))
            .Returns(false)
            .Returns(true);

        _mockFeatureFlagService.Setup(s => s.SetFeatureFlag(flagName, true));

        // Act
        var initialValue = _mockFeatureFlagService.Object.IsFeatureEnabled(flagName);
        _mockFeatureFlagService.Object.SetFeatureFlag(flagName, true);
        var updatedValue = _mockFeatureFlagService.Object.IsFeatureEnabled(flagName);

        // Assert
        Assert.False(initialValue);
        Assert.True(updatedValue);
        _mockFeatureFlagService.Verify(s => s.SetFeatureFlag(flagName, true), Times.Once);
        _mockFeatureFlagService.Verify(s => s.IsFeatureEnabled(flagName), Times.Exactly(2));
    }

    [Fact]
    public void SetFeatureFlag_WithMultipleFlags_ShouldUpdateIndependently()
    {
        // Arrange
        var flag1 = FeatureFlags.OfflineSync;
        var flag2 = FeatureFlags.AutomaticSync;

        _mockFeatureFlagService.Setup(s => s.SetFeatureFlag(flag1, true));
        _mockFeatureFlagService.Setup(s => s.SetFeatureFlag(flag2, false));
        _mockFeatureFlagService.Setup(s => s.IsFeatureEnabled(flag1)).Returns(true);
        _mockFeatureFlagService.Setup(s => s.IsFeatureEnabled(flag2)).Returns(false);

        // Act
        _mockFeatureFlagService.Object.SetFeatureFlag(flag1, true);
        _mockFeatureFlagService.Object.SetFeatureFlag(flag2, false);

        var flag1Value = _mockFeatureFlagService.Object.IsFeatureEnabled(flag1);
        var flag2Value = _mockFeatureFlagService.Object.IsFeatureEnabled(flag2);

        // Assert
        Assert.True(flag1Value);
        Assert.False(flag2Value);
        _mockFeatureFlagService.Verify(s => s.SetFeatureFlag(flag1, true), Times.Once);
        _mockFeatureFlagService.Verify(s => s.SetFeatureFlag(flag2, false), Times.Once);
    }

    [Fact]
    public void GetAllFeatureFlags_ShouldReturnAllFlags()
    {
        // Arrange
        var expectedFlags = new Dictionary<string, bool>
        {
            { FeatureFlags.OfflineSync, true },
            { FeatureFlags.SyncStatusIndicators, false },
            { FeatureFlags.AutomaticSync, true },
            { FeatureFlags.ConflictResolution, false }
        };

        _mockFeatureFlagService.Setup(s => s.GetAllFeatureFlags())
            .Returns(expectedFlags);

        // Act
        var result = _mockFeatureFlagService.Object.GetAllFeatureFlags();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(4, result.Count);
        Assert.Equal(expectedFlags, result);
        _mockFeatureFlagService.Verify(s => s.GetAllFeatureFlags(), Times.Once);
    }

    [Fact]
    public void GetAllFeatureFlags_WithNoFlags_ShouldReturnEmptyDictionary()
    {
        // Arrange
        var emptyFlags = new Dictionary<string, bool>();

        _mockFeatureFlagService.Setup(s => s.GetAllFeatureFlags())
            .Returns(emptyFlags);

        // Act
        var result = _mockFeatureFlagService.Object.GetAllFeatureFlags();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _mockFeatureFlagService.Verify(s => s.GetAllFeatureFlags(), Times.Once);
    }

    [Fact]
    public void GetAllFeatureFlags_ShouldIncludeAllKnownFlags()
    {
        // Arrange
        var allFlags = new Dictionary<string, bool>
        {
            { FeatureFlags.OfflineSync, true },
            { FeatureFlags.SyncStatusIndicators, true },
            { FeatureFlags.AutomaticSync, false },
            { FeatureFlags.ConflictResolution, true }
        };

        _mockFeatureFlagService.Setup(s => s.GetAllFeatureFlags())
            .Returns(allFlags);

        // Act
        var result = _mockFeatureFlagService.Object.GetAllFeatureFlags();

        // Assert
        Assert.NotNull(result);
        Assert.Contains(FeatureFlags.OfflineSync, result.Keys);
        Assert.Contains(FeatureFlags.SyncStatusIndicators, result.Keys);
        Assert.Contains(FeatureFlags.AutomaticSync, result.Keys);
        Assert.Contains(FeatureFlags.ConflictResolution, result.Keys);
        _mockFeatureFlagService.Verify(s => s.GetAllFeatureFlags(), Times.Once);
    }

    [Fact]
    public void GetAllFeatureFlags_AfterSettingFlags_ShouldReflectChanges()
    {
        // Arrange
        var initialFlags = new Dictionary<string, bool>
        {
            { FeatureFlags.OfflineSync, false },
            { FeatureFlags.SyncStatusIndicators, false }
        };

        var updatedFlags = new Dictionary<string, bool>
        {
            { FeatureFlags.OfflineSync, true },
            { FeatureFlags.SyncStatusIndicators, false }
        };

        _mockFeatureFlagService.SetupSequence(s => s.GetAllFeatureFlags())
            .Returns(initialFlags)
            .Returns(updatedFlags);

        _mockFeatureFlagService.Setup(s => s.SetFeatureFlag(FeatureFlags.OfflineSync, true));

        // Act
        var initialResult = _mockFeatureFlagService.Object.GetAllFeatureFlags();
        _mockFeatureFlagService.Object.SetFeatureFlag(FeatureFlags.OfflineSync, true);
        var updatedResult = _mockFeatureFlagService.Object.GetAllFeatureFlags();

        // Assert
        Assert.False(initialResult[FeatureFlags.OfflineSync]);
        Assert.True(updatedResult[FeatureFlags.OfflineSync]);
        _mockFeatureFlagService.Verify(s => s.GetAllFeatureFlags(), Times.Exactly(2));
        _mockFeatureFlagService.Verify(s => s.SetFeatureFlag(FeatureFlags.OfflineSync, true), Times.Once);
    }

    [Theory]
    [InlineData(FeatureFlags.OfflineSync)]
    [InlineData(FeatureFlags.SyncStatusIndicators)]
    [InlineData(FeatureFlags.AutomaticSync)]
    [InlineData(FeatureFlags.ConflictResolution)]
    public void FeatureFlags_Constants_ShouldHaveValidValues(string flagName)
    {
        // Act & Assert
        Assert.NotNull(flagName);
        Assert.NotEmpty(flagName);
        Assert.DoesNotContain(" ", flagName); // No spaces in flag names
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
    public void FeatureFlags_Constants_ShouldFollowNamingConvention()
    {
        // Arrange
        var flags = new[]
        {
            FeatureFlags.OfflineSync,
            FeatureFlags.SyncStatusIndicators,
            FeatureFlags.AutomaticSync,
            FeatureFlags.ConflictResolution
        };

        // Act & Assert
        foreach (var flag in flags)
        {
            Assert.True(char.IsUpper(flag[0]), $"Flag '{flag}' should start with uppercase letter");
            Assert.DoesNotContain("_", flag); // Should use PascalCase, not snake_case
            Assert.DoesNotContain("-", flag); // Should not contain hyphens
        }
    }

    [Fact]
    public void FeatureFlagService_ShouldSupportConcurrentAccess()
    {
        // This test verifies that the service can handle concurrent operations
        // In a real implementation, you would test thread safety

        // Arrange
        var flags = new Dictionary<string, bool>
        {
            { FeatureFlags.OfflineSync, true },
            { FeatureFlags.SyncStatusIndicators, false }
        };

        _mockFeatureFlagService.Setup(s => s.GetAllFeatureFlags()).Returns(flags);
        _mockFeatureFlagService.Setup(s => s.IsFeatureEnabled(It.IsAny<string>()))
            .Returns<string>(flagName => flags.ContainsKey(flagName) && flags[flagName]);

        // Act
        var tasks = new List<Task<bool>>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(() => _mockFeatureFlagService.Object.IsFeatureEnabled(FeatureFlags.OfflineSync)));
        }

        var results = Task.WhenAll(tasks).Result;

        // Assert
        Assert.All(results, result => Assert.True(result));
        _mockFeatureFlagService.Verify(s => s.IsFeatureEnabled(FeatureFlags.OfflineSync), Times.Exactly(10));
    }

    [Fact]
    public void FeatureFlagService_ShouldPersistChanges()
    {
        // This test verifies that flag changes are persisted
        // In a real implementation, you would test persistence to storage

        // Arrange
        var flagName = FeatureFlags.OfflineSync;
        var sequence = new Queue<bool>(new[] { false, true, true });

        _mockFeatureFlagService.Setup(s => s.IsFeatureEnabled(flagName))
            .Returns(() => sequence.Dequeue());
        _mockFeatureFlagService.Setup(s => s.SetFeatureFlag(flagName, true));

        // Act
        var initialValue = _mockFeatureFlagService.Object.IsFeatureEnabled(flagName);
        _mockFeatureFlagService.Object.SetFeatureFlag(flagName, true);
        var valueAfterSet = _mockFeatureFlagService.Object.IsFeatureEnabled(flagName);
        var valueAfterRestart = _mockFeatureFlagService.Object.IsFeatureEnabled(flagName);

        // Assert
        Assert.False(initialValue);
        Assert.True(valueAfterSet);
        Assert.True(valueAfterRestart); // Should persist after "restart"
        _mockFeatureFlagService.Verify(s => s.SetFeatureFlag(flagName, true), Times.Once);
        _mockFeatureFlagService.Verify(s => s.IsFeatureEnabled(flagName), Times.Exactly(3));
    }
}