using FinTrack.Infrastructure.Data;
using FinTrack.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit;

namespace FinTrack.Tests.Unit.Infrastructure.Services;

public class DatabaseInitializationServiceTests : IDisposable
{
    private readonly FinTrackDbContext _context;
    private readonly DatabaseInitializationService _service;
    private readonly DataSeedingService _seedingService;

    public DatabaseInitializationServiceTests()
    {
        var options = new DbContextOptionsBuilder<FinTrackDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new FinTrackDbContext(options);
        
        var logger = new LoggerFactory().CreateLogger<DatabaseInitializationService>();
        var seedingLogger = new LoggerFactory().CreateLogger<DataSeedingService>();
        
        _seedingService = new DataSeedingService(_context, seedingLogger);
        _service = new DatabaseInitializationService(_context, _seedingService, logger);
    }

    [Fact]
    public async Task InitializeDatabaseAsync_ShouldCreateDatabaseAndSeedData()
    {
        // Act
        await _service.InitializeDatabaseAsync(false);

        // Assert
        Assert.True(await _context.Database.CanConnectAsync());
        
        var categories = await _context.Categories.CountAsync();
        var accounts = await _context.Accounts.CountAsync();
        
        Assert.True(categories > 0, "Categories should be seeded");
        Assert.True(accounts > 0, "At least one account should be seeded");
    }

    [Fact]
    public async Task GetDatabaseInfoAsync_ShouldReturnCorrectInformation()
    {
        // Arrange
        await _service.InitializeDatabaseAsync(false);

        // Act
        var info = await _service.GetDatabaseInfoAsync();

        // Assert
        Assert.True(info.CanConnect);
        Assert.True(info.IsSeeded);
        Assert.True(info.CategoryCount > 0);
        Assert.True(info.AccountCount > 0);
    }

    [Fact]
    public async Task ValidateDatabaseIntegrityAsync_ShouldReturnTrueForValidDatabase()
    {
        // Arrange
        await _service.InitializeDatabaseAsync(false);

        // Act
        var isValid = await _service.ValidateDatabaseIntegrityAsync();

        // Assert
        Assert.True(isValid);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}