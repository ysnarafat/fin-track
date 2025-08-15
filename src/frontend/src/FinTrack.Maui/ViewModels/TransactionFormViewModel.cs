using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using FinTrack.Maui.Models;
using FinTrack.Maui.Services;

namespace FinTrack.Maui.ViewModels;

public class TransactionFormViewModel : INotifyPropertyChanged, IQueryAttributable
{
    private readonly ITransactionService _transactionService;
    private string _selectedType = "Expense";
    private string _amount = string.Empty;
    private string _description = string.Empty;
    private DateTime _date = DateTime.Today;
    private string _selectedCategory = string.Empty;
    private bool _isSaving = false;
    private bool _isEditMode = false;
    private int? _editingTransactionId;

    public TransactionFormViewModel(ITransactionService transactionService)
    {
        _transactionService = transactionService;
        LoadCategories();
        
        SelectTypeCommand = new Command<string>(OnSelectType);
        SaveCommand = new Command(OnSave, CanExecuteSave);
        CancelCommand = new Command(OnCancel);
        
        // Set default category
        SelectedCategory = Categories.FirstOrDefault() ?? string.Empty;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    // Properties
    public string SelectedType
    {
        get => _selectedType;
        set
        {
            if (_selectedType != value)
            {
                _selectedType = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IncomeButtonColor));
                OnPropertyChanged(nameof(ExpenseButtonColor));
                ((Command)SaveCommand).ChangeCanExecute();
            }
        }
    }

    public string Amount
    {
        get => _amount;
        set
        {
            if (_amount != value)
            {
                _amount = value;
                OnPropertyChanged();
                ((Command)SaveCommand).ChangeCanExecute();
            }
        }
    }

    public string Description
    {
        get => _description;
        set
        {
            if (_description != value)
            {
                _description = value;
                OnPropertyChanged();
                ((Command)SaveCommand).ChangeCanExecute();
            }
        }
    }

    public DateTime Date
    {
        get => _date;
        set
        {
            if (_date != value)
            {
                _date = value;
                OnPropertyChanged();
            }
        }
    }

    public string SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            if (_selectedCategory != value)
            {
                _selectedCategory = value;
                OnPropertyChanged();
                ((Command)SaveCommand).ChangeCanExecute();
            }
        }
    }

    public bool IsSaving
    {
        get => _isSaving;
        set
        {
            if (_isSaving != value)
            {
                _isSaving = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsEditMode
    {
        get => _isEditMode;
        set
        {
            if (_isEditMode != value)
            {
                _isEditMode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PageTitle));
                OnPropertyChanged(nameof(SaveButtonText));
            }
        }
    }

    public List<string> Categories { get; } = new();

    public string PageTitle => IsEditMode ? "Edit Transaction" : "Add Transaction";
    public string SaveButtonText => IsEditMode ? "Update" : "Save";

    public bool CanSave => !string.IsNullOrWhiteSpace(Description) &&
                          !string.IsNullOrWhiteSpace(Amount) &&
                          decimal.TryParse(Amount, out var amt) && amt > 0 &&
                          !string.IsNullOrWhiteSpace(SelectedCategory);

    public string IncomeButtonColor => SelectedType == "Income" ? "#4CAF50" : "#E0E0E0";
    public string ExpenseButtonColor => SelectedType == "Expense" ? "#F44336" : "#E0E0E0";

    // Commands
    public ICommand SelectTypeCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    // Methods
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("transactionId") && int.TryParse(query["transactionId"].ToString(), out var transactionId))
        {
            _editingTransactionId = transactionId;
            LoadTransaction(transactionId);
        }
    }

    private async void LoadTransaction(int transactionId)
    {
        try
        {
            var transaction = await _transactionService.GetTransactionByIdAsync(transactionId);
            if (transaction != null)
            {
                IsEditMode = true;
                SelectedType = transaction.Type.ToString();
                Amount = Math.Abs(transaction.Amount).ToString("F2");
                Description = transaction.Description;
                Date = transaction.Date;
                SelectedCategory = transaction.Category;
            }
        }
        catch (Exception ex)
        {
            await Application.Current?.MainPage?.DisplayAlert("Error", $"Error loading transaction: {ex.Message}", "OK");
        }
    }

    private void LoadCategories()
    {
        Categories.AddRange(new[]
        {
            "Food & Dining",
            "Transportation",
            "Shopping",
            "Entertainment",
            "Bills & Utilities",
            "Healthcare",
            "Education",
            "Travel",
            "Income",
            "Investment",
            "Other"
        });
    }

    private void OnSelectType(string type)
    {
        SelectedType = type;
    }

    private async void OnSave()
    {
        if (!CanSave)
            return;

        try
        {
            IsSaving = true;

            if (!decimal.TryParse(Amount, out var amount))
            {
                await Application.Current?.MainPage?.DisplayAlert("Error", "Please enter a valid amount", "OK");
                return;
            }

            if (!Enum.TryParse<SimpleTransactionType>(SelectedType, out var transactionType))
            {
                await Application.Current?.MainPage?.DisplayAlert("Error", "Please select a transaction type", "OK");
                return;
            }

            // Adjust amount for expenses (make negative)
            if (transactionType == SimpleTransactionType.Expense)
            {
                amount = -Math.Abs(amount);
            }
            else
            {
                amount = Math.Abs(amount);
            }

            var transaction = new SimpleTransaction
            {
                Amount = amount,
                Description = Description,
                Date = Date,
                Type = transactionType,
                Category = SelectedCategory
            };

            if (IsEditMode && _editingTransactionId.HasValue)
            {
                transaction.Id = _editingTransactionId.Value;
                await _transactionService.UpdateTransactionAsync(transaction);
            }
            else
            {
                await _transactionService.CreateTransactionAsync(transaction);
            }

            await Application.Current?.MainPage?.DisplayAlert(
                "Success", 
                $"Transaction {(IsEditMode ? "updated" : "saved")} successfully!", 
                "OK");

            // Navigate back
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Application.Current?.MainPage?.DisplayAlert("Error", $"Error saving transaction: {ex.Message}", "OK");
        }
        finally
        {
            IsSaving = false;
        }
    }

    private bool CanExecuteSave()
    {
        return CanSave && !IsSaving;
    }

    private async void OnCancel()
    {
        await Shell.Current.GoToAsync("..");
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}