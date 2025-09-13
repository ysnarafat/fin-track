using FinTrack.Core.Entities;
using FinTrack.Core.Enums;
using FinTrack.Infrastructure.Data;
using FinTrack.Infrastructure.Repositories;
using FinTrack.Tests.Unit.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinTrack.Tests.Unit.Infrastructure.Repositories;

/// <summary>
/// Additional unit tests for BaseRepository covering edge cases and advanced scenarios
/// </summary>
public class BaseRepositoryAdditionalTests : IDisposable
{
    private readonly FinTrackDbContext _context;
    private readonly Mock<ILogger<BaseRepository<Transaction>>> _mockLogger;
    private readonly BaseRepository<Transaction> _repository;

    public BaseRepositoryAdditionalTests()
    {
        var options = new DbContextOptionsBuilder<FinTrackDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new FinTrackDbContext(options);
        _mockLogger = new Mock<ILogger<BaseRepository<Transaction>>>();
        _repository = new BaseRepository<Transaction>(_context, _mockLogger.Object);
    }

    #region GetBySyncIdAsync Additional Tests

    [Fact]
    public async Task GetBySyncIdAsync_WithNullSyncId_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetBySyncIdAsync(null!);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetBySyncIdAsync_WithEmptySyncId_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetBySyncIdAsync(string.Empty);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetBySyncIdAsync_WithDeletedEntity_ShouldReturnNull()
    {
        // Arrange
        var transaction = TestDataBuilder.CreateTransaction();
        transaction.SyncId = Guid.NewGuid().ToString();
        transaction.IsDeleted = true;
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetBySyncIdAsync(transaction.SyncId);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region AddRangeAsync Additional Tests

    [Fact]
    public async Task AddRangeAsync_WithNullCollection_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.AddRangeAsync(null!));
    }

    [Fact]
    public async Task AddRangeAsync_WithEmptyCollection_ShouldReturnEmptyCollection()
    {
        // Arrange
        var emptyList = new List<Transaction>();

        // Act
        var result = await _repository.AddRangeAsync(emptyList);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task AddRangeAsync_ShouldGenerateUniqueSyncIds()
    {
        // Arrange
        var transactions = new[]
        {
            TestDataBuilder.CreateTransaction(description: "Transaction 1"),
            TestDataBuilder.CreateTransaction(description: "Transaction 2"),
            TestDataBuilder.CreateTransaction(description: "Transaction 3")
        };

        // Act
        var result = await _repository.AddRangeAsync(transactions);

        // Assert
        var syncIds = result.Select(t => t.SyncId).ToList();
        Assert.Equal(3, syncIds.Distinct().Count()); // All sync IDs should be unique
        Assert.All(syncIds, syncId => Assert.False(string.IsNullOrEmpty(syncId)));
    }

    #endregion

    #region UpdateRangeAsync Additional Tests

    [Fact]
    public async Task UpdateRangeAsync_WithNullCollection_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.UpdateRangeAsync(null!));
    }

    [Fact]
    public async Task UpdateRangeAsync_WithEmptyCollection_ShouldReturnEmptyCollection()
    {
        // Arrange
        var emptyList = new List<Transaction>();

        // Act
        var result = await _repository.UpdateRangeAsync(emptyList);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task UpdateRangeAsync_ShouldIncrementVersionForAllEntities()
    {
        // Arrange
        var transactions = new[]
        {
            TestDataBuilder.CreateTransaction(description: "Transaction 1"),
            TestDataBuilder.CreateTransaction(description: "Transaction 2")
        };
        
        _context.Transactions.AddRange(transactions);
        await _context.SaveChangesAsync();
        
        var originalVersions = transactions.Select(t => t.Version).ToList();
        
        // Modify entities
        transactions[0].Description = "Updated 1";
        transactions[1].Description = "Updated 2";

        // Act
        var result = await _repository.UpdateRangeAsync(transactions);

        // Assert
        var resultList = result.ToList();
        for (int i = 0; i < resultList.Count; i++)
        {
            Assert.Equal(originalVersions[i] + 1, resultList[i].Version);
        }
    }

    #endregion

    #region DeleteAsync Additional Tests

    [Fact]
    public async Task DeleteAsync_WithEntity_ShouldCallDeleteById()
    {
        // Arrange
        var transaction = TestDataBuilder.CreateTransaction();
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(transaction);

        // Assert
        Assert.True(result);
        
        var deletedEntity = await _context.Transactions.FindAsync(transaction.Id);
        Assert.NotNull(deletedEntity);
        Assert.True(deletedEntity.IsDeleted);
    }

    [Fact]
    public async Task DeleteAsync_WithNullEntity_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.DeleteAsync(null!));
    }

    [Fact]
    public async Task DeleteAsync_ShouldSetSyncStatusToPendingDelete()
    {
        // Arrange
        var transaction = TestDataBuilder.CreateTransaction();
        transaction.SyncStatus = SyncStatus.Synced;
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(transaction.Id);

        // Assert
        var deletedEntity = await _context.Transactions.FindAsync(transaction.Id);
        Assert.Equal(SyncStatus.PendingDelete, deletedEntity!.SyncStatus);
    }

    #endregion

    #region HardDeleteAsync Additional Tests

    [Fact]
    public async Task HardDeleteAsync_WithNonExistentId_ShouldReturnFalse()
    {
        // Act
        var result = await _repository.HardDeleteAsync(999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task HardDeleteAsync_ShouldRemoveEntityCompletely()
    {
        // Arrange
        var transaction = TestDataBuilder.CreateTransaction();
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();
        var transactionId = transaction.Id;

        // Act
        var result = await _repository.HardDeleteAsync(transactionId);

        // Assert
        Assert.True(result);
        
        // Verify entity is completely removed (even with IgnoreQueryFilters)
        var deletedEntity = await _context.Transactions
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == transactionId);
        Assert.Null(deletedEntity);
    }

    #endregion

    #region GetPagedAsync Additional Tests

    [Fact]
    public async Task GetPagedAsync_WithPredicate_ShouldReturnFilteredPage()
    {
        // Arrange
        var transactions = Enumerable.Range(1, 10)
            .Select(i => TestDataBuilder.CreateTransaction(
                amount: i * 100m,
                description: $"Transaction {i}"))
            .ToArray();
        
        _context.Transactions.AddRange(transactions);
        await _context.SaveChangesAsync();

        // Act - Get transactions with amount >= 500
        var result = await _repository.GetPagedAsync(
            skip: 0,
            take: 3,
            predicate: t => t.Amount >= 500m);

        // Assert
        var resultList = result.ToList();
        Assert.Equal(3, resultList.Count);
        Assert.All(resultList, t => Assert.True(t.Amount >= 500m));
    }

    [Fact]
    public async Task GetPagedAsync_WithOrderBy_ShouldReturnOrderedPage()
    {
        // Arrange
        var transactions = new[]
        {
            TestDataBuilder.CreateTransaction(amount: 300m, description: "C"),
            TestDataBuilder.CreateTransaction(amount: 100m, description: "A"),
            TestDataBuilder.CreateTransaction(amount: 200m, description: "B")
        };
        
        _context.Transactions.AddRange(transactions);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetPagedAsync(
            skip: 0,
            take: 3,
            orderBy: t => t.Amount);

        // Assert
        var resultList = result.ToList();
        Assert.Equal(3, resultList.Count);
        Assert.Equal(100m, resultList[0].Amount);
        Assert.Equal(200m, resultList[1].Amount);
        Assert.Equal(300m, resultList[2].Amount);
    }

    [Fact]
    public async Task GetPagedAsync_WithSkipGreaterThanTotal_ShouldReturnEmpty()
    {
        // Arrange
        var transactions = Enumerable.Range(1, 5)
            .Select(i => TestDataBuilder.CreateTransaction(description: $"Transaction {i}"))
            .ToArray();
        
        _context.Transactions.AddRange(transactions);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetPagedAsync(skip: 10, take: 5);

        // Assert
        Assert.Empty(result);
    }

    [Theory]
    [InlineData(0, 100)] // Take more than available
    [InlineData(2, 10)]  // Skip some, take more than remaining
    public async Task GetPagedAsync_WithTakeGreaterThanAvailable_ShouldReturnAvailable(int skip, int take)
    {
        // Arrange
        var transactions = Enumerable.Range(1, 5)
            .Select(i => TestDataBuilder.CreateTransaction(description: $"Transaction {i}"))
            .ToArray();
        
        _context.Transactions.AddRange(transactions);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetPagedAsync(skip, take);

        // Assert
        var resultList = result.ToList();
        Assert.True(resultList.Count <= 5 - skip);
    }

    #endregion

    #region GetPendingSyncAsync Additional Tests

    [Fact]
    public async Task GetPendingSyncAsync_ShouldIncludeSoftDeletedEntities()
    {
        // Arrange
        var transaction = TestDataBuilder.CreateTransaction();
        transaction.IsDeleted = true;
        transaction.SyncStatus = SyncStatus.PendingDelete;
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetPendingSyncAsync();

        // Assert
        Assert.Single(result);
        Assert.Contains(result, t => t.Id == transaction.Id);
    }

    [Fact]
    public async Task GetPendingSyncAsync_WithNoEntities_ShouldReturnEmpty()
    {
        // Act
        var result = await _repository.GetPendingSyncAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetPendingSyncAsync_ShouldNotIncludeSyncedEntities()
    {
        // Arrange
        var transactions = new[]
        {
            TestDataBuilder.CreateTransaction(description: "Synced"),
            TestDataBuilder.CreateTransaction(description: "Pending")
        };
        
        _context.Transactions.AddRange(transactions);
        await _context.SaveChangesAsync();
        
        transactions[0].SyncStatus = SyncStatus.Synced;
        transactions[1].SyncStatus = SyncStatus.PendingCreate;
        
        foreach (var transaction in transactions)
        {
            _context.Entry(transaction).Property(nameof(BaseEntity.SyncStatus)).IsModified = true;
        }
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetPendingSyncAsync();

        // Assert
        Assert.Single(result);
        Assert.DoesNotContain(result, t => t.SyncStatus == SyncStatus.Synced);
    }

    #endregion

    #region GetBySyncStatusAsync Additional Tests

    [Fact]
    public async Task GetBySyncStatusAsync_WithNoMatchingEntities_ShouldReturnEmpty()
    {
        // Arrange
        var transaction = TestDataBuilder.CreateTransaction();
        // Add and save first, then update sync status
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();
        
        // Now update the sync status to Synced (this will trigger PendingUpdate, but we'll force it)
        transaction.SyncStatus = SyncStatus.Synced;
        _context.Entry(transaction).Property(nameof(Transaction.SyncStatus)).IsModified = true;
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetBySyncStatusAsync(SyncStatus.PendingCreate);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetBySyncStatusAsync_ShouldIncludeDeletedEntities()
    {
        // Arrange
        var transaction = TestDataBuilder.CreateTransaction();
        transaction.IsDeleted = true;
        transaction.SyncStatus = SyncStatus.PendingDelete;
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetBySyncStatusAsync(SyncStatus.PendingDelete);

        // Assert
        Assert.Single(result);
    }

    #endregion

    #region GetModifiedAfterAsync Additional Tests

    [Fact]
    public async Task GetModifiedAfterAsync_WithFutureTimestamp_ShouldReturnEmpty()
    {
        // Arrange
        var transaction = TestDataBuilder.CreateTransaction();
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetModifiedAfterAsync(DateTime.UtcNow.AddDays(1));

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetModifiedAfterAsync_ShouldIncludeDeletedEntities()
    {
        // Arrange
        var transaction = TestDataBuilder.CreateTransaction();
        transaction.IsDeleted = true;
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetModifiedAfterAsync(DateTime.UtcNow.AddMinutes(-1));

        // Assert
        Assert.Single(result);
    }

    #endregion

    #region MarkAsSyncedAsync Additional Tests

    [Fact]
    public async Task MarkAsSyncedAsync_WithNullCollection_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.MarkAsSyncedAsync(null!));
    }

    [Fact]
    public async Task MarkAsSyncedAsync_WithEmptyCollection_ShouldReturnZero()
    {
        // Act
        var result = await _repository.MarkAsSyncedAsync(new List<string>());

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public async Task MarkAsSyncedAsync_WithNonExistentSyncIds_ShouldReturnZero()
    {
        // Act
        var result = await _repository.MarkAsSyncedAsync(new[] { "non-existent-1", "non-existent-2" });

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public async Task MarkAsSyncedAsync_ShouldUpdateLastSyncAt()
    {
        // Arrange
        var transaction = TestDataBuilder.CreateTransaction();
        transaction.SyncStatus = SyncStatus.PendingCreate;
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();
        
        var beforeSync = DateTime.UtcNow;

        // Act
        await _repository.MarkAsSyncedAsync(new[] { transaction.SyncId });

        // Assert
        var updatedTransaction = await _context.Transactions.FindAsync(transaction.Id);
        Assert.NotNull(updatedTransaction!.LastSyncAt);
        Assert.True(updatedTransaction.LastSyncAt >= beforeSync);
    }

    #endregion

    #region MarkAsConflictedAsync Additional Tests

    [Fact]
    public async Task MarkAsConflictedAsync_WithNullCollection_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.MarkAsConflictedAsync(null!));
    }

    [Fact]
    public async Task MarkAsConflictedAsync_WithEmptyCollection_ShouldReturnZero()
    {
        // Act
        var result = await _repository.MarkAsConflictedAsync(new List<string>());

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public async Task MarkAsConflictedAsync_WithNonExistentSyncIds_ShouldReturnZero()
    {
        // Act
        var result = await _repository.MarkAsConflictedAsync(new[] { "non-existent-1", "non-existent-2" });

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public async Task MarkAsConflictedAsync_ShouldUpdateMultipleEntities()
    {
        // Arrange
        var transactions = new[]
        {
            TestDataBuilder.CreateTransaction(description: "Transaction 1"),
            TestDataBuilder.CreateTransaction(description: "Transaction 2")
        };
        
        _context.Transactions.AddRange(transactions);
        await _context.SaveChangesAsync();
        
        var syncIds = transactions.Select(t => t.SyncId).ToList();

        // Act
        var result = await _repository.MarkAsConflictedAsync(syncIds);

        // Assert
        Assert.Equal(2, result);
        
        var updatedTransactions = await _context.Transactions
            .Where(t => syncIds.Contains(t.SyncId))
            .ToListAsync();
        
        Assert.All(updatedTransactions, t => Assert.Equal(SyncStatus.Conflict, t.SyncStatus));
    }

    #endregion

    #region SaveChangesAsync Tests

    [Fact]
    public async Task SaveChangesAsync_WithNoChanges_ShouldReturnZero()
    {
        // Act
        var result = await _repository.SaveChangesAsync();

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public async Task SaveChangesAsync_WithChanges_ShouldReturnCount()
    {
        // Arrange
        var transaction = TestDataBuilder.CreateTransaction();
        _context.Transactions.Add(transaction);

        // Act
        var result = await _repository.SaveChangesAsync();

        // Assert
        Assert.Equal(1, result);
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task GetByIdAsync_WithCancelledToken_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _repository.GetByIdAsync(1, cts.Token));
    }

    [Fact]
    public async Task GetAllAsync_WithCancelledToken_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => _repository.GetAllAsync(cts.Token));
    }

    #endregion

    #region Concurrent Operations Tests

    [Fact(Skip = "EF Core InMemory provider doesn't support true concurrent operations on the same DbContext instance")]
    public async Task ConcurrentAddOperations_ShouldAllSucceed()
    {
        // Arrange
        var tasks = Enumerable.Range(1, 10)
            .Select(i => Task.Run(async () =>
            {
                var transaction = TestDataBuilder.CreateTransaction(description: $"Concurrent {i}");
                return await _repository.AddAsync(transaction);
            }))
            .ToArray();

        // Act
        var results = await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(10, results.Length);
        Assert.All(results, r => Assert.True(r.Id > 0));
        
        var allTransactions = await _repository.GetAllAsync();
        Assert.Equal(10, allTransactions.Count());
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public async Task GetWhereAsync_WithComplexPredicate_ShouldReturnCorrectResults()
    {
        // Arrange
        var transactions = new[]
        {
            TestDataBuilder.CreateTransaction(amount: 100m, type: TransactionType.Income),
            TestDataBuilder.CreateTransaction(amount: 200m, type: TransactionType.Expense),
            TestDataBuilder.CreateTransaction(amount: 150m, type: TransactionType.Income),
            TestDataBuilder.CreateTransaction(amount: 250m, type: TransactionType.Expense)
        };
        
        _context.Transactions.AddRange(transactions);
        await _context.SaveChangesAsync();

        // Act - Get income transactions with amount > 100
        var result = await _repository.GetWhereAsync(
            t => t.Type == TransactionType.Income && t.Amount > 100m);

        // Assert
        Assert.Single(result);
        Assert.Equal(150m, result.First().Amount);
    }

    [Fact]
    public async Task CountAsync_WithComplexPredicate_ShouldReturnCorrectCount()
    {
        // Arrange
        var transactions = Enumerable.Range(1, 20)
            .Select(i => TestDataBuilder.CreateTransaction(
                amount: i * 50m,
                type: i % 2 == 0 ? TransactionType.Income : TransactionType.Expense))
            .ToArray();
        
        _context.Transactions.AddRange(transactions);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.CountAsync(
            t => t.Type == TransactionType.Income && t.Amount >= 500m);

        // Assert
        Assert.Equal(6, result); // Transactions with amounts 500, 600, 700, 800, 900, 1000 (even numbers 10-20)
    }

    #endregion

    public void Dispose()
    {
        _context.Dispose();
    }
}
