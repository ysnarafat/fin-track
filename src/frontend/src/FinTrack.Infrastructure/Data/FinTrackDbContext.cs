using FinTrack.Core.Entities;
using FinTrack.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace FinTrack.Infrastructure.Data;

/// <summary>
/// Entity Framework Core database context for FinTrack application
/// </summary>
public class FinTrackDbContext : DbContext
{
    private readonly ILogger<FinTrackDbContext>? _logger;

    public FinTrackDbContext(DbContextOptions<FinTrackDbContext> options) : base(options)
    {
    }

    public FinTrackDbContext(DbContextOptions<FinTrackDbContext> options, ILogger<FinTrackDbContext> logger) 
        : base(options)
    {
        _logger = logger;
    }

    // DbSets for all entities
    public DbSet<Transaction> Transactions { get; set; } = null!;
    public DbSet<Account> Accounts { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Budget> Budgets { get; set; } = null!;
    public DbSet<Goal> Goals { get; set; } = null!;
    public DbSet<GoalMilestone> GoalMilestones { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from the Configurations assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FinTrackDbContext).Assembly);
        
        // Apply base entity configuration for all entities
        Configurations.BaseEntityConfiguration.ApplyBaseEntityConfiguration(modelBuilder);
    }

    /// <summary>
    /// Override SaveChangesAsync to automatically update audit fields
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        
        try
        {
            var result = await base.SaveChangesAsync(cancellationToken);
            _logger?.LogDebug("Successfully saved {Count} changes to database", result);
            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error occurred while saving changes to database");
            throw;
        }
    }

    /// <summary>
    /// Override SaveChanges to automatically update audit fields
    /// </summary>
    public override int SaveChanges()
    {
        UpdateAuditFields();
        
        try
        {
            var result = base.SaveChanges();
            _logger?.LogDebug("Successfully saved {Count} changes to database", result);
            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error occurred while saving changes to database");
            throw;
        }
    }

    /// <summary>
    /// Updates audit fields for entities being added or modified
    /// </summary>
    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();
        var now = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    // Set CreatedAt and UpdatedAt if they are default values
                    if (entry.Entity.CreatedAt == default)
                    {
                        entry.Entity.CreatedAt = now;
                    }
                    // For new entities, always set UpdatedAt to the same time as CreatedAt or later
                    if (entry.Entity.UpdatedAt == default || entry.Entity.UpdatedAt < entry.Entity.CreatedAt)
                    {
                        entry.Entity.UpdatedAt = entry.Entity.CreatedAt;
                    }
                    if (string.IsNullOrEmpty(entry.Entity.SyncId))
                    {
                        entry.Entity.SyncId = Guid.NewGuid().ToString();
                    }
                    // Set initial version
                    if (entry.Entity.Version == 0)
                    {
                        entry.Entity.Version = 1;
                    }
                    // Check if SyncStatus was explicitly set
                    var addedSyncStatusProperty = entry.Property(nameof(BaseEntity.SyncStatus));
                    if (!addedSyncStatusProperty.IsModified && entry.Entity.SyncStatus == default(SyncStatus))
                    {
                        entry.Entity.SyncStatus = SyncStatus.PendingCreate;
                    }
                    break;
                case EntityState.Modified:
                    // Always update UpdatedAt for modifications
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.Version++;
                    
                    // Check if SyncStatus was explicitly changed by user code
                    var syncStatusProperty = entry.Property(nameof(BaseEntity.SyncStatus));
                    var originalSyncStatus = (SyncStatus)entry.OriginalValues[nameof(BaseEntity.SyncStatus)];
                    var currentSyncStatus = entry.Entity.SyncStatus;
                    
                    // If the sync status value hasn't actually changed (even if marked as modified),
                    // then we should update it based on business rules
                    if (originalSyncStatus == currentSyncStatus)
                    {
                        // Change sync status to PendingUpdate if it was originally Synced
                        if (originalSyncStatus == SyncStatus.Synced)
                        {
                            entry.Entity.SyncStatus = SyncStatus.PendingUpdate;
                        }
                        // If it's PendingCreate, keep it as PendingCreate (new entity being modified)
                        // If it's other statuses, leave them unchanged
                    }
                    break;
                case EntityState.Deleted:
                    // Check if this is a hard delete (marked with HardDelete sync status)
                    if (entry.Entity.SyncStatus == SyncStatus.HardDelete)
                    {
                        // Allow hard delete to proceed
                        break;
                    }
                    
                    // For soft delete, change to modified and set IsDeleted
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.Version++;
                    entry.Entity.SyncStatus = SyncStatus.PendingDelete;
                    break;
            }
        }
    }












}