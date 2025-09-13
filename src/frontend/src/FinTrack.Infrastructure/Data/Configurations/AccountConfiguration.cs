using FinTrack.Core.Entities;
using FinTrack.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTrack.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for Account entity
/// </summary>
public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        // Primary key
        builder.HasKey(a => a.Id);

        // Properties
        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(100)
            .HasComment("Name of the account");

        builder.Property(a => a.Balance)
            .IsRequired()
            .HasColumnType("decimal(18,2)")
            .HasComment("Current balance of the account");

        builder.Property(a => a.InitialBalance)
            .IsRequired()
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0)
            .HasComment("Initial balance when the account was created");

        builder.Property(a => a.Type)
            .IsRequired()
            .HasConversion<int>()
            .HasComment("Type of account (Checking, Savings, Credit Card, etc.)");

        builder.Property(a => a.Currency)
            .IsRequired()
            .HasMaxLength(3)
            .HasDefaultValue("USD")
            .HasComment("Currency code (ISO 4217)");

        builder.Property(a => a.Description)
            .HasMaxLength(500)
            .HasComment("Description of the account");

        builder.Property(a => a.Institution)
            .HasMaxLength(100)
            .HasComment("Financial institution name");

        builder.Property(a => a.AccountNumber)
            .HasMaxLength(50)
            .HasComment("Account number (masked for security)");

        builder.Property(a => a.IsActive)
            .HasDefaultValue(true)
            .HasComment("Whether the account is active");

        // Navigation properties
        builder.HasMany(a => a.Transactions)
            .WithOne(t => t.Account)
            .HasForeignKey(t => t.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(a => a.Goals)
            .WithOne(g => g.LinkedAccount)
            .HasForeignKey(g => g.LinkedAccountId)
            .OnDelete(DeleteBehavior.SetNull);

        // Constraints
        builder.HasIndex(a => a.Name)
            .IsUnique()
            .HasDatabaseName("IX_Accounts_Name_Unique");

        builder.HasIndex(a => a.SyncStatus)
            .HasDatabaseName("IX_Accounts_SyncStatus");

        // Check constraints
        builder.HasCheckConstraint("CK_Accounts_Currency_Length", "LENGTH(Currency) = 3");

        // Table configuration
        builder.ToTable("Accounts", t => t.HasComment("Financial accounts table"));
    }
}