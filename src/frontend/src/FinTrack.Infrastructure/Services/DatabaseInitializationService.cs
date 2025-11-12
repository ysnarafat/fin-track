using FinTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinTrack.Infrastructure.Services;

/// <summary>
/// Service responsible for database initialization, migrations, and setup
/// </summary>
public class DatabaseInitializationService
{
    private readonly FinTrackDbContext _context;
    private readonly DataSeedingService _seedingService;
    private readonly ILogger<DatabaseInitializationService> _logger;

    public DatabaseInitializationService(
        FinTrackDbContext context,
        DataSeedingService seedingService,
        ILogger<DatabaseInitializationService> logger)
    {
        _context = context;
        _seedingService = seedingService;
        _logger = logger;
    }

    /// <summary>
    /// Initializes the database with migrations and default data
    /// </summary>
    /// <param name="seedSampleData">Whether to include sample data for development/testing</param>
    public async Task InitializeDatabaseAsync(bool seedSampleData = false)
    {
        try
        {
            _logger.LogInformation("Starting database initialization");

            // Ensure database is created and migrations are applied
            await EnsureDatabaseCreatedAsync();

            // Apply any pending migrations
            await ApplyMigrationsAsync();

            // Seed default data if needed
            if (!await _seedingService.IsDataSeededAsync())
            {
                await _seedingService.SeedAllDataAsync(seedSampleData);
            }
            else
            {
                _logger.LogInformation("Database already contains data, skipping seeding");
            }

            _logger.LogInformation("Database initialization completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during database initialization");
            throw;
        }
    }

    /// <summary>
    /// Ensures the database is created
    /// </summary>
    public async Task EnsureDatabaseCreatedAsync()
    {
        try
        {
            _logger.LogInformation("Ensuring database is created");
            
            var created = await _context.Database.EnsureCreatedAsync();
            
            if (created)
            {
                _logger.LogInformation("Database was created successfully");
            }
            else
            {
                _logger.LogInformation("Database already exists");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ensuring database creation");
            throw;
        }
    }

    /// <summary>
    /// Applies any pending database migrations
    /// </summary>
    public async Task ApplyMigrationsAsync()
    {
        try
        {
            // Skip migrations for in-memory databases
            if (_context.Database.IsInMemory())
            {
                _logger.LogInformation("Skipping migrations for in-memory database");
                return;
            }

            _logger.LogInformation("Checking for pending migrations");

            var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
            var pendingMigrationsList = pendingMigrations.ToList();

            if (pendingMigrationsList.Any())
            {
                _logger.LogInformation("Applying {Count} pending migrations: {Migrations}", 
                    pendingMigrationsList.Count, string.Join(", ", pendingMigrationsList));

                await _context.Database.MigrateAsync();
                
                _logger.LogInformation("Migrations applied successfully");
            }
            else
            {
                _logger.LogInformation("No pending migrations found");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying database migrations");
            throw;
        }
    }

    /// <summary>
    /// Gets information about the current database state
    /// </summary>
    public async Task<DatabaseInfo> GetDatabaseInfoAsync()
    {
        try
        {
            var canConnect = await _context.Database.CanConnectAsync();
            
            // Skip migration queries for in-memory databases
            var appliedMigrations = _context.Database.IsInMemory() 
                ? new List<string>() 
                : (await _context.Database.GetAppliedMigrationsAsync()).ToList();
            
            var pendingMigrations = _context.Database.IsInMemory() 
                ? new List<string>() 
                : (await _context.Database.GetPendingMigrationsAsync()).ToList();

            var info = new DatabaseInfo
            {
                CanConnect = canConnect,
                AppliedMigrations = appliedMigrations,
                PendingMigrations = pendingMigrations,
                IsSeeded = await _seedingService.IsDataSeededAsync()
            };

            if (canConnect)
            {
                info.CategoryCount = await _context.Categories.CountAsync();
                info.AccountCount = await _context.Accounts.CountAsync();
                info.TransactionCount = await _context.Transactions.CountAsync();
                info.BudgetCount = await _context.Budgets.CountAsync();
                info.GoalCount = await _context.Goals.CountAsync();
            }

            return info;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting database information");
            throw;
        }
    }

    /// <summary>
    /// Recreates the database (for development/testing purposes)
    /// </summary>
    public async Task RecreateDatabase(bool seedSampleData = false)
    {
        try
        {
            _logger.LogWarning("Recreating database - all data will be lost");

            await _context.Database.EnsureDeletedAsync();
            await _context.Database.EnsureCreatedAsync();
            
            await _seedingService.SeedAllDataAsync(seedSampleData);

            _logger.LogInformation("Database recreated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recreating database");
            throw;
        }
    }

    /// <summary>
    /// Validates database integrity
    /// </summary>
    public async Task<bool> ValidateDatabaseIntegrityAsync()
    {
        try
        {
            _logger.LogInformation("Validating database integrity");

            // Check if we can connect
            if (!await _context.Database.CanConnectAsync())
            {
                _logger.LogError("Cannot connect to database");
                return false;
            }

            // Check if required tables exist by trying to query them
            await _context.Categories.CountAsync();
            await _context.Accounts.CountAsync();
            await _context.Transactions.CountAsync();
            await _context.Budgets.CountAsync();
            await _context.Goals.CountAsync();

            // Check if we have at least some default categories
            var categoryCount = await _context.Categories.CountAsync();
            if (categoryCount == 0)
            {
                _logger.LogWarning("No categories found in database");
                return false;
            }

            _logger.LogInformation("Database integrity validation passed");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database integrity validation failed");
            return false;
        }
    }
}

/// <summary>
/// Information about the current database state
/// </summary>
public class DatabaseInfo
{
    public bool CanConnect { get; set; }
    public List<string> AppliedMigrations { get; set; } = new();
    public List<string> PendingMigrations { get; set; } = new();
    public bool IsSeeded { get; set; }
    public int CategoryCount { get; set; }
    public int AccountCount { get; set; }
    public int TransactionCount { get; set; }
    public int BudgetCount { get; set; }
    public int GoalCount { get; set; }
    public string? DatabasePath { get; set; }
}