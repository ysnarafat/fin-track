using FinTrack.Core.Entities;
using FinTrack.Core.Enums;
using FinTrack.Core.Interfaces;
using Moq;
using System.Linq.Expressions;

namespace FinTrack.Tests.Unit.Repositories;

/// <summary>
/// Unit tests for IRepository interface contract
/// These tests verify that repository implementations follow the expected behavior
/// </summary>
public class IRepositoryTests
{
    private readonly Mock<IRepository<TestEntity>> _mockRepository;

    public IRepositoryTests()
    {
        _mockRepository = new Mock<IRepository<TestEntity>>();
    }

    public class TestEntity : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnEntity()
    {
        // Arrange
        var entity = new TestEntity { Id = 1, Name = "Test" };
        _mockRepository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        // Act
        var result = await _mockRepository.Object.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Test", result.Name);
        _mockRepository.Verify(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        _mockRepository.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TestEntity?)null);

        // Act
        var result = await _mockRepository.Object.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
        _mockRepository.Verify(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetBySyncIdAsync_WithValidSyncId_ShouldReturnEntity()
    {
        // Arrange
        var syncId = Guid.NewGuid().ToString();
        var entity = new TestEntity { Id = 1, SyncId = syncId, Name = "Test" };
        _mockRepository.Setup(x => x.GetBySyncIdAsync(syncId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        // Act
        var result = await _mockRepository.Object.GetBySyncIdAsync(syncId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(syncId, result.SyncId);
        Assert.Equal("Test", result.Name);
        _mockRepository.Verify(x => x.GetBySyncIdAsync(syncId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllNonDeletedEntities()
    {
        // Arrange
        var entities = new List<TestEntity>
        {
            new TestEntity { Id = 1, Name = "Test1", IsDeleted = false },
            new TestEntity { Id = 2, Name = "Test2", IsDeleted = false }
        };
        _mockRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(entities);

        // Act
        var result = await _mockRepository.Object.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, entity => Assert.False(entity.IsDeleted));
        _mockRepository.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetWhereAsync_WithPredicate_ShouldReturnMatchingEntities()
    {
        // Arrange
        var entities = new List<TestEntity>
        {
            new TestEntity { Id = 1, Name = "Test1" },
            new TestEntity { Id = 2, Name = "Other" }
        };
        _mockRepository.Setup(x => x.GetWhereAsync(It.IsAny<Expression<Func<TestEntity, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entities.Where(e => e.Name.StartsWith("Test")));

        // Act
        var result = await _mockRepository.Object.GetWhereAsync(e => e.Name.StartsWith("Test"));

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Test1", result.First().Name);
        _mockRepository.Verify(x => x.GetWhereAsync(It.IsAny<Expression<Func<TestEntity, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddAsync_WithValidEntity_ShouldReturnEntityWithId()
    {
        // Arrange
        var entity = new TestEntity { Name = "New Test" };
        var savedEntity = new TestEntity { Id = 1, Name = "New Test" };
        _mockRepository.Setup(x => x.AddAsync(entity, It.IsAny<CancellationToken>()))
            .ReturnsAsync(savedEntity);

        // Act
        var result = await _mockRepository.Object.AddAsync(entity);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("New Test", result.Name);
        _mockRepository.Verify(x => x.AddAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddRangeAsync_WithMultipleEntities_ShouldReturnAllEntitiesWithIds()
    {
        // Arrange
        var entities = new List<TestEntity>
        {
            new TestEntity { Name = "Test1" },
            new TestEntity { Name = "Test2" }
        };
        var savedEntities = new List<TestEntity>
        {
            new TestEntity { Id = 1, Name = "Test1" },
            new TestEntity { Id = 2, Name = "Test2" }
        };
        _mockRepository.Setup(x => x.AddRangeAsync(entities, It.IsAny<CancellationToken>()))
            .ReturnsAsync(savedEntities);

        // Act
        var result = await _mockRepository.Object.AddRangeAsync(entities);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, entity => Assert.True(entity.Id > 0));
        _mockRepository.Verify(x => x.AddRangeAsync(entities, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithValidEntity_ShouldReturnUpdatedEntity()
    {
        // Arrange
        var entity = new TestEntity { Id = 1, Name = "Updated Test" };
        _mockRepository.Setup(x => x.UpdateAsync(entity, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        // Act
        var result = await _mockRepository.Object.UpdateAsync(entity);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Updated Test", result.Name);
        _mockRepository.Verify(x => x.UpdateAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldReturnTrue()
    {
        // Arrange
        _mockRepository.Setup(x => x.DeleteAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _mockRepository.Object.DeleteAsync(1);

        // Assert
        Assert.True(result);
        _mockRepository.Verify(x => x.DeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Arrange
        _mockRepository.Setup(x => x.DeleteAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _mockRepository.Object.DeleteAsync(999);

        // Assert
        Assert.False(result);
        _mockRepository.Verify(x => x.DeleteAsync(999, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CountAsync_WithoutPredicate_ShouldReturnTotalCount()
    {
        // Arrange
        _mockRepository.Setup(x => x.CountAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        // Act
        var result = await _mockRepository.Object.CountAsync();

        // Assert
        Assert.Equal(5, result);
        _mockRepository.Verify(x => x.CountAsync(null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CountAsync_WithPredicate_ShouldReturnFilteredCount()
    {
        // Arrange
        _mockRepository.Setup(x => x.CountAsync(It.IsAny<Expression<Func<TestEntity, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        // Act
        var result = await _mockRepository.Object.CountAsync(e => e.Name.StartsWith("Test"));

        // Assert
        Assert.Equal(3, result);
        _mockRepository.Verify(x => x.CountAsync(It.IsAny<Expression<Func<TestEntity, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AnyAsync_WithMatchingPredicate_ShouldReturnTrue()
    {
        // Arrange
        _mockRepository.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<TestEntity, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _mockRepository.Object.AnyAsync(e => e.Name == "Test");

        // Assert
        Assert.True(result);
        _mockRepository.Verify(x => x.AnyAsync(It.IsAny<Expression<Func<TestEntity, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPagedAsync_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        var entities = new List<TestEntity>
        {
            new TestEntity { Id = 3, Name = "Test3" },
            new TestEntity { Id = 4, Name = "Test4" }
        };
        _mockRepository.Setup(x => x.GetPagedAsync(2, 2, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entities);

        // Act
        var result = await _mockRepository.Object.GetPagedAsync(2, 2);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        _mockRepository.Verify(x => x.GetPagedAsync(2, 2, null, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPendingSyncAsync_ShouldReturnEntitiesNeedingSync()
    {
        // Arrange
        var entities = new List<TestEntity>
        {
            new TestEntity { Id = 1, Name = "Test1", SyncStatus = SyncStatus.PendingCreate },
            new TestEntity { Id = 2, Name = "Test2", SyncStatus = SyncStatus.PendingUpdate }
        };
        _mockRepository.Setup(x => x.GetPendingSyncAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(entities);

        // Act
        var result = await _mockRepository.Object.GetPendingSyncAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, entity => Assert.NotEqual(SyncStatus.Synced, entity.SyncStatus));
        _mockRepository.Verify(x => x.GetPendingSyncAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetBySyncStatusAsync_WithSpecificStatus_ShouldReturnMatchingEntities()
    {
        // Arrange
        var entities = new List<TestEntity>
        {
            new TestEntity { Id = 1, Name = "Test1", SyncStatus = SyncStatus.Conflict }
        };
        _mockRepository.Setup(x => x.GetBySyncStatusAsync(SyncStatus.Conflict, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entities);

        // Act
        var result = await _mockRepository.Object.GetBySyncStatusAsync(SyncStatus.Conflict);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(SyncStatus.Conflict, result.First().SyncStatus);
        _mockRepository.Verify(x => x.GetBySyncStatusAsync(SyncStatus.Conflict, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetModifiedAfterAsync_WithTimestamp_ShouldReturnRecentlyModifiedEntities()
    {
        // Arrange
        var timestamp = DateTime.UtcNow.AddHours(-1);
        var entities = new List<TestEntity>
        {
            new TestEntity { Id = 1, Name = "Test1", UpdatedAt = DateTime.UtcNow.AddMinutes(-30) }
        };
        _mockRepository.Setup(x => x.GetModifiedAfterAsync(timestamp, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entities);

        // Act
        var result = await _mockRepository.Object.GetModifiedAfterAsync(timestamp);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.True(result.First().UpdatedAt > timestamp);
        _mockRepository.Verify(x => x.GetModifiedAfterAsync(timestamp, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkAsSyncedAsync_WithSyncIds_ShouldReturnUpdatedCount()
    {
        // Arrange
        var syncIds = new[] { "sync1", "sync2", "sync3" };
        _mockRepository.Setup(x => x.MarkAsSyncedAsync(syncIds, It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        // Act
        var result = await _mockRepository.Object.MarkAsSyncedAsync(syncIds);

        // Assert
        Assert.Equal(3, result);
        _mockRepository.Verify(x => x.MarkAsSyncedAsync(syncIds, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkAsConflictedAsync_WithSyncIds_ShouldReturnUpdatedCount()
    {
        // Arrange
        var syncIds = new[] { "sync1", "sync2" };
        _mockRepository.Setup(x => x.MarkAsConflictedAsync(syncIds, It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        // Act
        var result = await _mockRepository.Object.MarkAsConflictedAsync(syncIds);

        // Assert
        Assert.Equal(2, result);
        _mockRepository.Verify(x => x.MarkAsConflictedAsync(syncIds, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HardDeleteAsync_WithValidId_ShouldReturnTrue()
    {
        // Arrange
        _mockRepository.Setup(x => x.HardDeleteAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _mockRepository.Object.HardDeleteAsync(1);

        // Assert
        Assert.True(result);
        _mockRepository.Verify(x => x.HardDeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetSingleAsync_WithMatchingPredicate_ShouldReturnEntity()
    {
        // Arrange
        var entity = new TestEntity { Id = 1, Name = "Unique" };
        _mockRepository.Setup(x => x.GetSingleAsync(It.IsAny<Expression<Func<TestEntity, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        // Act
        var result = await _mockRepository.Object.GetSingleAsync(e => e.Name == "Unique");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Unique", result.Name);
        _mockRepository.Verify(x => x.GetSingleAsync(It.IsAny<Expression<Func<TestEntity, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithEntity_ShouldReturnTrue()
    {
        // Arrange
        var entity = new TestEntity { Id = 1, Name = "Test" };
        _mockRepository.Setup(x => x.DeleteAsync(entity, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _mockRepository.Object.DeleteAsync(entity);

        // Assert
        Assert.True(result);
        _mockRepository.Verify(x => x.DeleteAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateRangeAsync_WithMultipleEntities_ShouldReturnUpdatedEntities()
    {
        // Arrange
        var entities = new List<TestEntity>
        {
            new TestEntity { Id = 1, Name = "Updated1" },
            new TestEntity { Id = 2, Name = "Updated2" }
        };
        _mockRepository.Setup(x => x.UpdateRangeAsync(entities, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entities);

        // Act
        var result = await _mockRepository.Object.UpdateRangeAsync(entities);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        _mockRepository.Verify(x => x.UpdateRangeAsync(entities, It.IsAny<CancellationToken>()), Times.Once);
    }
}