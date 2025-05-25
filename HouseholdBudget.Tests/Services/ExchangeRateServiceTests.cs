using System;
using System.Threading.Tasks;
using FluentAssertions;
using HouseholdBudget.Core.Models;
using HouseholdBudget.Core.Services;
using Moq;
using Xunit;

namespace HouseholdBudget.Tests.Services
{
    public class ExchangeRateServiceTests
    {
        private readonly Mock<IExchangeRateProvider> _providerMock = new();
        private readonly ExchangeRateService _service;

        public ExchangeRateServiceTests()
        {
            _service = new ExchangeRateService(_providerMock.Object);
        }

        [Fact]
        public async Task ConvertAsync_WithSameCurrency_ShouldReturnOriginalAmount()
        {
            // Arrange
            var currency = Currency.Create("USD", "$", "US Dollar");

            // Act
            var result = await _service.ConvertAsync(100m, currency, currency);

            // Assert
            result.Should().Be(100m);
        }

        [Fact]
        public async Task ConvertAsync_WithDifferentCurrencies_ShouldUseExchangeRate()
        {
            // Arrange
            var from = Currency.Create("USD", "$", "US Dollar");
            var to = Currency.Create("PLN", "zł", "Polski Złoty");

            _providerMock.Setup(p => p.GetExchangeRateAsync("USD", "PLN"))
                         .ReturnsAsync(ExchangeRate.Create("USD", "PLN", 4.0m));

            // Act
            var result = await _service.ConvertAsync(100m, from, to);

            // Assert
            result.Should().Be(400m);
        }

        [Fact]
        public async Task ConvertAsync_StringOverload_ShouldUseCurrencyLookup()
        {
            // Arrange
            var from = Currency.Create("EUR", "€", "Euro");
            var to = Currency.Create("USD", "$", "US Dollar");

            _providerMock.Setup(p => p.GetCurrencyByCodeAsync("EUR")).ReturnsAsync(from);
            _providerMock.Setup(p => p.GetCurrencyByCodeAsync("USD")).ReturnsAsync(to);
            _providerMock.Setup(p => p.GetExchangeRateAsync("EUR", "USD"))
                         .ReturnsAsync(ExchangeRate.Create("EUR", "USD", 1.1m));

            // Act
            var result = await _service.ConvertAsync(50m, "EUR", "USD");

            // Assert
            result.Should().Be(55m);
        }

        [Fact]
        public async Task ConvertAsync_WithUnsupportedCurrency_ShouldThrowArgumentException()
        {
            // Arrange
            _providerMock.Setup(p => p.GetCurrencyByCodeAsync("XYZ"))
                         .ReturnsAsync((Currency?)null);

            // Act
            var act = async () => await _service.ConvertAsync(100m, "XYZ", "USD");

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                     .WithMessage("Unsupported currency code: XYZ*");
        }

        [Fact]
        public async Task ConvertAsync_WithNullCurrency_ShouldThrowArgumentNullException()
        {
            // Act
            var act = async () => await _service.ConvertAsync(100m, (Currency)null!, (Currency)null!);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task ConvertAsync_ShouldCacheExchangeRateForGivenPeriod()
        {
            // Arrange
            var from = Currency.Create("USD", "$", "US Dollar");
            var to = Currency.Create("EUR", "€", "Euro");

            _providerMock.Setup(p => p.GetExchangeRateAsync("USD", "EUR"))
                         .ReturnsAsync(ExchangeRate.Create("USD", "EUR", 0.9m));

            // First call should trigger provider
            var first = await _service.ConvertAsync(100m, from, to);

            // Second call should hit cache and not call provider again
            var second = await _service.ConvertAsync(200m, from, to);

            // Assert
            first.Should().Be(90m);
            second.Should().Be(180m);
            _providerMock.Verify(p => p.GetExchangeRateAsync("USD", "EUR"), Times.Once);
        }
    }
}
