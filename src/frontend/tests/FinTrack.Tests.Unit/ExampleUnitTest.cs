using Moq;

namespace FinTrack.Tests.Unit;

/// <summary>
/// Example unit test to verify the testing infrastructure is properly configured.
/// This test demonstrates the use of xUnit and Moq frameworks.
/// </summary>
public class ExampleUnitTest
{
    [Fact]
    public void ExampleTest_ShouldPass()
    {
        // Arrange
        var expected = "Hello, World!";
        
        // Act
        var actual = "Hello, World!";
        
        // Assert
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public void ExampleMockTest_ShouldDemonstrateMonqUsage()
    {
        // Arrange
        var mockService = new Mock<IExampleService>();
        mockService.Setup(x => x.GetMessage()).Returns("Mocked message");
        
        // Act
        var result = mockService.Object.GetMessage();
        
        // Assert
        Assert.Equal("Mocked message", result);
        mockService.Verify(x => x.GetMessage(), Times.Once);
    }
}

/// <summary>
/// Example interface for demonstrating mocking capabilities
/// </summary>
public interface IExampleService
{
    string GetMessage();
}