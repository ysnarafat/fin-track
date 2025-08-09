using FinTrack.Core.Entities;

namespace FinTrack.Maui.Models;

/// <summary>
/// Data model for dashboard summary information
/// </summary>
public class DashboardData
{
    /// <summary>
    /// Total balance across all accounts
    /// </summary>
    public decimal TotalBalance { get; set; }
    
    /// <summary>
    /// Total income for the current month
    /// </summary>
    public decimal MonthlyIncome { get; set; }
    
    /// <summary>
    /// Total expenses for the current month
    /// </summary>
    public decimal MonthlyExpenses { get; set; }
    
    /// <summary>
    /// Net savings for the current month (income - expenses)
    /// </summary>
    public decimal MonthlySavings => MonthlyIncome - MonthlyExpenses;
    
    /// <summary>
    /// Savings rate as a percentage (savings / income)
    /// </summary>
    public decimal SavingsRate => MonthlyIncome > 0 ? (MonthlySavings / MonthlyIncome) : 0;
    
    /// <summary>
    /// Recent transactions to display on the dashboard
    /// </summary>
    public IEnumerable<Transaction> RecentTransactions { get; set; } = new List<Transaction>();
    
    /// <summary>
    /// Number of active accounts
    /// </summary>
    public int AccountCount { get; set; }
    
    /// <summary>
    /// Trend data for comparison with previous month
    /// </summary>
    public DashboardTrends Trends { get; set; } = new DashboardTrends();
}

/// <summary>
/// Trend data for dashboard metrics
/// </summary>
public class DashboardTrends
{
    /// <summary>
    /// Balance change percentage from previous month
    /// </summary>
    public decimal? BalanceTrend { get; set; }
    
    /// <summary>
    /// Income change percentage from previous month
    /// </summary>
    public decimal? IncomeTrend { get; set; }
    
    /// <summary>
    /// Expense change percentage from previous month
    /// </summary>
    public decimal? ExpenseTrend { get; set; }
    
    /// <summary>
    /// Savings rate change from previous month
    /// </summary>
    public decimal? SavingsRateTrend { get; set; }
}