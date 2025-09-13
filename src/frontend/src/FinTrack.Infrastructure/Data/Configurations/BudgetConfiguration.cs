using FinTrack.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTrack.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for Budget entity
/// </summary>
public class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> builder)
    {
        // Primary key
        builder.HasKey(b => b.Id);

        // Properties
        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(100)
            .HasComment("Name of the budget");

        builder.Property(b => b.Amount)
            .IsRequired()
            .HasColumnType("decimal(18,2)")
            .HasComment("Budget amount limit");

        builder.Property(b => b.SpentAmount)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0)
            .HasComment("Amount spent against this budget");

        builder.Property(b => b.Period)
            .IsRequired()
            .HasConversion<int>()
            .HasComment("Budget period (Monthly, Quarterly, Yearly)");

        builder.Property(b => b.StartDate)
            .IsRequired()
            .HasComment("Start date of the budget period");

        builder.Property(b => b.EndDate)
            .IsRequired()
            .HasComment("End date of the budget period");

        builder.Property(b => b.IsActive)
            .HasDefaultValue(true)
            .HasComment("Whether the budget is active");

        builder.Property(b => b.AlertThreshold)
            .HasColumnType("decimal(18,2)")
            .HasComment("Threshold percentage for budget alerts (0-100)");

        // Relationships
        builder.HasOne(b => b.Category)
            .WithMany(c => c.Budgets)
            .HasForeignKey(b => b.CategoryId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("FK_Budgets_Categories");

        // Indexes
        builder.HasIndex(b => b.CategoryId)
            .HasDatabaseName("IX_Budgets_CategoryId");

        builder.HasIndex(b => new { b.StartDate, b.EndDate })
            .HasDatabaseName("IX_Budgets_StartDate_EndDate");

        builder.HasIndex(b => b.SyncStatus)
            .HasDatabaseName("IX_Budgets_SyncStatus");

        builder.HasIndex(b => new { b.IsActive, b.StartDate, b.EndDate })
            .HasDatabaseName("IX_Budgets_Active_DateRange");

        // Check constraints
        builder.HasCheckConstraint("CK_Budgets_Amount_Positive", "Amount > 0");
        builder.HasCheckConstraint("CK_Budgets_SpentAmount_NonNegative", "SpentAmount >= 0");
        builder.HasCheckConstraint("CK_Budgets_DateRange_Valid", "EndDate > StartDate");
        builder.HasCheckConstraint("CK_Budgets_AlertThreshold_Range", "AlertThreshold IS NULL OR (AlertThreshold >= 0 AND AlertThreshold <= 100)");

        // Table configuration
        builder.ToTable("Budgets", t => t.HasComment("Budget tracking table"));
    }
}