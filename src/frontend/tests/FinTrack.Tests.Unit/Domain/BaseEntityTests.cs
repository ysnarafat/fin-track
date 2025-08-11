using FinTrack.Core.Entities;
using FinTrack.Core.Enums;
using Xunit;

namespace FinTrack.Tests.Unit.Domain;

/// <summary>
/// Unit tests for BaseEntity class
/// </summary>
public class BaseEntityTests
{
    /// <summary>
    /// Test implementation of BaseEntity for testing purposes
    /// </summary>
    private class TestEntity : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
    }

    [Fact]
    public void Constructor_ShouldInitializeDefaultValues()
    {
        // Act
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
    public void MarkAsModified_ShouldUpdateTimestampAndVersion()
    {
        // Arrange
        var entity = new TestEntity();
        var originalUpdatedAt = entity.UpdatedAt;
        var originalVersion = entity.Version;

        // Act
        Thread.Sleep(1); // Ensure time difference
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
    public void MarkAsModified_WhenNotSynced_ShouldKeepCurrentStatus()
    {
        // Arrange
        var entity = new TestEntity(); // Starts as PendingCreate

        // Act
        entity.MarkAsModified();

        // Assert
        Assert.Equal(SyncStatus.PendingCreate, entity.SyncStatus);
    }

    [Fact]
    public void MarkAsDeleted_ShouldSetDeletedFlagAndUpdateStatus()
    {
        // Arrange
        var entity = new TestEntity();
        var originalVersion = entity.Version;

        // Act
        entity.MarkAsDeleted();

        // Assert
        Assert.True(entity.IsDeleted);
        Assert.Equal(SyncStatus.PendingDelete, entity.SyncStatus);
        Assert.Equal(originalVersion + 1, entity.Version);
    }

    [Fact]
    public void MarkAsSynced_ShouldSetSyncedStatusAndTimestamp()
    {
        // Arrange
        var entity = new TestEntity();

        // Act
        entity.MarkAsSynced();

        // Assert
        Assert.Equal(SyncStatus.Synced, entity.SyncStatus);
        Assert.NotNull(entity.LastSyncAt);
        Assert.True(entity.LastSyncAt <= DateTime.UtcNow);
    }

    [Fact]
    public void MarkAsConflicted_ShouldSetConflictStatus()
    {
        // Arrange
        var entity = new TestEntity();

        // Act
        entity.MarkAsConflicted();

        // Assert
        Assert.Equal(SyncStatus.Conflict, entity.SyncStatus);
    }

    [Fact]
    public void MarkAsSyncFailed_ShouldSetFailedStatus()
    {
        // Arrange
        var entity = new TestEntity();

        // Act
        entity.MarkAsSyncFailed();

        // Assert
        Assert.Equal(SyncStatus.SyncFailed, entity.SyncStatus);
    }
}