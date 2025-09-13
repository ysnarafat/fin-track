using FinTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinTrack.Tests.Unit.Infrastructure;

/// <summary>
/// Base class for database context tests providing common setup and utilities
/// </summary>
public abstract class DbContextTestBase : IDisposable
{
    protected readonly DbContextOptions<FinTrackDbContext> Options;
    protected readonly Mock<ILogger<FinTrackDbContext>> MockLogger;
    protected FinTrackDbContext Context;

    protected DbContextTestBase()
    {
        // Use in-memory database with unique name for each test
        Options = new DbContextOptionsBuilder<FinTrackDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        MockLogger = new Mock<ILogger<FinTrackDbContext>>();
        Context = new FinTrackDbContext(Options, MockLogger.Object);
    }

    /// <summary>
    /// Creates a new context instance (useful for testing concurrent scenarios)
    /// </summary>
    protected FinTrackDbContext CreateNewContext()
    {
        return new FinTrackDbContext(Options, MockLogger.Object);
    }

    /// <summary>
    /// Ensures the database is created and seeded
    /// </summary>
    protected async Task EnsureDatabaseCreatedAsync()
    {
        await Context.Database.EnsureCreatedAsync();
    }

    /// <summary>
    /// Clears all data from the database
    /// </summary>
    protected async Task ClearDatabaseAsync()
    {
        // Remove all entities in reverse dependency order
        Context.GoalMilestones.RemoveRange(Context.GoalMilestones.IgnoreQueryFilters());
        Context.Goals.RemoveRange(Context.Goals.IgnoreQueryFilters());
        Context.Budgets.RemoveRange(Context.Budgets.IgnoreQueryFilters());
        Context.Transactions.RemoveRange(Context.Transactions.IgnoreQueryFilters());
        Context.Categories.RemoveRange(Context.Categories.IgnoreQueryFilters());
        Context.Accounts.RemoveRange(Context.Accounts.IgnoreQueryFilters());

        await Context.SaveChangesAsync();
    }

    /// <summary>
    /// Detaches all tracked entities from the context
    /// </summary>
    protected void DetachAllEntities()
    {
        var entries = Context.ChangeTracker.Entries().ToList();
        foreach (var entry in entries)
        {
            entry.State = EntityState.Detached;
        }
    }

    /// <summary>
    /// Verifies that a specific log message was written
    /// </summary>
    protected void VerifyLogMessage(LogLevel logLevel, string messageContains, Times times)
    {
        MockLogger.Verify(
            x => x.Log(
                logLevel,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(messageContains)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times);
    }

    /// <summary>
    /// Verifies that an error log was written
    /// </summary>
    protected void VerifyErrorLogged(string messageContains = "Error occurred while saving changes")
    {
        VerifyLogMessage(LogLevel.Error, messageContains, Times.Once());
    }

    /// <summary>
    /// Verifies that a debug log was written
    /// </summary>
    protected void VerifyDebugLogged(string messageContains = "Successfully saved")
    {
        VerifyLogMessage(LogLevel.Debug, messageContains, Times.Once());
    }

    /// <summary>
    /// Disposes the context and cleans up resources
    /// </summary>
    public virtual void Dispose()
    {
        Context?.Dispose();
        GC.SuppressFinalize(this);
    }
}