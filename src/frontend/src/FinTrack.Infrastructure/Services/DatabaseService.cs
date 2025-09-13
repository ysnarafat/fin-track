using FinTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinTrack.Infrastructure.Services;

/// <summary>
/// Service for managing database operations like initialization and migrations
/// </summary>
public class DatabaseService
{
    private readonly DatabaseInitializationService _initializationService;
    private readonly DataSeedingService _seedingService;
    private readonly SampleDataService _sampleDataService;
    private readonly FinTrackDbContext _context;
    private readonly ILogger<DatabaseService> _logger;

    public DatabaseService(
        DatabaseInitializationService initializationService,
        DataSeedingService seedingService,
        SampleDataService sampleDataService,
        FinTrackDbContext context,
        ILogger<DatabaseService> logger)
    {
        _initializationService = initializationService;
        _seedingService = seedingService;
        _sampleDataService = sampleDataService;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Initializes the database with migrations and default data
    /// </summary>
    /// <param name="includeSampleData">Whether to include sample data for development/testing</param>
    public async Task InitializeDatabaseAsync(bool includeSampleData = false)
    {
        try
        {
            _logger.LogInformation("Starting complete database initialization");

            await _initializationService.InitializeDatabaseAsync(includeSampleData);

            if (includeSampleData)
            {
                await _sampleDataService.CreateSampleDataAsync();
            }

            _logger.LogInformation("Complete database initialization finished successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during complete database initialization");
            throw;
        }
    }

    /// <summary>
    /// Applies any pending migrations to the database
    /// </summary>
    public async Task MigrateDatabaseAsync()
    {
        try
        {
            await _initializationService.ApplyMigrationsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while applying migrations");
            throw;
        }
    }

    /// <summary>
    /// Seeds default data (categories, default account)
    /// </summary>
    public async Task SeedDefaultDataAsync()
    {
        try
        {
            await _seedingService.SeedAllDataAsync(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while seeding default data");
            throw;
        }
    }

    /// <summary>
    /// Creates sample data for development and testing
    /// </summary>
    public async Task CreateSampleDataAsync()
    {
        try
        {
            await _sampleDataService.CreateSampleDataAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating sample data");
            throw;
        }
    }

    /// <summary>
    /// Clears all sample data from the database
    /// </summary>
    public async Task ClearSampleDataAsync()
    {
        try
        {
            await _sampleDataService.ClearSampleDataAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while clearing sample data");
            throw;
        }
    }

    /// <summary>
    /// Checks if the database exists and is accessible
    /// </summary>
    public async Task<bool> CanConnectAsync()
    {
        try
        {
            return await _context.Database.CanConnectAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking database connection");
            return false;
        }
    }

    /// <summary>
    /// Gets comprehensive database information for diagnostics
    /// </summary>
    public async Task<DatabaseInfo> GetDatabaseInfoAsync()
    {
        try
        {
            var info = await _initializationService.GetDatabaseInfoAsync();
            info.DatabasePath = _context.Database.GetConnectionString();
            return info;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting database info");
            return new DatabaseInfo
            {
                CanConnect = false,
                AppliedMigrations = new List<string>(),
                PendingMigrations = new List<string>(),
                DatabasePath = "Unknown"
            };
        }
    }

    /// <summary>
    /// Validates database integrity
    /// </summary>
    public async Task<bool> ValidateDatabaseIntegrityAsync()
    {
        try
        {
            return await _initializationService.ValidateDatabaseIntegrityAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while validating database integrity");
            return false;
        }
    }

    /// <summary>
    /// Recreates the database (for development/testing purposes)
    /// </summary>
    public async Task RecreateDatabase(bool includeSampleData = false)
    {
        try
        {
            await _initializationService.RecreateDatabase(includeSampleData);
            
            if (includeSampleData)
            {
                await _sampleDataService.CreateSampleDataAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while recreating database");
            throw;
        }
    }
}