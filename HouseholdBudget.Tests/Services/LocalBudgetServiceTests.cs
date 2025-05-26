using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using HouseholdBudget.Core.Models;
using HouseholdBudget.Core.Services.Interfaces;
using HouseholdBudget.Core.Services.Local;
using HouseholdBudget.Core.UserData;
using Moq;
using Xunit;

namespace HouseholdBudget.Tests.Services;

public class LocalBudgetServiceTests
{
    private readonly Mock<ITransactionService> _transactionServiceMock = new();
    private readonly Mock<IUserSessionService> _userSessionMock = new();
    private readonly Mock<IExchangeRateService> _exchangeServiceMock = new();
    private readonly Mock<IExchangeRateProvider> _exchangeProviderMock = new();

    private readonly User _user = new()
    {
        Id = Guid.NewGuid(),
        Name = "Tester",
        Email = "test@example.com",
        PasswordHash = "hashed",
        DefaultCurrencyCode = "USD"
    };

    private readonly Currency _usd = Currency.Create("USD", "$", "US Dollar");
    private readonly Currency _eur = Currency.Create("EUR", "€", "Euro");

    private LocalBudgetService CreateService()
    {
        _userSessionMock.Setup(u => u.GetUser()).Returns(_user);
        _exchangeProviderMock.Setup(e => e.GetCurrencyByCodeAsync("USD")).ReturnsAsync(_usd);
        return new LocalBudgetService(
            _transactionServiceMock.Object,
            _userSessionMock.Object,
            _exchangeServiceMock.Object,
            _exchangeProviderMock.Object);
    }

    [Fact]
    public async Task GetTotalsAsync_ShouldAggregateCorrectly()
    {
        var service = CreateService();

        var txns = new[]
        {
            Transaction.Create(_user.Id, Guid.NewGuid(), 100, _eur, type: TransactionType.Income),
            Transaction.Create(_user.Id, Guid.NewGuid(), 50, _eur, type: TransactionType.Expense)
        };

        _transactionServiceMock.Setup(s => s.GetAsync(It.IsAny<TransactionFilter>())).ReturnsAsync(txns);
        _exchangeServiceMock.Setup(e => e.ConvertAsync(100, _eur, _usd)).ReturnsAsync(110);
        _exchangeServiceMock.Setup(e => e.ConvertAsync(50, _eur, _usd)).ReturnsAsync(55);

        var result = await service.GetTotalsAsync();

        result.TotalIncome.Should().Be(110);
        result.TotalExpenses.Should().Be(55);
        result.Currency.Code.Should().Be("USD");
    }

    [Fact]
    public async Task GetCategoryBreakdownAsync_ShouldGroupAndConvert()
    {
        var service = CreateService();
        var catId1 = Guid.NewGuid();
        var catId2 = Guid.NewGuid();

        var txns = new[]
        {
            Transaction.Create(_user.Id, catId1, 30, _eur),
            Transaction.Create(_user.Id, catId1, 20, _eur),
            Transaction.Create(_user.Id, catId2, 50, _eur)
        };

        _transactionServiceMock.Setup(s => s.GetAsync(It.IsAny<TransactionFilter>())).ReturnsAsync(txns);
        foreach (var t in txns)
            _exchangeServiceMock.Setup(e => e.ConvertAsync(t.Amount, _eur, _usd)).ReturnsAsync(t.Amount * 2);

        var result = await service.GetCategoryBreakdownAsync(DateTime.UtcNow, DateTime.UtcNow);

        result.Should().HaveCount(2);
        result.Should().Contain(b => b.CategoryId == catId1 && b.Amount == 100);
        result.Should().Contain(b => b.CategoryId == catId2 && b.Amount == 100);
    }

    [Fact]
    public async Task GetDailyTrendAsync_ShouldIncludeAllDatesAndAggregate()
    {
        var service = CreateService();
        var start = DateTime.UtcNow.Date;
        var end = start.AddDays(2);

        var txns = new[]
        {
            Transaction.Create(_user.Id, Guid.NewGuid(), 10, _eur, date: start, type: TransactionType.Income),
            Transaction.Create(_user.Id, Guid.NewGuid(), 5, _eur, date: start.AddDays(1), type: TransactionType.Expense)
        };

        _transactionServiceMock.Setup(s => s.GetAsync(It.IsAny<TransactionFilter>())).ReturnsAsync(txns);
        _exchangeServiceMock.Setup(e => e.ConvertAsync(10, _eur, _usd)).ReturnsAsync(20);
        _exchangeServiceMock.Setup(e => e.ConvertAsync(5, _eur, _usd)).ReturnsAsync(10);

        var result = await service.GetDailyTrendAsync(start, end);

        result.Should().HaveCount(3);
        result.First().TotalIncome.Should().Be(20);
        result[1].TotalExpenses.Should().Be(10);
        result[2].TotalIncome.Should().Be(0);
    }

    [Fact]
    public async Task GetMonthlySummaryAsync_ShouldReturnSummary()
    {
        var service = CreateService();
        var now = DateTime.UtcNow;
        var month = now.Month;
        var year = now.Year;

        _transactionServiceMock.Setup(s => s.GetAsync(It.IsAny<TransactionFilter>())).ReturnsAsync([]);
        _exchangeServiceMock.Setup(e => e.ConvertAsync(It.IsAny<decimal>(), It.IsAny<Currency>(), It.IsAny<Currency>()))
                            .ReturnsAsync(0);

        var summary = await service.GetMonthlySummaryAsync(year, month);

        summary.Year.Should().Be(year);
        summary.Month.Should().Be(month);
        summary.Currency.Should().Be(_usd);
        summary.TotalIncome.Should().Be(0);
        summary.TotalExpenses.Should().Be(0);
        summary.Categories.Should().BeEmpty();
        summary.DailyTrend.Should().NotBeNull();
    }

    [Fact]
    public async Task ShouldThrow_WhenUserIsNotAuthenticated()
    {
        _userSessionMock.Setup(u => u.GetUser()).Returns((User?)null);
        var service = new LocalBudgetService(
            _transactionServiceMock.Object,
            _userSessionMock.Object,
            _exchangeServiceMock.Object,
            _exchangeProviderMock.Object);

        Func<Task> act = () => service.GetTotalsAsync();

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*authenticated*");
    }

    [Fact]
    public async Task ShouldThrow_WhenCurrencyNotFound()
    {
        _userSessionMock.Setup(u => u.GetUser()).Returns(_user);
        _exchangeProviderMock.Setup(e => e.GetCurrencyByCodeAsync("USD")).ReturnsAsync((Currency?)null);

        var service = new LocalBudgetService(
            _transactionServiceMock.Object,
            _userSessionMock.Object,
            _exchangeServiceMock.Object,
            _exchangeProviderMock.Object);

        Func<Task> act = () => service.GetTotalsAsync();

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*currency*");
    }

    [Fact]
    public async Task GetDailyTrendAsync_ShouldReturnZeroValues_WhenNoTransactionsExistForDay()
    {
        var service = CreateService();
        var start = DateTime.UtcNow.Date;
        var end = start.AddDays(2);

        var txns = new[]
        {
            Transaction.Create(_user.Id, Guid.NewGuid(), 10, _eur, date: start.AddDays(1), type: TransactionType.Income)
        };

        _transactionServiceMock.Setup(s => s.GetAsync(It.IsAny<TransactionFilter>())).ReturnsAsync(txns);
        _exchangeServiceMock.Setup(e => e.ConvertAsync(10, _eur, _usd)).ReturnsAsync(20);

        var result = await service.GetDailyTrendAsync(start, end);

        result[0].TotalIncome.Should().Be(0);
        result[1].TotalIncome.Should().Be(20);
        result[2].TotalIncome.Should().Be(0);
    }

    [Fact]
    public async Task GetCategoryBreakdownAsync_ShouldReturnEmpty_WhenNoTransactions()
    {
        var service = CreateService();
        _transactionServiceMock.Setup(s => s.GetAsync(It.IsAny<TransactionFilter>())).ReturnsAsync([]);

        var result = await service.GetCategoryBreakdownAsync(DateTime.UtcNow, DateTime.UtcNow);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetDailyTrendAsync_ShouldConvertMultipleCurrencies()
    {
        var service = CreateService();
        var date = DateTime.UtcNow.Date;

        var txns = new[]
        {
            Transaction.Create(_user.Id, Guid.NewGuid(), 10, _eur, date: date, type: TransactionType.Income),
            Transaction.Create(_user.Id, Guid.NewGuid(), 20, _usd, date: date, type: TransactionType.Expense)
        };

        _transactionServiceMock.Setup(s => s.GetAsync(It.IsAny<TransactionFilter>())).ReturnsAsync(txns);
        _exchangeServiceMock.Setup(e => e.ConvertAsync(10, _eur, _usd)).ReturnsAsync(15);
        _exchangeServiceMock.Setup(e => e.ConvertAsync(20, _usd, _usd)).ReturnsAsync(20);

        var result = await service.GetDailyTrendAsync(date, date);

        result.Should().ContainSingle();
        result[0].TotalIncome.Should().Be(15);
        result[0].TotalExpenses.Should().Be(20);
    }

    [Fact]
    public async Task GetTotalsAsync_ShouldIgnoreUnknownTransactionTypes()
    {
        var service = CreateService();
        var txn = Transaction.Create(_user.Id, Guid.NewGuid(), 100, _eur);
        
        typeof(Transaction).GetProperty("Type")!.SetValue(txn, (TransactionType)999);

        _transactionServiceMock.Setup(s => s.GetAsync(It.IsAny<TransactionFilter>())).ReturnsAsync(new[] { txn });
        _exchangeServiceMock.Setup(e => e.ConvertAsync(100, _eur, _usd)).ReturnsAsync(100);

        var result = await service.GetTotalsAsync();

        result.TotalIncome.Should().Be(0);
        result.TotalExpenses.Should().Be(0);
    }

    [Fact]
    public async Task GetMonthlySummaryAsync_ShouldPreserveConsistencyAcrossComponents()
    {
        var service = CreateService();
        var now = DateTime.UtcNow;
        var start = new DateTime(now.Year, now.Month, 1);
        var end = start.AddMonths(1).AddDays(-1);

        _transactionServiceMock.Setup(s => s.GetAsync(It.IsAny<TransactionFilter>())).ReturnsAsync([]);
        _exchangeServiceMock.Setup(e => e.ConvertAsync(It.IsAny<decimal>(), It.IsAny<Currency>(), It.IsAny<Currency>()))
                            .ReturnsAsync(0);

        var summary = await service.GetMonthlySummaryAsync(start.Year, start.Month);

        summary.Year.Should().Be(start.Year);
        summary.Month.Should().Be(start.Month);
        summary.DailyTrend.All(p => p.Currency == _usd).Should().BeTrue();
        summary.Categories.All(c => c.Currency == _usd).Should().BeTrue();
    }


}
