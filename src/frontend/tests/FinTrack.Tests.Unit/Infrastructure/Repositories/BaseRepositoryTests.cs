using FinTrack.Core.Entities;
using FinTrack.Core.Enums;
using FinTrack.Core.Exceptions;
using FinTrack.Infrastructure.Data;
using FinTrack.Infrastructure.Repositories;
using FinTrack.Tests.Unit.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinTrack.Tests.Unit.Infrastructure.Repositories;

/// <summary>
/// Unit tests for BaseRepository
/// </summary>
public class BaseRepositoryTests : IDisposable
{
    private readonly FinTrackDbContext _context;
    private readonly Mock<ILogger<BaseRepository<Transaction>>> _mockLogger;
    private readonly BaseRepository<Transaction> _repository;

    public BaseRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<FinTrackDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new FinTrackDbContext(options);
        _mockLogger = new Mock<ILogger<BaseRepository<Transaction>>>();
        _repository = new BaseRepository<Transaction>(_context, _mockLogger.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnEntity()
    {
        // Arrange
        var transaction = TestDataBuilder.CreateTransaction();
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(transaction.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(transaction.Id, result.Id);
        Assert.Equal(transaction.Description, result.Description);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = 999;

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_WithDeletedEntity_ShouldReturnNull()
    {
        // Arrange
        var transaction = TestDataBuilder.CreateTransaction();
        transaction.IsDeleted = true;
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(transaction.Id);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetBySyncIdAsync_WithValidSyncId_ShouldReturnEntity()
    {
        // Arrange
        var transaction = TestDataBuilder.CreateTransaction();
        transaction.SyncId = Guid.NewGuid().ToString();
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetBySyncIdAsync(transaction.SyncId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(transaction.SyncId, result.SyncId);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllNonDeletedEntities()
    {
        // Arrange
        var transactions = new[]
        {
            TestDataBuilder.CreateTransaction(description: "Transaction 1"),
            TestDataBuilder.CreateTransaction(description: "Transaction 2"),
            TestDataBuilder.CreateTransaction(description: "Deleted Transaction")
        };
        
        transactions[2].IsDeleted = true;
        
        _context.Transactions.AddRange(transactions);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.DoesNotContain(result, t => t.IsDeleted);
    }

    [Fact]
    public async Task AddAsync_WithValidEntity_ShouldAddAndReturnEntity()
    {
        // Arrange
        var transaction = TestDataBuilder.CreateTransaction();

        // Act
        var result = await _repository.AddAsync(transaction);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.NotNull(result.SyncId);
        Assert.Equal(SyncStatus.PendingCreate, result.SyncStatus);
        
        var entityInDb = await _context.Transactions.FindAsync(result.Id);
        Assert.NotNull(entityInDb);
    }

    [Fact]
    public async Task AddAsync_WithNullEntity_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.AddAsync(null!));
    }

    [Fact]
    public async Task AddRangeAsync_WithValidEntities_ShouldAddAllEntities()
    {
        // Arrange
        var transactions = new[]
        {
            TestDataBuilder.CreateTransaction(description: "Transaction 1"),
            TestDataBuilder.CreateTransaction(description: "Transaction 2")
        };

        // Act
        var result = await _repository.AddRangeAsync(transactions);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, t => Assert.True(t.Id > 0));
        Assert.All(result, t => Assert.NotNull(t.SyncId));
        Assert.All(result, t => Assert.Equal(SyncStatus.PendingCreate, t.SyncStatus));
    }

    [Fact]
    public async Task UpdateAsync_WithValidEntity_ShouldUpdateEntity()
    {
        // Arrange
        var transaction = TestDataBuilder.CreateTransaction();
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();
        
        // Mark as synced to test the sync status change
        transaction.SyncStatus = SyncStatus.Synced;
        await _context.SaveChangesAsync();
        
        var originalVersion = transaction.Version;
        transaction.Description = "Updated Description";

        // Act
        var result = await _repository.UpdateAsync(transaction);

        // Assert
        Assert.Equal("Updated Description", result.Description);
        Assert.Equal(originalVersion + 1, result.Version);
        Assert.Equal(SyncStatus.PendingUpdate, result.SyncStatus);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistentEntity_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var transaction = TestDataBuilder.CreateTransaction();
        transaction.Id = 999; // Non-existent ID

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _repository.UpdateAsync(transaction));
    }

    [Fact]
    public async Task UpdateAsync_WithConcurrencyConflict_ShouldThrowConcurrencyException()
    {
        // Arrange
        var transaction = TestDataBuilder.CreateTransaction();
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();
        
        // Simulate concurrent update by changing version
        var conflictingTransaction = new Transaction
        {
            Id = transaction.Id,
            Version = transaction.Version + 1, // Different version
            Description = "Conflicting Update"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ConcurrencyException>(() => _repository.UpdateAsync(conflictingTransaction));
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldSoftDeleteEntity()
    {
        // Arrange
        var transaction = TestDataBuilder.CreateTransaction();
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(transaction.Id);

        // Assert
        Assert.True(result);
        
        var deletedEntity = await _context.Transactions.FindAsync(transaction.Id);
        Assert.NotNull(deletedEntity);
        Assert.True(deletedEntity.IsDeleted);
        Assert.Equal(SyncStatus.PendingDelete, deletedEntity.SyncStatus);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentId_ShouldReturnFalse()
    {
        // Act
        var result = await _repository.DeleteAsync(999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task HardDeleteAsync_WithValidId_ShouldPermanentlyDeleteEntity()
    {
        // Arrange
        var transaction = TestDataBuilder.CreateTransaction();
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.HardDeleteAsync(transaction.Id);

        // Assert
        Assert.True(result);
        
        var deletedEntity = await _context.Transactions
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == transaction.Id);
        Assert.Null(deletedEntity);
    }

    [Fact]
    public async Task CountAsync_WithoutPredicate_ShouldReturnTotalCount()
    {
        // Arrange
        var transactions = new[]
        {
            TestDataBuilder.CreateTransaction(),
            TestDataBuilder.CreateTransaction(),
            TestDataBuilder.CreateTransaction()
        };
        
        transactions[2].IsDeleted = true;
        
        _context.Transactions.AddRange(transactions);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.CountAsync();

        // Assert
        Assert.Equal(2, result); // Only non-deleted entities
    }

    [Fact]
    public async Task CountAsync_WithPredicate_ShouldReturnFilteredCount()
    {
        // Arrange
        var transactions = new[]
        {
            TestDataBuilder.CreateTransaction(type: TransactionType.Income),
            TestDataBuilder.CreateTransaction(type: TransactionType.Expense),
            TestDataBuilder.CreateTransaction(type: TransactionType.Income)
        };
        
        _context.Transactions.AddRange(transactions);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.CountAsync(t => t.Type == TransactionType.Income);

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public async Task AnyAsync_WithMatchingPredicate_ShouldReturnTrue()
    {
        // Arrange
        var transaction = TestDataBuilder.CreateTransaction(type: TransactionType.Income);
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.AnyAsync(t => t.Type == TransactionType.Income);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task AnyAsync_WithNonMatchingPredicate_ShouldReturnFalse()
    {
        // Arrange
        var transaction = TestDataBuilder.CreateTransaction(type: TransactionType.Expense);
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.AnyAsync(t => t.Type == TransactionType.Income);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnCorrectPage()
    {
        // Arrange
        var transactions = Enumerable.Range(1, 10)
            .Select(i => TestDataBuilder.CreateTransaction(description: $"Transaction {i}"))
            .ToArray();
        
        _context.Transactions.AddRange(transactions);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetPagedAsync(skip: 2, take: 3);

        // Assert
        Assert.Equal(3, result.Count());
    }

    [Theory]
    [InlineData(-1, 5)]
    [InlineData(0, 0)]
    [InlineData(0, -1)]
    public async Task GetPagedAsync_WithInvalidParameters_ShouldThrowArgumentException(int skip, int take)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _repository.GetPagedAsync(skip, take));
    }

    [Fact]
    public async Task GetPendingSyncAsync_ShouldReturnEntitiesPendingSync()
    {
        // Arrange
        var transactions = new[]
        {
            TestDataBuilder.CreateTransaction(description: "Synced"),
            TestDataBuilder.CreateTransaction(description: "Pending Create"),
            TestDataBuilder.CreateTransaction(description: "Pending Update"),
            TestDataBuilder.CreateTransaction(description: "Pending Delete")
        };
        
        _context.Transactions.AddRange(transactions);
        await _context.SaveChangesAsync();
        
        // Update sync status after saving to ensure it's preserved
        transactions[0].SyncStatus = SyncStatus.Synced;
        transactions[1].SyncStatus = SyncStatus.PendingCreate;
        transactions[2].SyncStatus = SyncStatus.PendingUpdate;
        transactions[3].SyncStatus = SyncStatus.PendingDelete;
        
        foreach (var transaction in transactions)
        {
            _context.Entry(transaction).Property(nameof(BaseEntity.SyncStatus)).IsModified = true;
        }
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetPendingSyncAsync();

        // Assert
        Assert.Equal(3, result.Count());
        Assert.DoesNotContain(result, t => t.SyncStatus == SyncStatus.Synced);
    }

    [Fact]
    public async Task GetBySyncStatusAsync_ShouldReturnEntitiesWithSpecificStatus()
    {
        // Arrange
        var transactions = new[]
        {
            TestDataBuilder.CreateTransaction(description: "Synced 1"),
            TestDataBuilder.CreateTransaction(description: "Synced 2"),
            TestDataBuilder.CreateTransaction(description: "Pending")
        };
        
        // Set sync status before adding to context
        transactions[0].SyncStatus = SyncStatus.Synced;
        transactions[1].SyncStatus = SyncStatus.Synced;
        transactions[2].SyncStatus = SyncStatus.PendingCreate;
        
        _context.Transactions.AddRange(transactions);
        await _context.SaveChangesAsync();
        
        // Update sync status after saving to ensure it's preserved
        foreach (var transaction in transactions.Take(2))
        {
            transaction.SyncStatus = SyncStatus.Synced;
            _context.Entry(transaction).Property(nameof(BaseEntity.SyncStatus)).IsModified = true;
        }
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetBySyncStatusAsync(SyncStatus.Synced);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, t => Assert.Equal(SyncStatus.Synced, t.SyncStatus));
    }

    [Fact]
    public async Task GetModifiedAfterAsync_ShouldReturnEntitiesModifiedAfterTimestamp()
    {
        // Arrange
        var cutoffTime = DateTime.UtcNow;
        
        // Create and save first transaction (old)
        var oldTransaction = TestDataBuilder.CreateTransaction(description: "Old");
        _context.Transactions.Add(oldTransaction);
        await _context.SaveChangesAsync();
        
        // Wait to ensure different timestamps
        await Task.Delay(100);
        
        // Create and save second transaction (recent) after the cutoff time
        var recentTransaction = TestDataBuilder.CreateTransaction(description: "Recent");
        _context.Transactions.Add(recentTransaction);
        await _context.SaveChangesAsync();

        // Act - get entities modified after the cutoff time (should only return recent)
        var result = await _repository.GetModifiedAfterAsync(cutoffTime.AddMilliseconds(50));

        // Assert
        Assert.Single(result);
        Assert.Equal("Recent", result.First().Description);
    }

    [Fact]
    public async Task MarkAsSyncedAsync_ShouldUpdateSyncStatus()
    {
        // Arrange
        var transactions = new[]
        {
            TestDataBuilder.CreateTransaction(description: "Transaction 1"),
            TestDataBuilder.CreateTransaction(description: "Transaction 2")
        };
        
        _context.Transactions.AddRange(transactions);
        await _context.SaveChangesAsync();
        
        // Update sync status after saving
        transactions[0].SyncStatus = SyncStatus.PendingCreate;
        transactions[1].SyncStatus = SyncStatus.PendingUpdate;
        
        foreach (var transaction in transactions)
        {
            _context.Entry(transaction).Property(nameof(BaseEntity.SyncStatus)).IsModified = true;
        }
        await _context.SaveChangesAsync();
        
        var syncIds = transactions.Select(t => t.SyncId).ToList();

        // Act
        var result = await _repository.MarkAsSyncedAsync(syncIds);

        // Assert
        Assert.Equal(2, result);
        
        var updatedTransactions = await _context.Transactions
            .IgnoreQueryFilters()
            .Where(t => syncIds.Contains(t.SyncId))
            .ToListAsync();
        
        Assert.All(updatedTransactions, t => Assert.Equal(SyncStatus.Synced, t.SyncStatus));
    }

    [Fact]
    public async Task MarkAsConflictedAsync_ShouldUpdateSyncStatus()
    {
        // Arrange
        var transaction = TestDataBuilder.CreateTransaction();
        transaction.SyncStatus = SyncStatus.PendingUpdate;
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.MarkAsConflictedAsync(new[] { transaction.SyncId });

        // Assert
        Assert.Equal(1, result);
        
        var updatedTransaction = await _context.Transactions.FindAsync(transaction.Id);
        Assert.Equal(SyncStatus.Conflict, updatedTransaction!.SyncStatus);
    }

    [Fact]
    public async Task GetWhereAsync_WithPredicate_ShouldReturnMatchingEntities()
    {
        // Arrange
        var transactions = new[]
        {
            TestDataBuilder.CreateTransaction(amount: 100m),
            TestDataBuilder.CreateTransaction(amount: 200m),
            TestDataBuilder.CreateTransaction(amount: 300m)
        };
        
        _context.Transactions.AddRange(transactions);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetWhereAsync(t => t.Amount >= 200m);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, t => Assert.True(t.Amount >= 200m));
    }

    [Fact]
    public async Task GetSingleAsync_WithMatchingPredicate_ShouldReturnEntity()
    {
        // Arrange
        var transaction = TestDataBuilder.CreateTransaction(amount: 150m);
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetSingleAsync(t => t.Amount == 150m);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(150m, result.Amount);
    }

    [Fact]
    public async Task GetSingleAsync_WithNonMatchingPredicate_ShouldReturnNull()
    {
        // Arrange
        var transaction = TestDataBuilder.CreateTransaction(amount: 100m);
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetSingleAsync(t => t.Amount == 200m);

        // Assert
        Assert.Null(result);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}