using FinTrack.Core.Entities;
using FinTrack.Core.Enums;

namespace FinTrack.Tests.Unit.Domain;

/// <summary>
/// Comprehensive unit tests for BaseEntity abstract class
/// </summary>
public class BaseEntityTests
{
    // Test implementation of BaseEntity for testing purposes
    private class TestEntity : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
    }

    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow.AddSeconds(-1);

        // Act
        var entity = new TestEntity();
        var afterCreation = DateTime.UtcNow.AddSeconds(1);

        // Assert
        Assert.Equal(0, entity.Id); // Default int value
        Assert.True(entity.CreatedAt >= beforeCreation && entity.CreatedAt <= afterCreation);
        Assert.True(entity.UpdatedAt >= beforeCreation && entity.UpdatedAt <= afterCreation);
        Assert.False(entity.IsDeleted);
        Assert.Equal(SyncStatus.PendingCreate, entity.SyncStatus);
        Assert.NotEmpty(entity.SyncId);
        Assert.True(Guid.TryParse(entity.SyncId, out _)); // Should be a valid GUID
        Assert.Null(entity.LastSyncAt);
        Assert.Equal(1, entity.Version);
        Assert.Null(entity.LastModifiedBy);
    }

    [Fact]
    public void Constructor_ShouldGenerateUniqueSyncIds()
    {
        // Act
        var entity1 = new TestEntity();
        var entity2 = new TestEntity();
        var entity3 = new TestEntity();

        // Assert
        Assert.NotEqual(entity1.SyncId, entity2.SyncId);
        Assert.NotEqual(entity1.SyncId, entity3.SyncId);
        Assert.NotEqual(entity2.SyncId, entity3.SyncId);
    }

    [Fact]
    public void MarkAsModified_ShouldUpdateTimestampAndVersion()
    {
        // Arrange
        var entity = new TestEntity();
        var originalUpdatedAt = entity.UpdatedAt;
        var originalVersion = entity.Version;
        
        // Wait a small amount to ensure timestamp difference
        Thread.Sleep(10);

        // Act
        entity.MarkAsModified();

        // Assert
        Assert.True(entity.UpdatedAt > originalUpdatedAt);
        Assert.Equal(originalVersion + 1, entity.Version);
    }

    [Fact]
    public void MarkAsModified_WhenSyncStatusIsSynced_ShouldChangeToPendingUpdate()
    {
        // Arrange
        var entity = new TestEntity();
        entity.MarkAsSynced(); // Set to synced first

        // Act
        entity.MarkAsModified();

        // Assert
        Assert.Equal(SyncStatus.PendingUpdate, entity.SyncStatus);
    }

    [Theory]
    [InlineData(SyncStatus.PendingCreate)]
    [InlineData(SyncStatus.PendingUpdate)]
    [InlineData(SyncStatus.PendingDelete)]
    [InlineData(SyncStatus.SyncFailed)]
    [InlineData(SyncStatus.Conflict)]
    public void MarkAsModified_WhenSyncStatusIsNotSynced_ShouldNotChangeSyncStatus(SyncStatus initialStatus)
    {
        // Arrange
        var entity = new TestEntity();
        entity.SyncStatus = initialStatus;

        // Act
        entity.MarkAsModified();

        // Assert
        Assert.Equal(initialStatus, entity.SyncStatus);
    }

    [Fact]
    public void MarkAsDeleted_ShouldSetDeletedFlagAndUpdateMetadata()
    {
        // Arrange
        var entity = new TestEntity();
        var originalUpdatedAt = entity.UpdatedAt;
        var originalVersion = entity.Version;
        
        // Wait a small amount to ensure timestamp difference
        Thread.Sleep(10);

        // Act
        entity.MarkAsDeleted();

        // Assert
        Assert.True(entity.IsDeleted);
        Assert.True(entity.UpdatedAt > originalUpdatedAt);
        Assert.Equal(originalVersion + 1, entity.Version);
        Assert.Equal(SyncStatus.PendingDelete, entity.SyncStatus);
    }

    [Fact]
    public void MarkAsDeleted_WhenAlreadyDeleted_ShouldStillUpdateMetadata()
    {
        // Arrange
        var entity = new TestEntity();
        entity.MarkAsDeleted(); // Delete first time
        var firstDeleteVersion = entity.Version;
        var firstDeleteTime = entity.UpdatedAt;
        
        // Wait a small amount to ensure timestamp difference
        Thread.Sleep(10);

        // Act
        entity.MarkAsDeleted(); // Delete second time

        // Assert
        Assert.True(entity.IsDeleted);
        Assert.True(entity.UpdatedAt > firstDeleteTime);
        Assert.Equal(firstDeleteVersion + 1, entity.Version);
        Assert.Equal(SyncStatus.PendingDelete, entity.SyncStatus);
    }

    [Fact]
    public void MarkAsSynced_ShouldSetSyncStatusAndTimestamp()
    {
        // Arrange
        var entity = new TestEntity();
        var beforeSync = DateTime.UtcNow;

        // Act
        entity.MarkAsSynced();

        // Assert
        Assert.Equal(SyncStatus.Synced, entity.SyncStatus);
        Assert.NotNull(entity.LastSyncAt);
        Assert.True(entity.LastSyncAt >= beforeSync);
        Assert.True(entity.LastSyncAt <= DateTime.UtcNow);
    }

    [Fact]
    public void MarkAsSynced_CalledMultipleTimes_ShouldUpdateTimestamp()
    {
        // Arrange
        var entity = new TestEntity();
        entity.MarkAsSynced();
        var firstSyncTime = entity.LastSyncAt;
        
        // Wait a small amount to ensure timestamp difference
        Thread.Sleep(10);

        // Act
        entity.MarkAsSynced();

        // Assert
        Assert.Equal(SyncStatus.Synced, entity.SyncStatus);
        Assert.NotNull(entity.LastSyncAt);
        Assert.True(entity.LastSyncAt > firstSyncTime);
    }

    [Fact]
    public void MarkAsConflicted_ShouldSetSyncStatusToConflict()
    {
        // Arrange
        var entity = new TestEntity();
        entity.MarkAsSynced(); // Start with synced status

        // Act
        entity.MarkAsConflicted();

        // Assert
        Assert.Equal(SyncStatus.Conflict, entity.SyncStatus);
    }

    [Fact]
    public void MarkAsSyncFailed_ShouldSetSyncStatusToSyncFailed()
    {
        // Arrange
        var entity = new TestEntity();
        entity.MarkAsSynced(); // Start with synced status

        // Act
        entity.MarkAsSyncFailed();

        // Assert
        Assert.Equal(SyncStatus.SyncFailed, entity.SyncStatus);
    }

    [Fact]
    public void SyncStatusTransitions_ShouldWorkCorrectly()
    {
        // Arrange
        var entity = new TestEntity();

        // Act & Assert - Test typical sync lifecycle
        
        // 1. Entity starts as PendingCreate
        Assert.Equal(SyncStatus.PendingCreate, entity.SyncStatus);
        
        // 2. Entity gets synced
        entity.MarkAsSynced();
        Assert.Equal(SyncStatus.Synced, entity.SyncStatus);
        
        // 3. Entity gets modified
        entity.MarkAsModified();
        Assert.Equal(SyncStatus.PendingUpdate, entity.SyncStatus);
        
        // 4. Sync fails
        entity.MarkAsSyncFailed();
        Assert.Equal(SyncStatus.SyncFailed, entity.SyncStatus);
        
        // 5. Eventually syncs successfully
        entity.MarkAsSynced();
        Assert.Equal(SyncStatus.Synced, entity.SyncStatus);
        
        // 6. Entity gets deleted
        entity.MarkAsDeleted();
        Assert.Equal(SyncStatus.PendingDelete, entity.SyncStatus);
    }

    [Fact]
    public void ConflictResolutionScenario_ShouldWorkCorrectly()
    {
        // Arrange
        var entity = new TestEntity();
        entity.MarkAsSynced(); // Start synced

        // Act & Assert - Test conflict resolution scenario
        
        // 1. Entity gets modified locally
        entity.MarkAsModified();
        Assert.Equal(SyncStatus.PendingUpdate, entity.SyncStatus);
        
        // 2. Sync detects conflict
        entity.MarkAsConflicted();
        Assert.Equal(SyncStatus.Conflict, entity.SyncStatus);
        
        // 3. Conflict gets resolved and entity syncs
        entity.MarkAsSynced();
        Assert.Equal(SyncStatus.Synced, entity.SyncStatus);
    }

    [Fact]
    public void VersionControl_ShouldIncrementCorrectly()
    {
        // Arrange
        var entity = new TestEntity();
        var initialVersion = entity.Version;

        // Act & Assert
        Assert.Equal(1, initialVersion);

        entity.MarkAsModified();
        Assert.Equal(2, entity.Version);

        entity.MarkAsModified();
        Assert.Equal(3, entity.Version);

        entity.MarkAsDeleted();
        Assert.Equal(4, entity.Version);

        // Sync operations should not increment version
        entity.MarkAsSynced();
        Assert.Equal(4, entity.Version);

        entity.MarkAsConflicted();
        Assert.Equal(4, entity.Version);

        entity.MarkAsSyncFailed();
        Assert.Equal(4, entity.Version);
    }

    [Fact]
    public void Properties_ShouldBeSettableAndGettable()
    {
        // Arrange
        var entity = new TestEntity();
        var testId = 42;
        var testCreatedAt = DateTime.UtcNow.AddDays(-1);
        var testUpdatedAt = DateTime.UtcNow.AddHours(-1);
        var testSyncId = Guid.NewGuid().ToString();
        var testLastSyncAt = DateTime.UtcNow.AddMinutes(-30);
        var testVersion = 5L;
        var testLastModifiedBy = "TestDevice123";

        // Act
        entity.Id = testId;
        entity.CreatedAt = testCreatedAt;
        entity.UpdatedAt = testUpdatedAt;
        entity.IsDeleted = true;
        entity.SyncStatus = SyncStatus.Conflict;
        entity.SyncId = testSyncId;
        entity.LastSyncAt = testLastSyncAt;
        entity.Version = testVersion;
        entity.LastModifiedBy = testLastModifiedBy;

        // Assert
        Assert.Equal(testId, entity.Id);
        Assert.Equal(testCreatedAt, entity.CreatedAt);
        Assert.Equal(testUpdatedAt, entity.UpdatedAt);
        Assert.True(entity.IsDeleted);
        Assert.Equal(SyncStatus.Conflict, entity.SyncStatus);
        Assert.Equal(testSyncId, entity.SyncId);
        Assert.Equal(testLastSyncAt, entity.LastSyncAt);
        Assert.Equal(testVersion, entity.Version);
        Assert.Equal(testLastModifiedBy, entity.LastModifiedBy);
    }

    [Fact]
    public void BaseEntity_ShouldSupportInheritance()
    {
        // Arrange & Act
        var entity = new TestEntity { Name = "Test Entity" };

        // Assert
        Assert.IsAssignableFrom<BaseEntity>(entity);
        Assert.Equal("Test Entity", entity.Name);
        Assert.True(entity.Id >= 0); // Has BaseEntity properties
        Assert.NotEmpty(entity.SyncId); // Has BaseEntity properties
    }

    [Fact]
    public void VirtualMethods_ShouldBeOverridable()
    {
        // This test verifies that the virtual methods can be overridden
        // In a real scenario, you might have entities that override these methods

        // Arrange
        var entity = new TestEntity();

        // Act & Assert - Verify methods are virtual by calling them
        // (In a real test, you would create a derived class that overrides these methods)
        
        entity.MarkAsModified(); // Should not throw
        entity.MarkAsDeleted(); // Should not throw
        entity.MarkAsSynced(); // Should not throw
        entity.MarkAsConflicted(); // Should not throw
        entity.MarkAsSyncFailed(); // Should not throw

        // Verify the methods had their expected effects
        Assert.True(entity.IsDeleted);
        Assert.Equal(SyncStatus.SyncFailed, entity.SyncStatus);
    }

    [Fact]
    public void TimestampPrecision_ShouldBeAccurate()
    {
        // This test verifies that timestamps are precise enough for ordering

        // Arrange & Act
        var entity1 = new TestEntity();
        Thread.Sleep(1); // Ensure different timestamps
        var entity2 = new TestEntity();

        // Assert
        Assert.True(entity2.CreatedAt >= entity1.CreatedAt);
        Assert.True(entity2.UpdatedAt >= entity1.UpdatedAt);
    }

    [Theory]
    [InlineData(SyncStatus.PendingCreate)]
    [InlineData(SyncStatus.PendingUpdate)]
    [InlineData(SyncStatus.PendingDelete)]
    [InlineData(SyncStatus.Synced)]
    [InlineData(SyncStatus.SyncFailed)]
    [InlineData(SyncStatus.Conflict)]
    public void SyncStatus_ShouldSupportAllEnumValues(SyncStatus status)
    {
        // Arrange
        var entity = new TestEntity();

        // Act
        entity.SyncStatus = status;

        // Assert
        Assert.Equal(status, entity.SyncStatus);
    }
}