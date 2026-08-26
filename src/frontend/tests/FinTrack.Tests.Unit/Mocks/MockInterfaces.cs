using System.ComponentModel;
using FinTrack.Core.Interfaces;
using FinTrack.Core.Enums;

namespace FinTrack.Tests.Unit.Mocks;

// Color structure for UI testing
public struct Color
{
    public int R { get; }
    public int G { get; }
    public int B { get; }

    public Color(int r, int g, int b)
    {
        R = r;
        G = g;
        B = b;
    }

    public static bool operator ==(Color left, Color right)
    {
        return left.R == right.R && left.G == right.G && left.B == right.B;
    }

    public static bool operator !=(Color left, Color right)
    {
        return !(left == right);
    }

    public override bool Equals(object? obj)
    {
        return obj is Color color && this == color;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(R, G, B);
    }
}

public static class Colors
{
    public static readonly Color Green = new Color(0, 255, 0);
    public static readonly Color Orange = new Color(255, 165, 0);
    public static readonly Color Red = new Color(255, 0, 0);
    public static readonly Color Blue = new Color(0, 0, 255);
}

// Budget-related interfaces and models
public interface IBudgetService
{
    Task<IEnumerable<Budget>> GetCurrentMonthBudgetsAsync();
    Task<BudgetSummary> GetBudgetSummaryAsync();
    Task<IEnumerable<BudgetAlert>> GetBudgetAlertsAsync();
    Task<bool> DeleteBudgetAsync(int budgetId);
}

public class Budget
{
    public int Id { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Spent { get; set; }
    public decimal Remaining => Amount - Spent;
    public decimal ProgressPercentage => Amount > 0 ? (Spent / Amount) * 100 : 0;
}

public class BudgetSummary
{
    public decimal TotalBudget { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal TotalRemaining { get; set; }
    public int BudgetCount { get; set; }
    public int OverBudgetCount { get; set; }
}

public class BudgetAlert
{
    public int Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public AlertType Type { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum AlertType
{
    Warning,
    Critical,
    Info
}

// Note: Sync-related classes are defined in FinTrack.Core.Interfaces.ISyncService

// Application and Shell mocks for navigation testing
public static class Application
{
    public static MockPage? Current { get; set; } = new MockPage();
}

public class MockPage
{
    public MockPage? MainPage { get; set; } = new MockPage();
    
    public Task<bool> DisplayAlert(string title, string message, string accept, string cancel)
    {
        return Task.FromResult(true);
    }
    
    public Task DisplayAlert(string title, string message, string cancel)
    {
        return Task.CompletedTask;
    }
    
    public Task<string> DisplayActionSheet(string title, string cancel, string? destruction, params string[] buttons)
    {
        return Task.FromResult(buttons.FirstOrDefault() ?? cancel);
    }
}

public static class Shell
{
    public static MockShell Current { get; set; } = new MockShell();
}

public class MockShell
{
    public Task GoToAsync(string route)
    {
        return Task.CompletedTask;
    }
}

public static class MainThread
{
    public static void BeginInvokeOnMainThread(Action action)
    {
        action?.Invoke();
    }
}