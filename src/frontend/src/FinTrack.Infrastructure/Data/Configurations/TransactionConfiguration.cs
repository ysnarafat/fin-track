using FinTrack.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTrack.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for Transaction entity
/// </summary>
public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        // Primary key
        builder.HasKey(t => t.Id);

        // Properties
        builder.Property(t => t.Amount)
            .IsRequired()
            .HasColumnType("decimal(18,2)")
            .HasComment("Transaction amount in the account's currency");

        builder.Property(t => t.Description)
            .IsRequired()
            .HasMaxLength(500)
            .HasComment("Description of the transaction");

        builder.Property(t => t.Date)
            .IsRequired()
            .HasComment("Date when the transaction occurred");

        builder.Property(t => t.Type)
            .IsRequired()
            .HasConversion<int>()
            .HasComment("Type of transaction (Income/Expense)");

        builder.Property(t => t.Notes)
            .HasMaxLength(1000)
            .HasComment("Additional notes for the transaction");

        builder.Property(t => t.ReferenceNumber)
            .HasMaxLength(50)
            .HasComment("External reference number (e.g., check number, confirmation code)");

        // Relationships
        builder.HasOne(t => t.Account)
            .WithMany(a => a.Transactions)
            .HasForeignKey(t => t.AccountId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Transactions_Accounts");

        builder.HasOne(t => t.Category)
            .WithMany(c => c.Transactions)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Transactions_Categories");

        // Indexes for performance
        builder.HasIndex(t => t.AccountId)
            .HasDatabaseName("IX_Transactions_AccountId");

        builder.HasIndex(t => t.CategoryId)
            .HasDatabaseName("IX_Transactions_CategoryId");

        builder.HasIndex(t => t.Date)
            .HasDatabaseName("IX_Transactions_Date");

        builder.HasIndex(t => t.Type)
            .HasDatabaseName("IX_Transactions_Type");

        builder.HasIndex(t => new { t.Date, t.AccountId })
            .HasDatabaseName("IX_Transactions_Date_AccountId");

        builder.HasIndex(t => t.SyncStatus)
            .HasDatabaseName("IX_Transactions_SyncStatus");

        // Table configuration
        builder.ToTable("Transactions", t => t.HasComment("Financial transactions table"));
    }
}