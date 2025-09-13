using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FinTrack.Infrastructure.Data;
using FinTrack.Infrastructure.Services;
using Moq;

namespace FinTrack.Tests.Unit;

/// <summary>
/// Configuration class for setting up test dependencies and services
/// </summary>
public static class TestConfiguration
{
    /// <summary>
    /// Creates an in-memory database context for testing
    /// </summary>
    /// <param name="databaseName">Optional database name for isolation</param>
    /// <returns>Configured DbContext for testing</returns>
    public static FinTrackDbContext CreateInMemoryDbContext(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<FinTrackDbContext>()
            .UseInMemoryDatabase(databaseName: databaseName ?? Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        var mockLogger = new Mock<ILogger<FinTrackDbContext>>();
        return new FinTrackDbContext(options, mockLogger.Object);
    }

    /// <summary>
    /// Creates a service collection with common test dependencies
    /// </summary>
    /// <returns>Configured service collection</returns>
    public static IServiceCollection CreateTestServices()
    {
        var services = new ServiceCollection();
        
        // Add logging
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
        
        // Add in-memory database
        services.AddDbContext<FinTrackDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        
        return services;
    }

    /// <summary>
    /// Creates a service provider with test dependencies
    /// </summary>
    /// <returns>Configured service provider</returns>
    public static IServiceProvider CreateTestServiceProvider()
    {
        return CreateTestServices().BuildServiceProvider();
    }

    /// <summary>
    /// Seeds test data into the provided database context
    /// </summary>
    /// <param name="context">Database context to seed</param>
    public static async Task SeedTestDataAsync(FinTrackDbContext context)
    {
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();
        
        // Create and use DataSeedingService to seed default data
        var mockLogger = new Mock<ILogger<FinTrack.Infrastructure.Services.DataSeedingService>>();
        var seedingService = new FinTrack.Infrastructure.Services.DataSeedingService(context, mockLogger.Object);
        
        // Seed default categories and accounts
        await seedingService.SeedAllDataAsync(includeSampleData: true);
    }

    /// <summary>
    /// Cleans up test data from the database context
    /// </summary>
    /// <param name="context">Database context to clean</param>
    public static async Task CleanupTestDataAsync(FinTrackDbContext context)
    {
        // Remove all test data
        context.Transactions.RemoveRange(context.Transactions);
        context.Accounts.RemoveRange(context.Accounts);
        context.Categories.RemoveRange(context.Categories);
        context.Budgets.RemoveRange(context.Budgets);
        context.Goals.RemoveRange(context.Goals);
        context.GoalMilestones.RemoveRange(context.GoalMilestones);
        
        await context.SaveChangesAsync();
    }
}

/// <summary>
/// Base class for tests that require database access
/// </summary>
public abstract class DatabaseTestBase : IDisposable
{
    protected readonly FinTrackDbContext Context;
    protected readonly IServiceProvider ServiceProvider;

    protected DatabaseTestBase()
    {
        ServiceProvider = TestConfiguration.CreateTestServiceProvider();
        Context = ServiceProvider.GetRequiredService<FinTrackDbContext>();
    }

    /// <summary>
    /// Seeds the database with test data
    /// </summary>
    protected async Task SeedDatabaseAsync()
    {
        await TestConfiguration.SeedTestDataAsync(Context);
    }

    /// <summary>
    /// Cleans up the database
    /// </summary>
    protected async Task CleanupDatabaseAsync()
    {
        await TestConfiguration.CleanupTestDataAsync(Context);
    }

    public virtual void Dispose()
    {
        Context?.Dispose();
        if (ServiceProvider is IDisposable disposableProvider)
        {
            disposableProvider.Dispose();
        }
    }
}

/// <summary>
/// Attribute to mark tests that require database access
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class DatabaseTestAttribute : Attribute
{
    public bool SeedData { get; set; } = true;
    public bool CleanupAfter { get; set; } = true;
}

/// <summary>
/// Custom test collection for database tests to ensure proper isolation
/// </summary>
[CollectionDefinition("Database Tests")]
public class DatabaseTestCollection : ICollectionFixture<DatabaseTestFixture>
{
}

/// <summary>
/// Test fixture for database tests
/// </summary>
public class DatabaseTestFixture : IDisposable
{
    public FinTrackDbContext CreateContext()
    {
        return TestConfiguration.CreateInMemoryDbContext();
    }

    public void Dispose()
    {
        // Cleanup any shared resources if needed
    }
}