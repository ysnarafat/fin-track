using FinTrack.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTrack.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for GoalMilestone entity
/// </summary>
public class GoalMilestoneConfiguration : IEntityTypeConfiguration<GoalMilestone>
{
    public void Configure(EntityTypeBuilder<GoalMilestone> builder)
    {
        // Primary key
        builder.HasKey(gm => gm.Id);

        // Properties
        builder.Property(gm => gm.Name)
            .IsRequired()
            .HasMaxLength(100)
            .HasComment("Name of the milestone");

        builder.Property(gm => gm.Description)
            .HasMaxLength(500)
            .HasComment("Description of the milestone");

        builder.Property(gm => gm.TargetAmount)
            .IsRequired()
            .HasColumnType("decimal(18,2)")
            .HasComment("Target amount for this milestone");

        builder.Property(gm => gm.TargetDate)
            .HasComment("Target date for achieving this milestone");

        builder.Property(gm => gm.IsAchieved)
            .HasDefaultValue(false)
            .HasComment("Whether the milestone has been achieved");

        builder.Property(gm => gm.AchievedDate)
            .HasComment("Date when the milestone was achieved");

        builder.Property(gm => gm.SortOrder)
            .HasDefaultValue(0)
            .HasComment("Sort order for displaying milestones");

        // Relationships
        builder.HasOne(gm => gm.Goal)
            .WithMany(g => g.Milestones)
            .HasForeignKey(gm => gm.GoalId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_GoalMilestones_Goals");

        // Indexes
        builder.HasIndex(gm => gm.GoalId)
            .HasDatabaseName("IX_GoalMilestones_GoalId");

        builder.HasIndex(gm => gm.SyncStatus)
            .HasDatabaseName("IX_GoalMilestones_SyncStatus");

        builder.HasIndex(gm => new { gm.GoalId, gm.SortOrder })
            .HasDatabaseName("IX_GoalMilestones_Goal_SortOrder");

        builder.HasIndex(gm => new { gm.IsAchieved, gm.TargetDate })
            .HasDatabaseName("IX_GoalMilestones_Achieved_TargetDate");

        // Check constraints
        builder.HasCheckConstraint("CK_GoalMilestones_TargetAmount_Positive", "TargetAmount > 0");
        builder.HasCheckConstraint("CK_GoalMilestones_SortOrder_NonNegative", "SortOrder >= 0");
        builder.HasCheckConstraint("CK_GoalMilestones_AchievedDate_Logic", "AchievedDate IS NULL OR IsAchieved = 1");

        // Table configuration
        builder.ToTable("GoalMilestones", t => t.HasComment("Goal milestones tracking table"));
    }
}