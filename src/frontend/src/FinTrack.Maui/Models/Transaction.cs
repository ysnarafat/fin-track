namespace FinTrack.Maui.Models;

public class SimpleTransaction
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Category { get; set; } = string.Empty;
    public SimpleTransactionType Type { get; set; }
}

public enum SimpleTransactionType
{
    Income,
    Expense
}