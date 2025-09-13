using FinTrack.Core.Entities;
using FinTrack.Core.Enums;
using FinTrack.Core.Exceptions;
using FinTrack.Core.Interfaces;
using FinTrack.Shared.Models;
using FinTrack.Shared.Services;
using FinTrack.Tests.Unit.Helpers;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinTrack.Tests.Unit.Shared.Services;

/// <summary>
/// Unit tests for AccountService
/// </summary>
public class AccountServiceTests : IDisposable
{
    private readonly Mock<IAccountRepository> _mockAccountRepository;
    private readonly Mock<ITransactionRepository> _mockTransactionRepository;
    private readonly Mock<IGoalRepository> _mockGoalRepository;
    private readonly Mock<ILogger<AccountService>> _mockLogger;
    private readonly AccountService _service;

    public AccountServiceTests()
    {
        _mockAccountRepository = new Mock<IAccountRepository>();
        _mockTransactionRepository = new Mock<ITransactionRepository>();
        _mockGoalRepository = new Mock<IGoalRepository>();
        _mockLogger = new Mock<ILogger<AccountService>>();
        
        _service = new AccountService(
            _mockAccountRepository.Object,
            _mockTransactionRepository.Object,
            _mockGoalRepository.Object,
            _mockLogger.Object);
    }

    #region CreateAccountAsync Tests

    [Fact]
    public async Task CreateAccountAsync_WithValidAccount_ShouldCreateAccount()
    {
        // Arrange
        var account = TestDataBuilder.CreateAccount("New Account", 1000m);
        var createdAccount = TestDataBuilder.CreateAccount("New Account", 1000m);
        createdAccount.Id = 1;
        createdAccount.InitialBalance = 1000m;
        
        _mockAccountRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Account>());
        _mockAccountRepository.Setup(x => x.AddAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdAccount);
        _mockAccountRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.CreateAccountAsync(account);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(account.Name, result.Name);
        Assert.Equal(account.Balance, result.Balance);
    }

    [Fact]
    public async Task CreateAccountAsync_WithNullAccount_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.CreateAccountAsync(null!));
    }

    [Fact]
    public async Task CreateAccountAsync_WithInvalidAccount_ShouldThrowBusinessRuleException()
    {
        // Arrange
        var account = TestDataBuilder.CreateAccount("", 1000m); // Invalid name

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.CreateAccountAsync(account));
        Assert.Equal("InvalidAccount", exception.RuleName);
    }

    [Fact]
    public async Task CreateAccountAsync_WithDuplicateName_ShouldThrowBusinessRuleException()
    {
        // Arrange
        var account = TestDataBuilder.CreateAccount("Existing Account", 1000m);
        var existingAccounts = new[] { TestDataBuilder.CreateAccount("Existing Account", 500m) };
        
        _mockAccountRepository.Setup(x => x.SearchAsync("Existing Account", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAccounts);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.CreateAccountAsync(account));
        Assert.Equal("DuplicateEntity", exception.RuleName);
    }

    #endregion

    #region UpdateAccountAsync Tests

    [Fact]
    public async Task UpdateAccountAsync_WithValidAccount_ShouldUpdateAccount()
    {
        // Arrange
        var account = TestDataBuilder.CreateAccount("Updated Account", 1500m);
        account.Id = 1;
        
        _mockAccountRepository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
        _mockAccountRepository.Setup(x => x.UpdateAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        // Act
        var result = await _service.UpdateAccountAsync(account);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(account.Name, result.Name);
        _mockAccountRepository.Verify(x => x.UpdateAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAccountAsync_WithNonExistentAccount_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var account = TestDataBuilder.CreateAccount("Non-existent", 1000m);
        account.Id = 999;
        
        _mockAccountRepository.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account?)null);

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.UpdateAccountAsync(account));
    }

    #endregion

    #region DeleteAccountAsync Tests

    [Fact]
    public async Task DeleteAccountAsync_WithValidId_ShouldDeleteAccount()
    {
        // Arrange
        var account = TestDataBuilder.CreateAccount("To Delete", 0m);
        account.Id = 1;
        
        _mockAccountRepository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
        _mockTransactionRepository.Setup(x => x.CountAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Transaction, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        _mockGoalRepository.Setup(x => x.GetByLinkedAccountAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Goal>());
        _mockAccountRepository.Setup(x => x.DeleteAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteAccountAsync(1);

        // Assert
        Assert.True(result);
        _mockAccountRepository.Verify(x => x.DeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAccountAsync_WithTransactions_ShouldThrowBusinessRuleException()
    {
        // Arrange
        var account = TestDataBuilder.CreateAccount("With Transactions", 1000m);
        account.Id = 1;
        
        _mockAccountRepository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
        _mockTransactionRepository.Setup(x => x.CountAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Transaction, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(5); // Has transactions

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.DeleteAccountAsync(1));
        Assert.Contains("cannot be deleted", exception.Message.ToLower());
    }

    [Fact]
    public async Task DeleteAccountAsync_WithNonZeroBalance_ShouldThrowBusinessRuleException()
    {
        // Arrange
        var account = TestDataBuilder.CreateAccount("With Balance", 1000m);
        account.Id = 1;
        
        _mockAccountRepository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.DeleteAccountAsync(1));
        Assert.Contains("balance must be zero", exception.Message.ToLower());
    }

    [Fact]
    public async Task DeleteAccountAsync_WithLinkedGoals_ShouldThrowBusinessRuleException()
    {
        // Arrange
        var account = TestDataBuilder.CreateAccount("With Goals", 0m);
        account.Id = 1;
        var goals = new[] { TestDataBuilder.CreateGoal("Test Goal", 1000m, GoalType.Savings, 1) };
        
        _mockAccountRepository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
        _mockTransactionRepository.Setup(x => x.CountAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Transaction, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        _mockGoalRepository.Setup(x => x.GetByLinkedAccountAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(goals);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.DeleteAccountAsync(1));
        Assert.Contains("linked goals", exception.Message.ToLower());
    }

    #endregion

    #region Balance Management Tests

    [Fact]
    public async Task RecalculateBalanceAsync_WithValidAccount_ShouldUpdateBalance()
    {
        // Arrange
        var account = TestDataBuilder.CreateAccount("Test Account", 1000m);
        account.Id = 1;
        var calculatedBalance = 1250m;
        
        _mockAccountRepository.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);
        _mockAccountRepository.Setup(x => x.CalculateBalanceAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(calculatedBalance);
        _mockAccountRepository.Setup(x => x.UpdateBalanceAsync(1, calculatedBalance, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.RecalculateBalanceAsync(1);

        // Assert
        Assert.Equal(calculatedBalance, result);
        _mockAccountRepository.Verify(x => x.UpdateBalanceAsync(1, calculatedBalance, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RecalculateBalanceAsync_WithNonExistentAccount_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        _mockAccountRepository.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account?)null);

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.RecalculateBalanceAsync(999));
    }

    [Fact]
    public async Task GetAccountSummaryAsync_WithValidAccount_ShouldReturnSummary()
    {
        // Arrange
        var accountSummary = new AccountSummary
        {
            AccountId = 1,
            AccountName = "Test Account",
            CurrentBalance = 1000m,
            TransactionCount = 10,
            MonthToDateIncome = 2000m,
            MonthToDateExpenses = 800m
        };
        
        _mockAccountRepository.Setup(x => x.GetAccountSummaryAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(accountSummary);

        // Act
        var result = await _service.GetAccountSummaryAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.AccountId);
        Assert.Equal("Test Account", result.AccountName);
        Assert.Equal(1000m, result.CurrentBalance);
        Assert.Equal(10, result.TransactionCount);
    }

    [Fact]
    public async Task GetAccountSummaryAsync_WithNonExistentAccount_ShouldReturnNull()
    {
        // Arrange
        _mockAccountRepository.Setup(x => x.GetAccountSummaryAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AccountSummary?)null);

        // Act
        var result = await _service.GetAccountSummaryAsync(999);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region Query Tests

    [Fact]
    public async Task GetActiveAccountsAsync_ShouldReturnActiveAccounts()
    {
        // Arrange
        var accounts = TestDataBuilder.CreateAccountList(3);
        
        _mockAccountRepository.Setup(x => x.GetActiveAccountsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(accounts);

        // Act
        var result = await _service.GetActiveAccountsAsync();

        // Assert
        Assert.Equal(3, result.Count());
        _mockAccountRepository.Verify(x => x.GetActiveAccountsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAccountsByTypeAsync_ShouldReturnAccountsOfType()
    {
        // Arrange
        var accounts = new[] { TestDataBuilder.CreateAccount("Savings 1", 1000m, AccountType.Savings) };
        
        _mockAccountRepository.Setup(x => x.GetByTypeAsync(AccountType.Savings, It.IsAny<CancellationToken>()))
            .ReturnsAsync(accounts);

        // Act
        var result = await _service.GetAccountsByTypeAsync(AccountType.Savings);

        // Assert
        Assert.Single(result);
        Assert.Equal(AccountType.Savings, result.First().Type);
    }

    [Fact]
    public async Task GetOverdrawnAccountsAsync_ShouldReturnOverdrawnAccounts()
    {
        // Arrange
        var overdrawnAccounts = new[] 
        { 
            TestDataBuilder.CreateAccount("Overdrawn", -100m, AccountType.Checking) 
        };
        
        _mockAccountRepository.Setup(x => x.GetOverdrawnAccountsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(overdrawnAccounts);

        // Act
        var result = await _service.GetOverdrawnAccountsAsync();

        // Assert
        Assert.Single(result);
        Assert.True(result.First().Balance < 0);
    }

    [Fact]
    public async Task GetNetWorthAsync_ShouldReturnTotalNetWorth()
    {
        // Arrange
        var netWorth = 15000m;
        
        _mockAccountRepository.Setup(x => x.GetTotalNetWorthAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(netWorth);

        // Act
        var result = await _service.GetNetWorthAsync();

        // Assert
        Assert.Equal(netWorth, result);
    }

    [Fact]
    public async Task GetTotalAssetsAsync_ShouldReturnTotalAssets()
    {
        // Arrange
        var totalAssets = 25000m;
        
        _mockAccountRepository.Setup(x => x.GetTotalAssetsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(totalAssets);

        // Act
        var result = await _service.GetTotalAssetsAsync();

        // Assert
        Assert.Equal(totalAssets, result);
    }

    [Fact]
    public async Task GetTotalLiabilitiesAsync_ShouldReturnTotalLiabilities()
    {
        // Arrange
        var totalLiabilities = 5000m;
        
        _mockAccountRepository.Setup(x => x.GetTotalLiabilitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(totalLiabilities);

        // Act
        var result = await _service.GetTotalLiabilitiesAsync();

        // Assert
        Assert.Equal(totalLiabilities, result);
    }

    #endregion

    #region Balance History Tests

    [Fact]
    public async Task GetBalanceHistoryAsync_WithValidParameters_ShouldReturnHistory()
    {
        // Arrange
        var startDate = DateTime.Today.AddDays(-30);
        var endDate = DateTime.Today;
        var balanceHistory = new Dictionary<DateTime, decimal>
        {
            { startDate, 1000m },
            { startDate.AddDays(15), 1200m },
            { endDate, 1500m }
        };
        
        _mockAccountRepository.Setup(x => x.GetBalanceHistoryAsync(1, startDate, endDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(balanceHistory);

        // Act
        var result = await _service.GetBalanceHistoryAsync(1, startDate, endDate);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(1000m, result[startDate]);
        Assert.Equal(1500m, result[endDate]);
    }

    [Fact]
    public async Task GetBalanceHistoryAsync_WithInvalidDateRange_ShouldThrowBusinessRuleException()
    {
        // Arrange
        var startDate = DateTime.Today;
        var endDate = DateTime.Today.AddDays(-30); // End before start

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => 
            _service.GetBalanceHistoryAsync(1, startDate, endDate));
        Assert.Equal("InvalidDateRange", exception.RuleName);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task ValidateAccountAsync_WithValidAccount_ShouldNotThrow()
    {
        // Arrange
        var account = TestDataBuilder.CreateAccount("Valid Account", 1000m);

        // Act & Assert - Should not throw
        await _service.ValidateAccountAsync(account);
    }

    [Fact]
    public async Task ValidateAccountAsync_WithInvalidAccount_ShouldThrowBusinessRuleException()
    {
        // Arrange
        var account = TestDataBuilder.CreateAccount("", 1000m); // Invalid name

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => _service.ValidateAccountAsync(account));
        Assert.Equal("InvalidAccount", exception.RuleName);
    }

    [Fact]
    public async Task ValidateAccountDeletionAsync_WithValidAccount_ShouldNotThrow()
    {
        // Arrange
        var account = TestDataBuilder.CreateAccount("Valid", 0m);
        account.Id = 1;
        
        _mockTransactionRepository.Setup(x => x.CountAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Transaction, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        _mockGoalRepository.Setup(x => x.GetByLinkedAccountAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Goal>());

        // Act & Assert - Should not throw
        await _service.ValidateAccountDeletionAsync(account);
    }

    #endregion

    public void Dispose()
    {
        // Cleanup if needed
    }
}