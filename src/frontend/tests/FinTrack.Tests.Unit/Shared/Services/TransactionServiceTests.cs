using FinTrack.Core.Entities;
using FinTrack.Core.Enums;
using FinTrack.Core.Exceptions;
using FinTrack.Core.Interfaces;
using FinTrack.Shared.Services;
using FinTrack.Tests.Unit.Helpers;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinTrack.Tests.Unit.Shared.Services;

/// <summary>
/// Unit tests for TransactionService
/// </summary>
public class TransactionServiceTests : IDisposable
{
    private readonly Mock<ITransactionRepository> _mockTransactionRepository;
    private readonly Mock<IAccountRepository> _mockAccountRepository;
    private readonly Mock<ICategoryRepository> _mockCategoryRepository;
    private readonly Mock<IBudgetRepository> _mockBudgetRepository;
    private readonly Mock<ILogger<TransactionService>> _mockLogger;
    private readonly TransactionService _service;

    public TransactionServiceTests()
    {
        _mockTransactionRepository = new Mock<ITransactionRepository>();
        _mockAccountRepository = new Mock<IAccountRepository>();
        _mockCategoryRepository = new Mock<ICategoryRepository>();
        _mockBudgetRepository = new Mock<IBudgetRepository>();
        _mockLogger = new Mock<ILogger<TransactionService>>();
        
        _service = new TransactionService(
            _mockTransactionRepository.Object,
            _mockAccountRepository.Object,
            _mockCategoryRepository.Object,
            _mockBudgetRepository.Object,
            _mockLogger.Object);
    }

    #region AddTransactionAsync Tests

    [Fact]
    public async Task AddTransactionAsync_WithValidTransaction_ShouldAddAndUpdateBalance()
    {
        // Arrange
        var account = TestDataBuilder.CreateAccount("Test Account", 1000m);
        var category = TestDataBuilder.CreateCategory("Food", TransactionType.Expense);
        var transaction = TestDataBuilder.CreateTransaction(100m, "Groceries", TransactionType.Expense, category.Id, account.Id);

        _mockAccountRepository.Setup(x => x.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
        _mockCategoryRepository.Setup(x => x.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _mockTransactionRepository.Setup(x => x.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);

        // Act
        var result = await _service.AddTransactionAsync(transaction);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(transaction.Amount, result.Amount);
        _mockTransactionRepository.Verify(x => x.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockAccountRepository.Verify(x => x.UpdateBalanceAsync(account.Id, 900m, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddTransactionAsync_WithNullTransaction_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.AddTransactionAsync(null!));
    }

    [Fact]
    public async Task AddTransactionAsync_WithInvalidTransaction_ShouldThrowBusinessRuleException()
    {
        // Arrange
        var transaction = TestDataBuilder.CreateTransaction(0m, "", TransactionType.Expense, 1, 1);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.AddTransactionAsync(transaction));
        Assert.Equal("InvalidTransaction", exception.RuleName);
    }

    [Fact]
    public async Task AddTransactionAsync_WithNonExistentAccount_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var transaction = TestDataBuilder.CreateTransaction(100m, "Test", TransactionType.Expense, 1, 999);
        
        _mockAccountRepository.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account?)null);

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.AddTransactionAsync(transaction));
    }

    [Fact]
    public async Task AddTransactionAsync_WithInsufficientFunds_ShouldThrowBusinessRuleException()
    {
        // Arrange
        var account = TestDataBuilder.CreateAccount("Test Account", 50m);
        var category = TestDataBuilder.CreateCategory("Food", TransactionType.Expense);
        var transaction = TestDataBuilder.CreateTransaction(100m, "Expensive meal", TransactionType.Expense, category.Id, account.Id);

        _mockAccountRepository.Setup(x => x.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
        _mockCategoryRepository.Setup(x => x.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.AddTransactionAsync(transaction));
        Assert.Equal("InsufficientFunds", exception.RuleName);
    }

    [Fact]
    public async Task AddTransactionAsync_WithIncomeTransaction_ShouldIncreaseBalance()
    {
        // Arrange
        var account = TestDataBuilder.CreateAccount("Test Account", 1000m);
        var category = TestDataBuilder.CreateCategory("Salary", TransactionType.Income);
        var transaction = TestDataBuilder.CreateTransaction(500m, "Monthly salary", TransactionType.Income, category.Id, account.Id);

        _mockAccountRepository.Setup(x => x.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
        _mockCategoryRepository.Setup(x => x.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _mockTransactionRepository.Setup(x => x.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);

        // Act
        var result = await _service.AddTransactionAsync(transaction);

        // Assert
        Assert.NotNull(result);
        _mockAccountRepository.Verify(x => x.UpdateBalanceAsync(account.Id, 1500m, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region UpdateTransactionAsync Tests

    [Fact]
    public async Task UpdateTransactionAsync_WithValidTransaction_ShouldUpdateAndAdjustBalance()
    {
        // Arrange
        var account = TestDataBuilder.CreateAccount("Test Account", 900m); // Already reduced by 100
        var category = TestDataBuilder.CreateCategory("Food", TransactionType.Expense);
        var originalTransaction = TestDataBuilder.CreateTransaction(100m, "Original", TransactionType.Expense, category.Id, account.Id);
        var updatedTransaction = TestDataBuilder.CreateTransaction(150m, "Updated", TransactionType.Expense, category.Id, account.Id);
        updatedTransaction.Id = originalTransaction.Id;

        _mockTransactionRepository.Setup(x => x.GetByIdAsync(originalTransaction.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(originalTransaction);
        _mockAccountRepository.Setup(x => x.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
        _mockCategoryRepository.Setup(x => x.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);
        _mockTransactionRepository.Setup(x => x.UpdateAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedTransaction);

        // Act
        var result = await _service.UpdateTransactionAsync(updatedTransaction);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(150m, result.Amount);
        // Balance should be adjusted: 900 + 100 (reverse original) - 150 (new amount) = 850
        _mockAccountRepository.Verify(x => x.UpdateBalanceAsync(account.Id, 850m, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTransactionAsync_WithNonExistentTransaction_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var transaction = TestDataBuilder.CreateTransaction(100m, "Test", TransactionType.Expense, 1, 1);
        transaction.Id = 999;

        _mockTransactionRepository.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Transaction?)null);

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.UpdateTransactionAsync(transaction));
    }

    #endregion

    #region DeleteTransactionAsync Tests

    [Fact]
    public async Task DeleteTransactionAsync_WithValidId_ShouldDeleteAndReverseBalance()
    {
        // Arrange
        var account = TestDataBuilder.CreateAccount("Test Account", 900m); // Already reduced by 100
        var transaction = TestDataBuilder.CreateTransaction(100m, "To Delete", TransactionType.Expense, 1, account.Id);
        transaction.Id = 1;

        _mockTransactionRepository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transaction);
        _mockAccountRepository.Setup(x => x.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
        _mockTransactionRepository.Setup(x => x.DeleteAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteTransactionAsync(1);

        // Assert
        Assert.True(result);
        // Balance should be restored: 900 + 100 = 1000
        _mockAccountRepository.Verify(x => x.UpdateBalanceAsync(account.Id, 1000m, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteTransactionAsync_WithNonExistentId_ShouldReturnFalse()
    {
        // Arrange
        _mockTransactionRepository.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Transaction?)null);

        // Act
        var result = await _service.DeleteTransactionAsync(999);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Transfer Tests

    [Fact]
    public async Task CreateTransferAsync_WithValidAccounts_ShouldCreateTransferPair()
    {
        // Arrange
        var sourceAccount = TestDataBuilder.CreateAccount("Source", 1000m);
        var destinationAccount = TestDataBuilder.CreateAccount("Destination", 500m);
        sourceAccount.Id = 1;
        destinationAccount.Id = 2;

        _mockAccountRepository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sourceAccount);
        _mockAccountRepository.Setup(x => x.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(destinationAccount);
        _mockTransactionRepository.Setup(x => x.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Transaction t, CancellationToken ct) => t);

        // Act
        var result = await _service.CreateTransferAsync(1, 2, 200m, "Transfer test");

        // Assert
        Assert.NotNull(result.sourceTransaction);
        Assert.NotNull(result.destinationTransaction);
        Assert.Equal(200m, result.sourceTransaction.Amount);
        Assert.Equal(200m, result.destinationTransaction.Amount);
        Assert.Equal(TransactionType.Transfer, result.sourceTransaction.Type);
        Assert.Equal(TransactionType.Transfer, result.destinationTransaction.Type);
        
        // Verify balance updates
        _mockAccountRepository.Verify(x => x.UpdateBalanceAsync(1, 800m, It.IsAny<CancellationToken>()), Times.Once);
        _mockAccountRepository.Verify(x => x.UpdateBalanceAsync(2, 700m, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateTransferAsync_WithSameAccount_ShouldThrowBusinessRuleException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => 
            _service.CreateTransferAsync(1, 1, 100m, "Invalid transfer"));
        Assert.Equal("InvalidTransfer", exception.RuleName);
    }

    [Fact]
    public async Task CreateTransferAsync_WithInsufficientFunds_ShouldThrowBusinessRuleException()
    {
        // Arrange
        var sourceAccount = TestDataBuilder.CreateAccount("Source", 50m);
        sourceAccount.Id = 1;

        _mockAccountRepository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sourceAccount);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => 
            _service.CreateTransferAsync(1, 2, 100m, "Insufficient funds transfer"));
        Assert.Equal("InsufficientFunds", exception.RuleName);
    }

    #endregion

    #region Query Tests

    [Fact]
    public async Task GetTransactionsByAccountAsync_ShouldReturnAccountTransactions()
    {
        // Arrange
        var transactions = TestDataBuilder.CreateTransactionList(5, 1);
        
        _mockTransactionRepository.Setup(x => x.GetByAccountAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactions);

        // Act
        var result = await _service.GetTransactionsByAccountAsync(1);

        // Assert
        Assert.Equal(5, result.Count());
        _mockTransactionRepository.Verify(x => x.GetByAccountAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetTransactionsByCategoryAsync_ShouldReturnCategoryTransactions()
    {
        // Arrange
        var transactions = TestDataBuilder.CreateTransactionList(3, 1);
        
        _mockTransactionRepository.Setup(x => x.GetByCategoryAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactions);

        // Act
        var result = await _service.GetTransactionsByCategoryAsync(1);

        // Assert
        Assert.Equal(3, result.Count());
        _mockTransactionRepository.Verify(x => x.GetByCategoryAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetTransactionsByDateRangeAsync_ShouldReturnTransactionsInRange()
    {
        // Arrange
        var startDate = DateTime.Today.AddDays(-30);
        var endDate = DateTime.Today;
        var transactions = TestDataBuilder.CreateTransactionList(10, 1);
        
        _mockTransactionRepository.Setup(x => x.GetByDateRangeAsync(startDate, endDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactions);

        // Act
        var result = await _service.GetTransactionsByDateRangeAsync(startDate, endDate);

        // Assert
        Assert.Equal(10, result.Count());
        _mockTransactionRepository.Verify(x => x.GetByDateRangeAsync(startDate, endDate, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SearchTransactionsAsync_WithValidTerm_ShouldReturnMatchingTransactions()
    {
        // Arrange
        var transactions = TestDataBuilder.CreateTransactionList(2, 1);
        
        _mockTransactionRepository.Setup(x => x.SearchAsync("groceries", It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactions);

        // Act
        var result = await _service.SearchTransactionsAsync("groceries");

        // Assert
        Assert.Equal(2, result.Count());
        _mockTransactionRepository.Verify(x => x.SearchAsync("groceries", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task SearchTransactionsAsync_WithInvalidTerm_ShouldReturnEmpty(string searchTerm)
    {
        // Act
        var result = await _service.SearchTransactionsAsync(searchTerm);

        // Assert
        Assert.Empty(result);
        _mockTransactionRepository.Verify(x => x.SearchAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task ValidateTransactionAsync_WithValidTransaction_ShouldNotThrow()
    {
        // Arrange
        var account = TestDataBuilder.CreateAccount("Test Account", 1000m);
        var category = TestDataBuilder.CreateCategory("Food", TransactionType.Expense);
        var transaction = TestDataBuilder.CreateTransaction(100m, "Valid transaction", TransactionType.Expense, category.Id, account.Id);

        _mockAccountRepository.Setup(x => x.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
        _mockCategoryRepository.Setup(x => x.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act & Assert - Should not throw
        await _service.ValidateTransactionAsync(transaction);
    }

    [Fact]
    public async Task ValidateTransactionAsync_WithInactiveAccount_ShouldThrowBusinessRuleException()
    {
        // Arrange
        var account = TestDataBuilder.CreateAccount("Inactive Account", 1000m);
        account.IsActive = false;
        var transaction = TestDataBuilder.CreateTransaction(100m, "Test", TransactionType.Expense, 1, account.Id);

        _mockAccountRepository.Setup(x => x.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.ValidateTransactionAsync(transaction));
        Assert.Equal("InactiveEntityOperation", exception.RuleName);
    }

    [Fact]
    public async Task ValidateTransactionAsync_WithInactiveCategory_ShouldThrowBusinessRuleException()
    {
        // Arrange
        var account = TestDataBuilder.CreateAccount("Test Account", 1000m);
        var category = TestDataBuilder.CreateCategory("Inactive Category", TransactionType.Expense);
        category.IsActive = false;
        var transaction = TestDataBuilder.CreateTransaction(100m, "Test", TransactionType.Expense, category.Id, account.Id);

        _mockAccountRepository.Setup(x => x.GetByIdAsync(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
        _mockCategoryRepository.Setup(x => x.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.ValidateTransactionAsync(transaction));
        Assert.Equal("InactiveEntityOperation", exception.RuleName);
    }

    #endregion

    public void Dispose()
    {
        // Cleanup if needed
    }
}