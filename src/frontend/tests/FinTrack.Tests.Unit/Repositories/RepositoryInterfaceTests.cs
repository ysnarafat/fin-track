using FinTrack.Core.Entities;
using FinTrack.Core.Enums;
using FinTrack.Core.Interfaces;
using FinTrack.Tests.Unit.Helpers;
using Moq;
using System.Linq.Expressions;

namespace FinTrack.Tests.Unit.Repositories;

/// <summary>
/// Unit tests for IRepository interface implementations
/// These tests verify the contract behavior that any repository implementation should follow
/// </summary>
public class RepositoryInterfaceTests
{
    private readonly Mock<IRepository<Transaction>> _mockRepository;

    public RepositoryInterfaceTests()
    {
        _mockRepository = new Mock<IRepository<Transaction>>();
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnEntity()
    {
        // Arrange
        var transaction = TestDataBuilder.Transaction()
            .WithId(1)
            .WithDescription("Test Transaction")
            .Build();

        _mockRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);

        // Act
        var result = await _mockRepository.Object.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Test Transaction", result.Description);
        _mockRepository.Verify(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Transaction?)null);

        // Act
        var result = await _mockRepository.Object.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
        _mockRepository.Verify(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetBySyncIdAsync_WithValidSyncId_ShouldReturnEntity()
    {
        // Arrange
        var syncId = Guid.NewGuid().ToString();
        var transaction = TestDataBuilder.Transaction()
            .WithId(1)
            .WithDescription("Synced Transaction")
            .Build();
        transaction.SyncId = syncId;

        _mockRepository.Setup(r => r.GetBySyncIdAsync(syncId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);

        // Act
        var result = await _mockRepository.Object.GetBySyncIdAsync(syncId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(syncId, result.SyncId);
        _mockRepository.Verify(r => r.GetBySyncIdAsync(syncId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllNonDeletedEntities()
    {
        // Arrange
        var transactions = new List<Transaction>
        {
            TestDataBuilder.Transaction().WithId(1).WithDescription("Transaction 1").Build(),
            TestDataBuilder.Transaction().WithId(2).WithDescription("Transaction 2").Build(),
            TestDataBuilder.Transaction().WithId(3).WithDescription("Transaction 3").Build()
        };

        _mockRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactions);

        // Act
        var result = await _mockRepository.Object.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
        _mockRepository.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetWhereAsync_WithPredicate_ShouldReturnMatchingEntities()
    {
        // Arrange
        var transactions = new List<Transaction>
        {
            TestDataBuilder.Transaction().WithId(1).WithAmount(100m).Build(),
            TestDataBuilder.Transaction().WithId(2).WithAmount(200m).Build()
        };

        Expression<Func<Transaction, bool>> predicate = t => t.Amount > 150m;

        _mockRepository.Setup(r => r.GetWhereAsync(It.IsAny<Expression<Func<Transaction, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactions.Where(predicate.Compile()));

        // Act
        var result = await _mockRepository.Object.GetWhereAsync(predicate);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(200m, result.First().Amount);
        _mockRepository.Verify(r => r.GetWhereAsync(It.IsAny<Expression<Func<Transaction, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetSingleAsync_WithMatchingPredicate_ShouldReturnSingleEntity()
    {
        // Arrange
        var transaction = TestDataBuilder.Transaction()
            .WithId(1)
            .WithDescription("Unique Transaction")
            .Build();

        Expression<Func<Transaction, bool>> predicate = t => t.Description == "Unique Transaction";

        _mockRepository.Setup(r => r.GetSingleAsync(It.IsAny<Expression<Func<Transaction, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);

        // Act
        var result = await _mockRepository.Object.GetSingleAsync(predicate);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Unique Transaction", result.Description);
        _mockRepository.Verify(r => r.GetSingleAsync(It.IsAny<Expression<Func<Transaction, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddAsync_WithValidEntity_ShouldReturnEntityWithId()
    {
        // Arrange
        var transaction = TestDataBuilder.Transaction()
            .WithDescription("New Transaction")
            .WithAmount(100m)
            .Build();

        var savedTransaction = TestDataBuilder.Transaction()
            .WithId(1)
            .WithDescription("New Transaction")
            .WithAmount(100m)
            .Build();

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(savedTransaction);

        // Act
        var result = await _mockRepository.Object.AddAsync(transaction);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("New Transaction", result.Description);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddRangeAsync_WithMultipleEntities_ShouldReturnAllEntitiesWithIds()
    {
        // Arrange
        var transactions = new List<Transaction>
        {
            TestDataBuilder.Transaction().WithDescription("Transaction 1").Build(),
            TestDataBuilder.Transaction().WithDescription("Transaction 2").Build()
        };

        var savedTransactions = new List<Transaction>
        {
            TestDataBuilder.Transaction().WithId(1).WithDescription("Transaction 1").Build(),
            TestDataBuilder.Transaction().WithId(2).WithDescription("Transaction 2").Build()
        };

        _mockRepository.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<Transaction>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(savedTransactions);

        // Act
        var result = await _mockRepository.Object.AddRangeAsync(transactions);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, t => Assert.True(t.Id > 0));
        _mockRepository.Verify(r => r.AddRangeAsync(It.IsAny<IEnumerable<Transaction>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithValidEntity_ShouldReturnUpdatedEntity()
    {
        // Arrange
        var transaction = TestDataBuilder.Transaction()
            .WithId(1)
            .WithDescription("Updated Transaction")
            .WithAmount(150m)
            .Build();

        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);

        // Act
        var result = await _mockRepository.Object.UpdateAsync(transaction);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Updated Transaction", result.Description);
        Assert.Equal(150m, result.Amount);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateRangeAsync_WithMultipleEntities_ShouldReturnAllUpdatedEntities()
    {
        // Arrange
        var transactions = new List<Transaction>
        {
            TestDataBuilder.Transaction().WithId(1).WithDescription("Updated 1").Build(),
            TestDataBuilder.Transaction().WithId(2).WithDescription("Updated 2").Build()
        };

        _mockRepository.Setup(r => r.UpdateRangeAsync(It.IsAny<IEnumerable<Transaction>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactions);

        // Act
        var result = await _mockRepository.Object.UpdateRangeAsync(transactions);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        _mockRepository.Verify(r => r.UpdateRangeAsync(It.IsAny<IEnumerable<Transaction>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldReturnTrue()
    {
        // Arrange
        _mockRepository.Setup(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _mockRepository.Object.DeleteAsync(1);

        // Assert
        Assert.True(result);
        _mockRepository.Verify(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Arrange
        _mockRepository.Setup(r => r.DeleteAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _mockRepository.Object.DeleteAsync(999);

        // Assert
        Assert.False(result);
        _mockRepository.Verify(r => r.DeleteAsync(999, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithEntity_ShouldReturnTrue()
    {
        // Arrange
        var transaction = TestDataBuilder.Transaction().WithId(1).Build();

        _mockRepository.Setup(r => r.DeleteAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _mockRepository.Object.DeleteAsync(transaction);

        // Assert
        Assert.True(result);
        _mockRepository.Verify(r => r.DeleteAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HardDeleteAsync_WithValidId_ShouldReturnTrue()
    {
        // Arrange
        _mockRepository.Setup(r => r.HardDeleteAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _mockRepository.Object.HardDeleteAsync(1);

        // Assert
        Assert.True(result);
        _mockRepository.Verify(r => r.HardDeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CountAsync_WithoutPredicate_ShouldReturnTotalCount()
    {
        // Arrange
        _mockRepository.Setup(r => r.CountAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        // Act
        var result = await _mockRepository.Object.CountAsync();

        // Assert
        Assert.Equal(5, result);
        _mockRepository.Verify(r => r.CountAsync(null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CountAsync_WithPredicate_ShouldReturnFilteredCount()
    {
        // Arrange
        Expression<Func<Transaction, bool>> predicate = t => t.Amount > 100m;

        _mockRepository.Setup(r => r.CountAsync(It.IsAny<Expression<Func<Transaction, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        // Act
        var result = await _mockRepository.Object.CountAsync(predicate);

        // Assert
        Assert.Equal(3, result);
        _mockRepository.Verify(r => r.CountAsync(It.IsAny<Expression<Func<Transaction, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AnyAsync_WithMatchingPredicate_ShouldReturnTrue()
    {
        // Arrange
        Expression<Func<Transaction, bool>> predicate = t => t.Amount > 100m;

        _mockRepository.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Transaction, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _mockRepository.Object.AnyAsync(predicate);

        // Assert
        Assert.True(result);
        _mockRepository.Verify(r => r.AnyAsync(It.IsAny<Expression<Func<Transaction, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AnyAsync_WithNonMatchingPredicate_ShouldReturnFalse()
    {
        // Arrange
        Expression<Func<Transaction, bool>> predicate = t => t.Amount > 10000m;

        _mockRepository.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Transaction, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _mockRepository.Object.AnyAsync(predicate);

        // Assert
        Assert.False(result);
        _mockRepository.Verify(r => r.AnyAsync(It.IsAny<Expression<Func<Transaction, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPagedAsync_WithValidParameters_ShouldReturnPagedResults()
    {
        // Arrange
        var transactions = new List<Transaction>
        {
            TestDataBuilder.Transaction().WithId(3).WithAmount(300m).Build(),
            TestDataBuilder.Transaction().WithId(4).WithAmount(400m).Build()
        };

        _mockRepository.Setup(r => r.GetPagedAsync(
            It.IsAny<int>(), 
            It.IsAny<int>(), 
            It.IsAny<Expression<Func<Transaction, bool>>>(), 
            It.IsAny<Expression<Func<Transaction, object>>>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactions);

        // Act
        var result = await _mockRepository.Object.GetPagedAsync(2, 2);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        _mockRepository.Verify(r => r.GetPagedAsync(2, 2, null, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPendingSyncAsync_ShouldReturnEntitiesNeedingSync()
    {
        // Arrange
        var pendingTransactions = new List<Transaction>
        {
            TestDataBuilder.Transaction().WithId(1).WithSyncStatus(SyncStatus.PendingCreate).Build(),
            TestDataBuilder.Transaction().WithId(2).WithSyncStatus(SyncStatus.PendingUpdate).Build()
        };

        _mockRepository.Setup(r => r.GetPendingSyncAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(pendingTransactions);

        // Act
        var result = await _mockRepository.Object.GetPendingSyncAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, t => Assert.True(t.SyncStatus != SyncStatus.Synced));
        _mockRepository.Verify(r => r.GetPendingSyncAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetBySyncStatusAsync_WithSpecificStatus_ShouldReturnMatchingEntities()
    {
        // Arrange
        var conflictedTransactions = new List<Transaction>
        {
            TestDataBuilder.Transaction().WithId(1).WithSyncStatus(SyncStatus.Conflict).Build()
        };

        _mockRepository.Setup(r => r.GetBySyncStatusAsync(SyncStatus.Conflict, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conflictedTransactions);

        // Act
        var result = await _mockRepository.Object.GetBySyncStatusAsync(SyncStatus.Conflict);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(SyncStatus.Conflict, result.First().SyncStatus);
        _mockRepository.Verify(r => r.GetBySyncStatusAsync(SyncStatus.Conflict, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetModifiedAfterAsync_WithTimestamp_ShouldReturnRecentlyModifiedEntities()
    {
        // Arrange
        var timestamp = DateTime.UtcNow.AddHours(-1);
        var recentTransactions = new List<Transaction>
        {
            TestDataBuilder.Transaction().WithId(1).Build(),
            TestDataBuilder.Transaction().WithId(2).Build()
        };

        // Set UpdatedAt to recent time
        foreach (var transaction in recentTransactions)
        {
            transaction.UpdatedAt = DateTime.UtcNow.AddMinutes(-30);
        }

        _mockRepository.Setup(r => r.GetModifiedAfterAsync(timestamp, It.IsAny<CancellationToken>()))
            .ReturnsAsync(recentTransactions);

        // Act
        var result = await _mockRepository.Object.GetModifiedAfterAsync(timestamp);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        _mockRepository.Verify(r => r.GetModifiedAfterAsync(timestamp, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkAsSyncedAsync_WithSyncIds_ShouldReturnUpdatedCount()
    {
        // Arrange
        var syncIds = new[] { "sync1", "sync2", "sync3" };

        _mockRepository.Setup(r => r.MarkAsSyncedAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        // Act
        var result = await _mockRepository.Object.MarkAsSyncedAsync(syncIds);

        // Assert
        Assert.Equal(3, result);
        _mockRepository.Verify(r => r.MarkAsSyncedAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkAsConflictedAsync_WithSyncIds_ShouldReturnUpdatedCount()
    {
        // Arrange
        var syncIds = new[] { "sync1", "sync2" };

        _mockRepository.Setup(r => r.MarkAsConflictedAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        // Act
        var result = await _mockRepository.Object.MarkAsConflictedAsync(syncIds);

        // Assert
        Assert.Equal(2, result);
        _mockRepository.Verify(r => r.MarkAsConflictedAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Repository_ShouldSupportCancellationToken()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        _mockRepository.Setup(r => r.GetAllAsync(cancellationToken))
            .ReturnsAsync(new List<Transaction>());

        // Act
        var result = await _mockRepository.Object.GetAllAsync(cancellationToken);

        // Assert
        Assert.NotNull(result);
        _mockRepository.Verify(r => r.GetAllAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Repository_WithCancelledToken_ShouldThrowOperationCancelledException()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();
        var cancellationToken = cancellationTokenSource.Token;

        _mockRepository.Setup(r => r.GetAllAsync(cancellationToken))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _mockRepository.Object.GetAllAsync(cancellationToken));
    }
}