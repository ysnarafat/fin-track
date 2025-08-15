using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using FinTrack.Maui.Models;
using FinTrack.Maui.Services;

namespace FinTrack.Maui.ViewModels;

public class TransactionsViewModel : INotifyPropertyChanged
{
    private readonly ITransactionService _transactionService;
    private string _searchText = string.Empty;
    private string _selectedTransactionType = "All";
    private bool _isLoading = false;
    private ObservableCollection<TransactionViewModel> _transactions = new();
    private ObservableCollection<TransactionViewModel> _filteredTransactions = new();

    public TransactionsViewModel(ITransactionService transactionService)
    {
        _transactionService = transactionService;
        
        AddTransactionCommand = new Command(OnAddTransaction);
        EditTransactionCommand = new Command<TransactionViewModel>(OnEditTransaction);
        DeleteTransactionCommand = new Command<TransactionViewModel>(OnDeleteTransaction);
        ClearFiltersCommand = new Command(OnClearFilters);
        RefreshCommand = new Command(OnRefresh);
        
        LoadTransactions();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    // Properties
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText != value)
            {
                _searchText = value;
                OnPropertyChanged();
                UpdateFilteredTransactions();
            }
        }
    }

    public string SelectedTransactionType
    {
        get => _selectedTransactionType;
        set
        {
            if (_selectedTransactionType != value)
            {
                _selectedTransactionType = value;
                OnPropertyChanged();
                UpdateFilteredTransactions();
            }
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (_isLoading != value)
            {
                _isLoading = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotLoading));
            }
        }
    }

    public bool IsNotLoading => !IsLoading;

    public ObservableCollection<TransactionViewModel> Transactions
    {
        get => _transactions;
        set
        {
            _transactions = value;
            OnPropertyChanged();
            UpdateFilteredTransactions();
        }
    }

    public ObservableCollection<TransactionViewModel> FilteredTransactions
    {
        get => _filteredTransactions;
        private set
        {
            _filteredTransactions = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsEmpty));
        }
    }

    public List<string> TransactionTypes { get; } = new() { "All", "Income", "Expense" };

    public bool HasFilters => !string.IsNullOrWhiteSpace(SearchText) || SelectedTransactionType != "All";

    public bool IsEmpty => !IsLoading && !FilteredTransactions.Any();

    public decimal TotalIncome => Transactions.Where(t => t.Type == SimpleTransactionType.Income).Sum(t => t.Amount);

    public decimal TotalExpenses => Transactions.Where(t => t.Type == SimpleTransactionType.Expense).Sum(t => Math.Abs(t.Amount));

    public decimal NetIncome => TotalIncome - TotalExpenses;

    public int TotalTransactions => Transactions.Count;

    // Commands
    public ICommand AddTransactionCommand { get; }
    public ICommand EditTransactionCommand { get; }
    public ICommand DeleteTransactionCommand { get; }
    public ICommand ClearFiltersCommand { get; }
    public ICommand RefreshCommand { get; }

    // Methods
    private async void LoadTransactions()
    {
        try
        {
            IsLoading = true;
            
            var transactions = await _transactionService.GetTransactionsAsync();
            
            var transactionViewModels = transactions
                .Select(t => new TransactionViewModel
                {
                    Id = t.Id,
                    Description = t.Description,
                    Amount = t.Amount,
                    Date = t.Date,
                    Category = t.Category,
                    Type = t.Type
                })
                .ToList();

            Transactions = new ObservableCollection<TransactionViewModel>(transactionViewModels);
            UpdateFilteredTransactions();
        }
        catch (Exception ex)
        {
            await Application.Current?.MainPage?.DisplayAlert("Error", $"Error loading transactions: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void UpdateFilteredTransactions()
    {
        try
        {
            var filtered = Transactions.AsEnumerable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(t => 
                    t.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    t.Category.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            // Apply type filter
            if (SelectedTransactionType != "All")
            {
                if (Enum.TryParse<SimpleTransactionType>(SelectedTransactionType, out var typeFilter))
                {
                    filtered = filtered.Where(t => t.Type == typeFilter);
                }
            }

            FilteredTransactions = new ObservableCollection<TransactionViewModel>(
                filtered.OrderByDescending(t => t.Date).ToList());

            // Update summary properties
            OnPropertyChanged(nameof(TotalIncome));
            OnPropertyChanged(nameof(TotalExpenses));
            OnPropertyChanged(nameof(NetIncome));
            OnPropertyChanged(nameof(TotalTransactions));
            OnPropertyChanged(nameof(HasFilters));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating filtered transactions: {ex.Message}");
            FilteredTransactions = new ObservableCollection<TransactionViewModel>();
        }
    }

    private async void OnAddTransaction()
    {
        try
        {
            // Navigate to add transaction page
            await Shell.Current.GoToAsync("//transactions/form");
        }
        catch (Exception ex)
        {
            await Application.Current?.MainPage?.DisplayAlert("Error", $"Error navigating to add transaction: {ex.Message}", "OK");
        }
    }

    private async void OnEditTransaction(TransactionViewModel transaction)
    {
        try
        {
            if (transaction != null)
            {
                // Navigate to edit transaction page with transaction data
                await Shell.Current.GoToAsync($"//transactions/form?transactionId={transaction.Id}");
            }
        }
        catch (Exception ex)
        {
            await Application.Current?.MainPage?.DisplayAlert("Error", $"Error navigating to edit transaction: {ex.Message}", "OK");
        }
    }

    private async void OnDeleteTransaction(TransactionViewModel transaction)
    {
        try
        {
            if (transaction != null)
            {
                var result = await Application.Current?.MainPage?.DisplayAlert(
                    "Delete Transaction", 
                    $"Are you sure you want to delete '{transaction.Description}'?", 
                    "Delete", 
                    "Cancel");

                if (result == true)
                {
                    bool deleted = await _transactionService.DeleteTransactionAsync(transaction.Id);
                    if (deleted)
                    {
                        Transactions.Remove(transaction);
                        UpdateFilteredTransactions();
                        await Application.Current?.MainPage?.DisplayAlert("Success", "Transaction deleted successfully", "OK");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await Application.Current?.MainPage?.DisplayAlert("Error", $"Error deleting transaction: {ex.Message}", "OK");
        }
    }

    private void OnClearFilters()
    {
        SearchText = string.Empty;
        SelectedTransactionType = "All";
    }

    private async void OnRefresh()
    {
        LoadTransactions();
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class TransactionViewModel
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Category { get; set; } = string.Empty;
    public SimpleTransactionType Type { get; set; }
    
    public string FormattedAmount => Amount >= 0 ? $"+${Amount:F2}" : $"-${Math.Abs(Amount):F2}";
    public string FormattedDate => Date.ToString("MMM dd, yyyy");
    public Color AmountColor => Amount >= 0 ? Colors.Green : Colors.Red;
    public string TypeIcon => Type == SimpleTransactionType.Income ? "💰" : "💸";
}