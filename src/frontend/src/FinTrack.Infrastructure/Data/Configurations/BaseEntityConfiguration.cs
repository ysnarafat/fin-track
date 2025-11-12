using FinTrack.Core.Entities;
using FinTrack.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Linq.Expressions;

namespace FinTrack.Infrastructure.Data.Configurations;

/// <summary>
/// Base configuration for all entities inheriting from BaseEntity
/// </summary>
public static class BaseEntityConfiguration
{
    /// <summary>
    /// Configure common properties for all entities inheriting from BaseEntity
    /// </summary>
    public static void ConfigureBaseEntity<T>(EntityTypeBuilder<T> builder) where T : BaseEntity
    {
        // Audit fields
        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasComment("Date and time when the entity was created");

        builder.Property(e => e.UpdatedAt)
            .IsRequired()
            .HasComment("Date and time when the entity was last updated");

        builder.Property(e => e.IsDeleted)
            .HasDefaultValue(false)
            .HasComment("Soft delete flag");

        // Sync fields
        builder.Property(e => e.SyncId)
            .IsRequired()
            .HasMaxLength(50)
            .HasComment("Unique identifier for synchronization");

        builder.Property(e => e.SyncStatus)
            .IsRequired()
            .HasDefaultValue(SyncStatus.PendingCreate)
            .HasConversion<int>()
            .HasComment("Synchronization status");

        builder.Property(e => e.Version)
            .IsRequired()
            .HasDefaultValue(1)
            .IsConcurrencyToken()
            .HasComment("Version number for optimistic concurrency control");

        builder.Property(e => e.LastModifiedBy)
            .HasMaxLength(100)
            .HasComment("User who last modified the entity");

        builder.Property(e => e.LastSyncAt)
            .HasComment("Date and time when the entity was last synchronized");

        // Indexes for sync operations
        builder.HasIndex(e => e.SyncId)
            .IsUnique()
            .HasDatabaseName($"IX_{typeof(T).Name}_SyncId_Unique");

        builder.HasIndex(e => e.SyncStatus)
            .HasDatabaseName($"IX_{typeof(T).Name}_SyncStatus");

        builder.HasIndex(e => e.IsDeleted)
            .HasDatabaseName($"IX_{typeof(T).Name}_IsDeleted");

        // Global query filter to exclude soft deleted entities
        var parameter = Expression.Parameter(typeof(T), "e");
        var property = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
        var filter = Expression.Lambda<Func<T, bool>>(
            Expression.Equal(property, Expression.Constant(false)),
            parameter);
        
        builder.HasQueryFilter(filter);
    }

    /// <summary>
    /// Apply base entity configuration to all entities in the model
    /// </summary>
    public static void ApplyBaseEntityConfiguration(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(BaseEntityConfiguration)
                    .GetMethod(nameof(ConfigureBaseEntity))!
                    .MakeGenericMethod(entityType.ClrType);
                
                var builderProperty = typeof(ModelBuilder)
                    .GetMethod(nameof(ModelBuilder.Entity), Type.EmptyTypes)!
                    .MakeGenericMethod(entityType.ClrType)
                    .Invoke(modelBuilder, null);

                method.Invoke(null, new[] { builderProperty });
            }
        }
    }
}