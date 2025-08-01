using Microsoft.EntityFrameworkCore;

namespace FinTrack.Tests.Integration;

/// <summary>
/// Example integration test to verify the testing infrastructure is properly configured.
/// This test demonstrates the use of Entity Framework Core In-Memory database.
/// </summary>
public class ExampleIntegrationTest
{
    [Fact]
    public void ExampleTest_ShouldPass()
    {
        // Arrange
        var expected = "Integration test working!";
        
        // Act
        var actual = "Integration test working!";
        
        // Assert
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public void ExampleDatabaseTest_ShouldDemonstrateInMemoryDatabase()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ExampleDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            
        using var context = new ExampleDbContext(options);
        
        // Act
        var entity = new ExampleEntity { Name = "Test Entity" };
        context.ExampleEntities.Add(entity);
        context.SaveChanges();
        
        var retrievedEntity = context.ExampleEntities.First();
        
        // Assert
        Assert.Equal("Test Entity", retrievedEntity.Name);
        Assert.True(retrievedEntity.Id > 0);
    }
}

/// <summary>
/// Example DbContext for demonstrating Entity Framework Core integration testing
/// </summary>
public class ExampleDbContext : DbContext
{
    public ExampleDbContext(DbContextOptions<ExampleDbContext> options) : base(options)
    {
    }
    
    public DbSet<ExampleEntity> ExampleEntities { get; set; }
}

/// <summary>
/// Example entity for demonstrating database operations
/// </summary>
public class ExampleEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}