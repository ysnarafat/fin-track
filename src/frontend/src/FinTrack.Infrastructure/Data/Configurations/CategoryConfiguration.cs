using FinTrack.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTrack.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for Category entity
/// </summary>
public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        // Primary key
        builder.HasKey(c => c.Id);

        // Properties
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100)
            .HasComment("Name of the category");

        builder.Property(c => c.Description)
            .HasMaxLength(500)
            .HasComment("Description of the category");

        builder.Property(c => c.Icon)
            .HasMaxLength(50)
            .HasComment("Icon identifier for the category");

        builder.Property(c => c.Color)
            .HasMaxLength(7)
            .HasComment("Hex color code for the category");

        builder.Property(c => c.CategoryType)
            .IsRequired()
            .HasConversion<int>()
            .HasComment("Type of category (Income/Expense)");

        builder.Property(c => c.IsSystem)
            .HasDefaultValue(false)
            .HasComment("Whether this is a system-defined category");

        builder.Property(c => c.IsActive)
            .HasDefaultValue(true)
            .HasComment("Whether the category is active");

        builder.Property(c => c.SortOrder)
            .HasDefaultValue(0)
            .HasComment("Sort order for displaying categories");

        // Self-referencing relationship for hierarchical categories
        builder.HasOne(c => c.ParentCategory)
            .WithMany(c => c.SubCategories)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Categories_ParentCategory");

        // Navigation properties
        builder.HasMany(c => c.Transactions)
            .WithOne(t => t.Category)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Budgets)
            .WithOne(b => b.Category)
            .HasForeignKey(b => b.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        // Ignore computed properties that are not navigation properties
        builder.Ignore(c => c.ActiveTransactions);
        builder.Ignore(c => c.ActiveSubCategories);
        builder.Ignore(c => c.FullPath);
        builder.Ignore(c => c.Level);
        builder.Ignore(c => c.HasSubCategories);
        builder.Ignore(c => c.BudgetUtilizationPercentage);

        // Indexes
        builder.HasIndex(c => c.ParentCategoryId)
            .HasDatabaseName("IX_Categories_ParentCategoryId");

        builder.HasIndex(c => c.CategoryType)
            .HasDatabaseName("IX_Categories_CategoryType");

        builder.HasIndex(c => new { c.Name, c.ParentCategoryId })
            .IsUnique()
            .HasDatabaseName("IX_Categories_Name_ParentId_Unique");

        builder.HasIndex(c => c.SyncStatus)
            .HasDatabaseName("IX_Categories_SyncStatus");

        builder.HasIndex(c => new { c.IsActive, c.CategoryType, c.SortOrder })
            .HasDatabaseName("IX_Categories_Active_Type_Sort");

        // Check constraints
        builder.HasCheckConstraint("CK_Categories_Color_Format", "Color IS NULL OR (LENGTH(Color) = 7 AND Color LIKE '#%')");
        builder.HasCheckConstraint("CK_Categories_SortOrder_NonNegative", "SortOrder >= 0");

        // Table configuration
        builder.ToTable("Categories", t => t.HasComment("Transaction categories table with hierarchical support"));
    }
}