using FinTrack.Core.Entities;
using FinTrack.Core.Enums;

namespace FinTrack.Tests.Unit.Core.Entities;

/// <summary>
/// Unit tests for BaseEntity class
/// </summary>
public class BaseEntityTests
{
    private class TestEntity : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
    }

    [Fact]
    public void Constructor_ShouldInitializeDefaultValues()
    {
        // Arrange & Act
        var entity = new TestEntity();

        // Assert
        Assert.True(entity.Id == 0); // Default int value
        Assert.True(entity.CreatedAt <= DateTime.UtcNow);
        Assert.True(entity.UpdatedAt <= DateTime.UtcNow);
        Assert.False(entity.IsDeleted);
        Assert.Equal(SyncStatus.PendingCreate, entity.SyncStatus);
        Assert.NotEmpty(entity.SyncId);
        Assert.Null(entity.LastSyncAt);
        Assert.Equal(1, entity.Version);
        Assert.Null(entity.LastModifiedBy);
    }

    [Fact]
    public void Constructor_ShouldGenerateUniqueSyncIds()
    {
        // Arrange & Act
        var entity1 = new TestEntity();
        var entity2 = new TestEntity();

        // Assert
        Assert.NotEqual(entity1.SyncId, entity2.SyncId);
        Assert.True(Guid.TryParse(entity1.SyncId, out _));
        Assert.True(Guid.TryParse(entity2.SyncId, out _));
    }

    [Fact]
    public void MarkAsModified_ShouldUpdateTimestampAndVersion()
    {
        // Arrange
        var entity = new TestEntity();
        var originalUpdatedAt = entity.UpdatedAt;
        var originalVersion = entity.Version;
        
        // Wait a small amount to ensure timestamp difference
        Thread.Sleep(1);

        // Act
        entity.MarkAsModified();

        // Assert
        Assert.True(entity.UpdatedAt > originalUpdatedAt);
        Assert.Equal(originalVersion + 1, entity.Version);
    }

    [Fact]
    public void MarkAsModified_WhenSynced_ShouldChangeToPendingUpdate()
    {
        // Arrange
        var entity = new TestEntity();
        entity.MarkAsSynced(); // Set to synced first

        // Act
        entity.MarkAsModified();

        // Assert
        Assert.Equal(SyncStatus.PendingUpdate, entity.SyncStatus);
    }

    [Fact]
    public void MarkAsModified_WhenNotSynced_ShouldNotChangeSyncStatus()
    {
        // Arrange
        var entity = new TestEntity();
        var originalSyncStatus = entity.SyncStatus; // Should be PendingCreate

        // Act
        entity.MarkAsModified();

        // Assert
        Assert.Equal(originalSyncStatus, entity.SyncStatus);
    }

    [Fact]
    public void MarkAsDeleted_ShouldSetDeletedFlagAndUpdateMetadata()
    {
        // Arrange
        var entity = new TestEntity();
        var originalUpdatedAt = entity.UpdatedAt;
        var originalVersion = entity.Version;
        
        Thread.Sleep(1);

        // Act
        entity.MarkAsDeleted();

        // Assert
        Assert.True(entity.IsDeleted);
        Assert.True(entity.UpdatedAt > originalUpdatedAt);
        Assert.Equal(originalVersion + 1, entity.Version);
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
    public void MarkAsConflicted_ShouldSetSyncStatusToConflict()
    {
        // Arrange
        var entity = new TestEntity();

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

        // Act
        entity.MarkAsSyncFailed();

        // Assert
        Assert.Equal(SyncStatus.SyncFailed, entity.SyncStatus);
    }

    [Theory]
    [InlineData(SyncStatus.PendingCreate)]
    [InlineData(SyncStatus.PendingUpdate)]
    [InlineData(SyncStatus.PendingDelete)]
    [InlineData(SyncStatus.SyncFailed)]
    [InlineData(SyncStatus.Conflict)]
    public void SyncStatusTransitions_ShouldWorkCorrectly(SyncStatus targetStatus)
    {
        // Arrange
        var entity = new TestEntity();

        // Act & Assert based on target status
        switch (targetStatus)
        {
            case SyncStatus.PendingCreate:
                // Already set by constructor
                Assert.Equal(SyncStatus.PendingCreate, entity.SyncStatus);
                break;
            case SyncStatus.PendingUpdate:
                entity.MarkAsSynced();
                entity.MarkAsModified();
                Assert.Equal(SyncStatus.PendingUpdate, entity.SyncStatus);
                break;
            case SyncStatus.PendingDelete:
                entity.MarkAsDeleted();
                Assert.Equal(SyncStatus.PendingDelete, entity.SyncStatus);
                break;
            case SyncStatus.SyncFailed:
                entity.MarkAsSyncFailed();
                Assert.Equal(SyncStatus.SyncFailed, entity.SyncStatus);
                break;
            case SyncStatus.Conflict:
                entity.MarkAsConflicted();
                Assert.Equal(SyncStatus.Conflict, entity.SyncStatus);
                break;
        }
    }
}