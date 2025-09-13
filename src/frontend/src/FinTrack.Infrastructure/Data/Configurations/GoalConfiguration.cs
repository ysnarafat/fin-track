using FinTrack.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTrack.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for Goal entity
/// </summary>
public class GoalConfiguration : IEntityTypeConfiguration<Goal>
{
    public void Configure(EntityTypeBuilder<Goal> builder)
    {
        // Primary key
        builder.HasKey(g => g.Id);

        // Properties
        builder.Property(g => g.Name)
            .IsRequired()
            .HasMaxLength(100)
            .HasComment("Name of the goal");

        builder.Property(g => g.Description)
            .HasMaxLength(500)
            .HasComment("Description of the goal");

        builder.Property(g => g.TargetAmount)
            .IsRequired()
            .HasColumnType("decimal(18,2)")
            .HasComment("Target amount to achieve");

        builder.Property(g => g.CurrentAmount)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0)
            .HasComment("Current progress amount");

        builder.Property(g => g.TargetDate)
            .IsRequired()
            .HasComment("Target date to achieve the goal");

        builder.Property(g => g.Type)
            .IsRequired()
            .HasConversion<int>()
            .HasComment("Type of goal (Savings, Debt Payoff, Investment)");

        builder.Property(g => g.Priority)
            .IsRequired()
            .HasDefaultValue(3)
            .HasComment("Priority level (1=High, 2=Medium, 3=Low)");

        builder.Property(g => g.Color)
            .HasMaxLength(7)
            .HasDefaultValue("#3B82F6")
            .HasComment("Hex color code for the goal");

        builder.Property(g => g.IsCompleted)
            .HasDefaultValue(false)
            .HasComment("Whether the goal has been completed");

        builder.Property(g => g.CompletedDate)
            .HasComment("Date when the goal was completed");

        // Relationships
        builder.HasOne(g => g.LinkedAccount)
            .WithMany(a => a.Goals)
            .HasForeignKey(g => g.LinkedAccountId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("FK_Goals_Accounts");

        builder.HasMany(g => g.Milestones)
            .WithOne(gm => gm.Goal)
            .HasForeignKey(gm => gm.GoalId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(g => g.LinkedAccountId)
            .HasDatabaseName("IX_Goals_LinkedAccountId");

        builder.HasIndex(g => g.TargetDate)
            .HasDatabaseName("IX_Goals_TargetDate");

        builder.HasIndex(g => g.SyncStatus)
            .HasDatabaseName("IX_Goals_SyncStatus");

        builder.HasIndex(g => new { g.IsCompleted, g.TargetDate })
            .HasDatabaseName("IX_Goals_Completed_TargetDate");

        builder.HasIndex(g => new { g.Type, g.Priority })
            .HasDatabaseName("IX_Goals_Type_Priority");

        // Check constraints
        builder.HasCheckConstraint("CK_Goals_TargetAmount_Positive", "TargetAmount > 0");
        builder.HasCheckConstraint("CK_Goals_CurrentAmount_NonNegative", "CurrentAmount >= 0");
        builder.HasCheckConstraint("CK_Goals_Priority_Range", "Priority >= 1 AND Priority <= 5");
        builder.HasCheckConstraint("CK_Goals_Color_Format", "Color IS NULL OR (LENGTH(Color) = 7 AND Color LIKE '#%')");
        builder.HasCheckConstraint("CK_Goals_CompletedDate_Logic", "CompletedDate IS NULL OR IsCompleted = 1");

        // Table configuration
        builder.ToTable("Goals", t => t.HasComment("Financial goals tracking table"));
    }
}