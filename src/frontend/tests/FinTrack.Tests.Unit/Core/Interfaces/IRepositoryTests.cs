using System.Linq.Expressions;
using FinTrack.Core.Entities;
using FinTrack.Core.Enums;
using FinTrack.Core.Interfaces;
using FinTrack.Tests.Unit.Helpers;
using Moq;

namespace FinTrack.Tests.Unit.Core.Interfaces;

/// <summary>
/// Unit tests for IRepository interface contract and behavior expectations
/// </summary>
public class IRepositoryTests
{
    private readonly Mock<IRepository<Transaction>> _mockRepository;

    public IRepositoryTests()
    {
        _mockRepository = new Mock<IRepository<Transaction>>();
    }

    #region Basic CRUD Operations Tests

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnEntity()
    {
        // Arrange
        var transaction = TestDataBuilder.CreateTransaction();
        transaction.Id = 1;
        
        _mockRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(transaction);

        // Act
        var result = await _mockRepository.Object.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
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
        var transaction = TestDataBuilder.CreateTransaction();
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
    public async Task GetBySyncIdAsync_WithInvalidSyncId_ShouldReturnNull()
    {
        // Arrange
        var invalidSyncId = "invalid-sync-id";
        _mockRepository.Setup(r => r.GetBySyncIdAsync(invalidSyncId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync((Transaction?)null);

        // Act
        var result = await _mockRepository.Object.GetBySyncIdAsync(invalidSyncId);

        // Assert
        Assert.Null(result);
        _mockRepository.Verify(r => r.GetBySyncIdAsync(invalidSyncId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllEntities()
    {
        // Arrange
        var transactions = new List<Transaction>
        {
            TestDataBuilder.CreateTransaction(description: "Transaction 1"),
            TestDataBuilder.CreateTransaction(description: "Transaction 2"),
            TestDataBuilder.CreateTransaction(description: "Transaction 3")
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
    public async Task AddAsync_WithValidEntity_ShouldReturnAddedEntity()
    {
        // Arrange
        var transaction = TestDataBuilder.CreateTransaction();
        var addedTransaction = TestDataBuilder.CreateTransaction();
        addedTransaction.Id = 1;
        addedTransaction.SyncId = Guid.NewGuid().ToString();
        addedTransaction.SyncStatus = SyncStatus.PendingCreate;
        
        _mockRepository.Setup(r => r.AddAsync(transaction, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(addedTransaction);

        // Act
        var result = await _mockRepository.Object.AddAsync(transaction);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.NotNull(result.SyncId);
        Assert.Equal(SyncStatus.PendingCreate, result.SyncStatus);
        _mockRepository.Verify(r => r.AddAsync(transaction, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddRangeAsync_WithValidEntities_ShouldReturnAddedEntities()
    {
        // Arrange
        var transactions = new List<Transaction>
        {
            TestDataBuilder.CreateTransaction(description: "Transaction 1"),
            TestDataBuilder.CreateTransaction(description: "Transaction 2")
        };
        
        var addedTransactions = transactions.Select((t, index) =>
        {
            var added = TestDataBuilder.CreateTransaction(description: t.Description);
            added.Id = index + 1;
            added.SyncId = Guid.NewGuid().ToString();
            added.SyncStatus = SyncStatus.PendingCreate;
            return added;
        }).ToList();
        
        _mockRepository.Setup(r => r.AddRangeAsync(transactions, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(addedTransactions);

        // Act
        var result = await _mockRepository.Object.AddRangeAsync(transactions);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, t => Assert.True(t.Id > 0));
        Assert.All(result, t => Assert.NotNull(t.SyncId));
        Assert.All(result, t => Assert.Equal(SyncStatus.PendingCreate, t.SyncStatus));
        _mockRepository.Verify(r => r.AddRangeAsync(transactions, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithValidEntity_ShouldReturnUpdatedEntity()
    {
        // Arrange
        var transaction = TestDataBuilder.CreateTransaction();
        transaction.Id = 1;
        transaction.Version = 1;
        transaction.Description = "Updated Description";
        
        var updatedTransaction = TestDataBuilder.CreateTransaction();
        updatedTransaction.Id = 1;
        updatedTransaction.Version = 2;
        updatedTransaction.Description = "Updated Description";
        updatedTransaction.SyncStatus = SyncStatus.PendingUpdate;
        
        _mockRepository.Setup(r => r.UpdateAsync(transaction, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(updatedTransaction);

        // Act
        var result = await _mockRepository.Object.UpdateAsync(transaction);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Description", result.Description);
        Assert.Equal(2, result.Version);
        Assert.Equal(SyncStatus.PendingUpdate, result.SyncStatus);
        _mockRepository.Verify(r => r.UpdateAsync(transaction, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateRangeAsync_WithValidEntities_ShouldReturnUpdatedEntities()
    {
        // Arrange
        var transactions = new List<Transaction>
        {
            TestDataBuilder.CreateTransaction(description: "Transaction 1"),
            TestDataBuilder.CreateTransaction(description: "Transaction 2")
        };
        
        foreach (var (transaction, index) in transactions.Select((t, i) => (t, i)))
        {
            transaction.Id = index + 1;
            transaction.Version = 1;
        }
        
        var updatedTransactions = transactions.Select(t =>
        {
            var updated = TestDataBuilder.CreateTransaction(description: t.Description);
            updated.Id = t.Id;
            updated.Version = 2;
            updated.SyncStatus = SyncStatus.PendingUpdate;
            return updated;
        }).ToList();
        
        _mockRepository.Setup(r => r.UpdateRangeAsync(transactions, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(updatedTransactions);

        // Act
        var result = await _mockRepository.Object.UpdateRangeAsync(transactions);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, t => Assert.Equal(2, t.Version));
        Assert.All(result, t => Assert.Equal(SyncStatus.PendingUpdate, t.SyncStatus));
        _mockRepository.Verify(r => r.UpdateRangeAsync(transactions, It.IsAny<CancellationToken>()), Times.Once);
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
        var transaction = TestDataBuilder.CreateTransaction();
        transaction.Id = 1;
        
        _mockRepository.Setup(r => r.DeleteAsync(transaction, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(true);

        // Act
        var result = await _mockRepository.Object.DeleteAsync(transaction);

        // Assert
        Assert.True(result);
        _mockRepository.Verify(r => r.DeleteAsync(transaction, It.IsAny<CancellationToken>()), Times.Once);
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

    #endregion

    #region Query Operations Tests

    [Fact]
    public async Task GetWhereAsync_WithPredicate_ShouldReturnMatchingEntities()
    {
        // Arrange
        var transactions = new List<Transaction>
        {
            TestDataBuilder.CreateTransaction(amount: 100m),
            TestDataBuilder.CreateTransaction(amount: 200m)
        };
        
        Expression<Func<Transaction, bool>> predicate = t => t.Amount >= 150m;
        
        _mockRepository.Setup(r => r.GetWhereAsync(It.IsAny<Expression<Func<Transaction, bool>>>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(transactions.Where(t => t.Amount >= 150m));

        // Act
        var result = await _mockRepository.Object.GetWhereAsync(predicate);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.All(result, t => Assert.True(t.Amount >= 150m));
        _mockRepository.Verify(r => r.GetWhereAsync(It.IsAny<Expression<Func<Transaction, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetSingleAsync_WithMatchingPredicate_ShouldReturnEntity()
    {
        // Arrange
        var transaction = TestDataBuilder.CreateTransaction(amount: 150m);
        Expression<Func<Transaction, bool>> predicate = t => t.Amount == 150m;
        
        _mockRepository.Setup(r => r.GetSingleAsync(It.IsAny<Expression<Func<Transaction, bool>>>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(transaction);

        // Act
        var result = await _mockRepository.Object.GetSingleAsync(predicate);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(150m, result.Amount);
        _mockRepository.Verify(r => r.GetSingleAsync(It.IsAny<Expression<Func<Transaction, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetSingleAsync_WithNonMatchingPredicate_ShouldReturnNull()
    {
        // Arrange
        Expression<Func<Transaction, bool>> predicate = t => t.Amount == 999m;
        
        _mockRepository.Setup(r => r.GetSingleAsync(It.IsAny<Expression<Func<Transaction, bool>>>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync((Transaction?)null);

        // Act
        var result = await _mockRepository.Object.GetSingleAsync(predicate);

        // Assert
        Assert.Null(result);
        _mockRepository.Verify(r => r.GetSingleAsync(It.IsAny<Expression<Func<Transaction, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
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
        Expression<Func<Transaction, bool>> predicate = t => t.Type == TransactionType.Income;
        
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
        Expression<Func<Transaction, bool>> predicate = t => t.Type == TransactionType.Income;
        
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
            TestDataBuilder.CreateTransaction(description: "Transaction 1"),
            TestDataBuilder.CreateTransaction(description: "Transaction 2"),
            TestDataBuilder.CreateTransaction(description: "Transaction 3")
        };
        
        _mockRepository.Setup(r => r.GetPagedAsync(
                It.IsAny<int>(), 
                It.IsAny<int>(), 
                It.IsAny<Expression<Func<Transaction, bool>>>(), 
                It.IsAny<Expression<Func<Transaction, object>>>(), 
                It.IsAny<CancellationToken>()))
                      .ReturnsAsync(transactions.Take(2));

        // Act
        var result = await _mockRepository.Object.GetPagedAsync(0, 2);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        _mockRepository.Verify(r => r.GetPagedAsync(
            0, 
            2, 
            It.IsAny<Expression<Func<Transaction, bool>>>(), 
            It.IsAny<Expression<Func<Transaction, object>>>(), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Sync-related Operations Tests

    [Fact]
    public async Task GetPendingSyncAsync_ShouldReturnPendingEntities()
    {
        // Arrange
        var pendingTransactions = new List<Transaction>
        {
            TestDataBuilder.CreateTransaction(description: "Pending Create"),
            TestDataBuilder.CreateTransaction(description: "Pending Update")
        };
        
        pendingTransactions[0].SyncStatus = SyncStatus.PendingCreate;
        pendingTransactions[1].SyncStatus = SyncStatus.PendingUpdate;
        
        _mockRepository.Setup(r => r.GetPendingSyncAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(pendingTransactions);

        // Act
        var result = await _mockRepository.Object.GetPendingSyncAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, t => Assert.NotEqual(SyncStatus.Synced, t.SyncStatus));
        _mockRepository.Verify(r => r.GetPendingSyncAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetBySyncStatusAsync_WithSpecificStatus_ShouldReturnMatchingEntities()
    {
        // Arrange
        var syncedTransactions = new List<Transaction>
        {
            TestDataBuilder.CreateTransaction(description: "Synced 1"),
            TestDataBuilder.CreateTransaction(description: "Synced 2")
        };
        
        foreach (var transaction in syncedTransactions)
        {
            transaction.SyncStatus = SyncStatus.Synced;
        }
        
        _mockRepository.Setup(r => r.GetBySyncStatusAsync(SyncStatus.Synced, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(syncedTransactions);

        // Act
        var result = await _mockRepository.Object.GetBySyncStatusAsync(SyncStatus.Synced);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, t => Assert.Equal(SyncStatus.Synced, t.SyncStatus));
        _mockRepository.Verify(r => r.GetBySyncStatusAsync(SyncStatus.Synced, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetModifiedAfterAsync_WithTimestamp_ShouldReturnRecentEntities()
    {
        // Arrange
        var timestamp = DateTime.UtcNow.AddHours(-1);
        var recentTransactions = new List<Transaction>
        {
            TestDataBuilder.CreateTransaction(description: "Recent Transaction")
        };
        
        _mockRepository.Setup(r => r.GetModifiedAfterAsync(timestamp, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(recentTransactions);

        // Act
        var result = await _mockRepository.Object.GetModifiedAfterAsync(timestamp);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        _mockRepository.Verify(r => r.GetModifiedAfterAsync(timestamp, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkAsSyncedAsync_WithSyncIds_ShouldReturnUpdatedCount()
    {
        // Arrange
        var syncIds = new[] { "sync-id-1", "sync-id-2" };
        
        _mockRepository.Setup(r => r.MarkAsSyncedAsync(syncIds, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(2);

        // Act
        var result = await _mockRepository.Object.MarkAsSyncedAsync(syncIds);

        // Assert
        Assert.Equal(2, result);
        _mockRepository.Verify(r => r.MarkAsSyncedAsync(syncIds, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkAsConflictedAsync_WithSyncIds_ShouldReturnUpdatedCount()
    {
        // Arrange
        var syncIds = new[] { "sync-id-1", "sync-id-2" };
        
        _mockRepository.Setup(r => r.MarkAsConflictedAsync(syncIds, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(2);

        // Act
        var result = await _mockRepository.Object.MarkAsConflictedAsync(syncIds);

        // Assert
        Assert.Equal(2, result);
        _mockRepository.Verify(r => r.MarkAsConflictedAsync(syncIds, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Save Operations Tests

    [Fact]
    public async Task SaveChangesAsync_ShouldReturnChangesCount()
    {
        // Arrange
        _mockRepository.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(3);

        // Act
        var result = await _mockRepository.Object.SaveChangesAsync();

        // Assert
        Assert.Equal(3, result);
        _mockRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task GetByIdAsync_WithException_ShouldPropagateException()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                      .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _mockRepository.Object.GetByIdAsync(1));
        
        Assert.Equal("Database error", exception.Message);
    }

    [Fact]
    public async Task AddAsync_WithNullEntity_ShouldThrowArgumentNullException()
    {
        // Arrange
        _mockRepository.Setup(r => r.AddAsync(null!, It.IsAny<CancellationToken>()))
                      .ThrowsAsync(new ArgumentNullException("entity"));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _mockRepository.Object.AddAsync(null!));
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task GetByIdAsync_WithCancellationToken_ShouldPassTokenToImplementation()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var transaction = TestDataBuilder.CreateTransaction();
        
        _mockRepository.Setup(r => r.GetByIdAsync(1, cancellationToken))
                      .ReturnsAsync(transaction);

        // Act
        var result = await _mockRepository.Object.GetByIdAsync(1, cancellationToken);

        // Assert
        Assert.NotNull(result);
        _mockRepository.Verify(r => r.GetByIdAsync(1, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task AddAsync_WithCancellationToken_ShouldPassTokenToImplementation()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var transaction = TestDataBuilder.CreateTransaction();
        var addedTransaction = TestDataBuilder.CreateTransaction();
        addedTransaction.Id = 1;
        
        _mockRepository.Setup(r => r.AddAsync(transaction, cancellationToken))
                      .ReturnsAsync(addedTransaction);

        // Act
        var result = await _mockRepository.Object.AddAsync(transaction, cancellationToken);

        // Assert
        Assert.NotNull(result);
        _mockRepository.Verify(r => r.AddAsync(transaction, cancellationToken), Times.Once);
    }

    #endregion

    #region Contract Verification Tests

    [Fact]
    public void IRepository_ShouldHaveAllRequiredMethods()
    {
        // Arrange
        var repositoryType = typeof(IRepository<Transaction>);

        // Act & Assert - Verify all expected methods exist
        Assert.NotNull(repositoryType.GetMethod(nameof(IRepository<Transaction>.GetByIdAsync)));
        Assert.NotNull(repositoryType.GetMethod(nameof(IRepository<Transaction>.GetBySyncIdAsync)));
        Assert.NotNull(repositoryType.GetMethod(nameof(IRepository<Transaction>.GetAllAsync)));
        Assert.NotNull(repositoryType.GetMethod(nameof(IRepository<Transaction>.GetWhereAsync)));
        Assert.NotNull(repositoryType.GetMethod(nameof(IRepository<Transaction>.GetSingleAsync)));
        Assert.NotNull(repositoryType.GetMethod(nameof(IRepository<Transaction>.AddAsync)));
        Assert.NotNull(repositoryType.GetMethod(nameof(IRepository<Transaction>.AddRangeAsync)));
        Assert.NotNull(repositoryType.GetMethod(nameof(IRepository<Transaction>.UpdateAsync)));
        Assert.NotNull(repositoryType.GetMethod(nameof(IRepository<Transaction>.UpdateRangeAsync)));
        Assert.NotNull(repositoryType.GetMethod(nameof(IRepository<Transaction>.DeleteAsync), new[] { typeof(int), typeof(CancellationToken) }));
        Assert.NotNull(repositoryType.GetMethod(nameof(IRepository<Transaction>.DeleteAsync), new[] { typeof(Transaction), typeof(CancellationToken) }));
        Assert.NotNull(repositoryType.GetMethod(nameof(IRepository<Transaction>.HardDeleteAsync)));
        Assert.NotNull(repositoryType.GetMethod(nameof(IRepository<Transaction>.CountAsync)));
        Assert.NotNull(repositoryType.GetMethod(nameof(IRepository<Transaction>.AnyAsync)));
        Assert.NotNull(repositoryType.GetMethod(nameof(IRepository<Transaction>.GetPagedAsync)));
        Assert.NotNull(repositoryType.GetMethod(nameof(IRepository<Transaction>.GetPendingSyncAsync)));
        Assert.NotNull(repositoryType.GetMethod(nameof(IRepository<Transaction>.GetBySyncStatusAsync)));
        Assert.NotNull(repositoryType.GetMethod(nameof(IRepository<Transaction>.GetModifiedAfterAsync)));
        Assert.NotNull(repositoryType.GetMethod(nameof(IRepository<Transaction>.MarkAsSyncedAsync)));
        Assert.NotNull(repositoryType.GetMethod(nameof(IRepository<Transaction>.MarkAsConflictedAsync)));
        Assert.NotNull(repositoryType.GetMethod(nameof(IRepository<Transaction>.SaveChangesAsync)));
    }

    [Fact]
    public void IRepository_ShouldBeGenericInterface()
    {
        // Arrange
        var repositoryType = typeof(IRepository<>);

        // Act & Assert
        Assert.True(repositoryType.IsInterface);
        Assert.True(repositoryType.IsGenericTypeDefinition);
        Assert.Single(repositoryType.GetGenericArguments());
    }

    #endregion
}