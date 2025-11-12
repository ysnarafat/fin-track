using FinTrack.Core.Entities;
using FinTrack.Core.Enums;
using FinTrack.Tests.Unit.Helpers;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Tests.Unit.Infrastructure;

/// <summary>
/// Focused tests for audit field functionality in FinTrackDbContext
/// </summary>
public class FinTrackDbContextAuditTests : DbContextTestBase
{
    [Fact]
    public async Task SaveChangesAsync_NewEntity_SetsAllAuditFields()
    {
        // Arrange
        var account = TestDataBuilder.CreateTestAccount();
        var beforeAdd = DateTime.UtcNow;

        // Act
        Context.Accounts.Add(account);
        await Context.SaveChangesAsync();
        var afterSave = DateTime.UtcNow;

        // Assert
        // Allow a small buffer for timing (timestamps should be between beforeAdd and afterSave)
        Assert.True(account.CreatedAt >= beforeAdd.AddMilliseconds(-10) && account.CreatedAt <= afterSave);
        Assert.True(account.UpdatedAt >= beforeAdd.AddMilliseconds(-10) && account.UpdatedAt <= afterSave);
        // For new entities, UpdatedAt should equal CreatedAt (set by audit logic)
        // Allow for minor timing precision differences (within 1 millisecond)
        var timeDifference = Math.Abs((account.UpdatedAt - account.CreatedAt).TotalMilliseconds);
        Assert.True(timeDifference < 1, $"UpdatedAt and CreatedAt should be equal or very close. Difference: {timeDifference}ms");
        Assert.False(string.IsNullOrEmpty(account.SyncId));
        Assert.True(Guid.TryParse(account.SyncId, out _)); // Should be a valid GUID
        Assert.Equal(SyncStatus.PendingCreate, account.SyncStatus);
        Assert.Equal(1, account.Version);
        Assert.False(account.IsDeleted);
    }

    [Fact]
    public async Task SaveChangesAsync_ModifiedSyncedEntity_UpdatesAuditFieldsAndSyncStatus()
    {
        // Arrange
        var account = TestDataBuilder.CreateTestAccount();
        
        Context.Accounts.Add(account);
        await Context.SaveChangesAsync();

        // Manually set to synced (simulating a sync operation)
        // Update the sync status while the entity is still tracked
        account.SyncStatus = SyncStatus.Synced;
        await Context.SaveChangesAsync();
        
        // Reset the entity state to ensure clean tracking for the next modification
        Context.Entry(account).State = EntityState.Unchanged;

        var originalCreatedAt = account.CreatedAt;
        var originalUpdatedAt = account.UpdatedAt;
        var originalVersion = account.Version;
        var originalSyncId = account.SyncId;

        // Wait to ensure timestamp difference
        await Task.Delay(10);

        // Act
        account.Name = "Modified Account Name";
        // Don't call Update() since the entity is already tracked
        await Context.SaveChangesAsync();

        // Assert
        Assert.Equal(originalCreatedAt, account.CreatedAt); // Should not change
        Assert.True(account.UpdatedAt > originalUpdatedAt); // Should be updated
        Assert.Equal(originalVersion + 1, account.Version); // Should increment
        Assert.Equal(originalSyncId, account.SyncId); // Should not change
        Assert.Equal(SyncStatus.PendingUpdate, account.SyncStatus); // Should change from Synced
    }

    [Fact]
    public async Task SaveChangesAsync_ModifiedNonSyncedEntity_UpdatesAuditFieldsButNotSyncStatus()
    {
        // Arrange
        var account = TestDataBuilder.CreateTestAccount();
        account.SyncStatus = SyncStatus.SyncFailed; // Non-synced status
        
        Context.Accounts.Add(account);
        await Context.SaveChangesAsync();

        var originalSyncStatus = account.SyncStatus;

        // Act
        account.Name = "Modified Account Name";
        Context.Accounts.Update(account);
        await Context.SaveChangesAsync();

        // Assert
        Assert.Equal(originalSyncStatus, account.SyncStatus); // Should not change
    }

    [Fact]
    public async Task SaveChangesAsync_DeletedEntity_PerformsSoftDeleteWithAuditUpdate()
    {
        // Arrange
        var account = TestDataBuilder.CreateTestAccount();
        Context.Accounts.Add(account);
        await Context.SaveChangesAsync();

        var originalCreatedAt = account.CreatedAt;
        var originalVersion = account.Version;

        // Act
        Context.Accounts.Remove(account);
        await Context.SaveChangesAsync();

        // Assert
        Assert.True(account.IsDeleted);
        Assert.Equal(originalCreatedAt, account.CreatedAt); // Should not change
        Assert.True(account.UpdatedAt > originalCreatedAt); // Should be updated
        Assert.Equal(originalVersion + 1, account.Version); // Should increment
        Assert.Equal(SyncStatus.PendingDelete, account.SyncStatus);
    }

    [Fact]
    public async Task SaveChangesAsync_MultipleEntitiesWithDifferentStates_HandlesAllCorrectly()
    {
        // Arrange
        var newAccount = TestDataBuilder.CreateTestAccount("New Account");
        var existingAccount = TestDataBuilder.CreateTestAccount("Existing Account");
        
        // Add existing account first
        Context.Accounts.Add(existingAccount);
        await Context.SaveChangesAsync();
        existingAccount.SyncStatus = SyncStatus.Synced;
        await Context.SaveChangesAsync();

        var originalVersion = existingAccount.Version;

        // Act - Add new, modify existing, delete another
        Context.Accounts.Add(newAccount); // New entity
        existingAccount.Name = "Modified Existing Account"; // Modified entity
        // Don't call Update() since the entity is already tracked
        
        await Context.SaveChangesAsync();

        // Assert
        // New account
        Assert.Equal(SyncStatus.PendingCreate, newAccount.SyncStatus);
        Assert.Equal(1, newAccount.Version);
        Assert.False(string.IsNullOrEmpty(newAccount.SyncId));

        // Modified account
        Assert.Equal(SyncStatus.PendingUpdate, existingAccount.SyncStatus);
        Assert.Equal(originalVersion + 1, existingAccount.Version);
    }

    [Fact]
    public async Task SaveChangesAsync_EntityWithExistingSyncId_PreservesSyncId()
    {
        // Arrange
        var customSyncId = "custom-sync-id-12345";
        var account = TestDataBuilder.CreateTestAccount();
        account.SyncId = customSyncId;

        // Act
        Context.Accounts.Add(account);
        await Context.SaveChangesAsync();

        // Assert
        Assert.Equal(customSyncId, account.SyncId);
    }

    [Fact]
    public async Task SaveChangesAsync_EntityWithNullSyncId_GeneratesNewSyncId()
    {
        // Arrange
        var account = TestDataBuilder.CreateTestAccount();
        account.SyncId = null!;

        // Act
        Context.Accounts.Add(account);
        await Context.SaveChangesAsync();

        // Assert
        Assert.NotNull(account.SyncId);
        Assert.NotEmpty(account.SyncId);
        Assert.True(Guid.TryParse(account.SyncId, out _));
    }

    [Fact]
    public async Task SaveChangesAsync_EntityWithEmptySyncId_GeneratesNewSyncId()
    {
        // Arrange
        var account = TestDataBuilder.CreateTestAccount();
        account.SyncId = string.Empty;

        // Act
        Context.Accounts.Add(account);
        await Context.SaveChangesAsync();

        // Assert
        Assert.NotNull(account.SyncId);
        Assert.NotEmpty(account.SyncId);
        Assert.True(Guid.TryParse(account.SyncId, out _));
    }

    [Theory]
    [InlineData(SyncStatus.PendingCreate)]
    [InlineData(SyncStatus.PendingUpdate)]
    [InlineData(SyncStatus.PendingDelete)]
    [InlineData(SyncStatus.SyncFailed)]
    [InlineData(SyncStatus.Conflict)]
    public async Task SaveChangesAsync_NewEntityWithNonSyncedStatus_PreservesSyncStatus(SyncStatus initialStatus)
    {
        // Arrange
        var account = TestDataBuilder.CreateTestAccount();
        account.SyncStatus = initialStatus;

        // Act
        Context.Accounts.Add(account);
        await Context.SaveChangesAsync();

        // Assert
        Assert.Equal(initialStatus, account.SyncStatus);
    }

    [Fact]
    public async Task SaveChangesAsync_ConcurrentModification_HandlesVersioningCorrectly()
    {
        // Arrange
        var account = TestDataBuilder.CreateTestAccount();
        Context.Accounts.Add(account);
        await Context.SaveChangesAsync();

        // Create two separate contexts to simulate concurrent access
        using var context1 = CreateNewContext();
        using var context2 = CreateNewContext();

        var account1 = await context1.Accounts.FindAsync(account.Id);
        var account2 = await context2.Accounts.FindAsync(account.Id);

        // Act
        account1!.Name = "Modified by Context 1";
        account2!.Name = "Modified by Context 2";

        await context1.SaveChangesAsync(); // This should succeed

        // Assert
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => context2.SaveChangesAsync());
    }

    [Fact]
    public async Task SaveChanges_SynchronousVersion_WorksCorrectly()
    {
        // Arrange
        var account = TestDataBuilder.CreateTestAccount();

        // Act
        Context.Accounts.Add(account);
        var result = Context.SaveChanges();

        // Assert
        Assert.Equal(1, result);
        Assert.True(account.Id > 0);
        Assert.Equal(SyncStatus.PendingCreate, account.SyncStatus);
        Assert.False(string.IsNullOrEmpty(account.SyncId));
    }

    [Fact]
    public async Task UpdateAuditFields_WithMultipleEntityTypes_HandlesAllCorrectly()
    {
        // Arrange
        var account = TestDataBuilder.CreateTestAccount();
        var category = TestDataBuilder.CreateTestCategory();
        var budget = TestDataBuilder.CreateTestBudget();
        var goal = TestDataBuilder.CreateTestGoal();

        // Act
        Context.Accounts.Add(account);
        Context.Categories.Add(category);
        Context.Budgets.Add(budget);
        Context.Goals.Add(goal);
        await Context.SaveChangesAsync();

        // Assert
        var entities = new BaseEntity[] { account, category, budget, goal };
        
        foreach (var entity in entities)
        {
            Assert.True(entity.Id > 0);
            Assert.True(entity.CreatedAt > DateTime.MinValue);
            Assert.True(entity.UpdatedAt > DateTime.MinValue);
            Assert.False(string.IsNullOrEmpty(entity.SyncId));
            Assert.Equal(SyncStatus.PendingCreate, entity.SyncStatus);
            Assert.Equal(1, entity.Version);
        }
    }
}