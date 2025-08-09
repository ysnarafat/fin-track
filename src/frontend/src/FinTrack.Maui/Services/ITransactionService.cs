using FinTrack.Maui.Models;

namespace FinTrack.Maui.Services;

public interface ITransactionService
{
    Task<List<SimpleTransaction>> GetTransactionsAsync();
    Task<SimpleTransaction?> GetTransactionByIdAsync(int id);
    Task<SimpleTransaction> CreateTransactionAsync(SimpleTransaction transaction);
    Task<SimpleTransaction> UpdateTransactionAsync(SimpleTransaction transaction);
    Task<bool> DeleteTransactionAsync(int id);
}