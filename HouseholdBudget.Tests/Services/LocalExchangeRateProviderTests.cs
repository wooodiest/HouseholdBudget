using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using HouseholdBudget.Core.Models;
using HouseholdBudget.Core.Services.Local;
using Xunit;

namespace HouseholdBudget.Tests.Services
{
    public class LocalExchangeRateProviderTests
    {
        private readonly LocalExchangeRateProvider _provider = new();

        [Theory]
        [InlineData("PLN", "zł", "Polski Złoty")]
        [InlineData("USD", "$", "US Dollar")]
        [InlineData("EUR", "€", "Euro")]
        public async Task GetCurrencyByCodeAsync_ShouldReturnCorrectCurrency(string code, string symbol, string name)
        {
            // Act
            var result = await _provider.GetCurrencyByCodeAsync(code);

            // Assert
            result.Should().NotBeNull();
            result!.Code.Should().Be(code);
            result.Symbol.Should().Be(symbol);
            result.Name.Should().Be(name);
        }

        [Fact]
        public async Task GetCurrencyByCodeAsync_WithUnknownCode_ShouldReturnNull()
        {
            // Act
            var result = await _provider.GetCurrencyByCodeAsync("XYZ");

            // Assert
            result.Should().BeNull();
        }

        [Theory]
        [InlineData("USD", "PLN", 4.0)]
        [InlineData("PLN", "USD", 0.25)]
        [InlineData("EUR", "PLN", 4.5)]
        [InlineData("PLN", "EUR", 0.22)]
        [InlineData("USD", "EUR", 0.9)]
        [InlineData("EUR", "USD", 1.1)]
        public async Task GetExchangeRateAsync_ShouldReturnCorrectRate(string from, string to, decimal expectedRate)
        {
            // Act
            var rate = await _provider.GetExchangeRateAsync(from, to);

            // Assert
            rate.Should().NotBeNull();
            rate.FromCurrencyCode.Should().Be(from);
            rate.ToCurrencyCode.Should().Be(to);
            rate.Rate.Should().Be(expectedRate);
        }

        [Fact]
        public async Task GetExchangeRateAsync_WithUnknownCurrency_ShouldThrowArgumentException()
        {
            // Act
            Func<Task> act = async () => await _provider.GetExchangeRateAsync("USD", "XYZ");

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("One or both currency codes are not supported.");
        }

        [Fact]
        public async Task GetExchangeRateAsync_WithMissingRate_ShouldThrowInvalidOperationException()
        {
            // Arrange: Add currency without defining rate between them
            var provider = new LocalExchangeRateProvider();
            var privateCurrencies = typeof(LocalExchangeRateProvider)
                .GetField("_currencies", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
            var dict = (Dictionary<string, Currency>)privateCurrencies.GetValue(provider)!;
            dict["GBP"] = Currency.Create("GBP", "£", "British Pound");

            // Act
            Func<Task> act = async () => await provider.GetExchangeRateAsync("GBP", "USD");

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Exchange rate from GBP to USD is not available.");
        }

        [Fact]
        public async Task GetSupportedCurrenciesAsync_ShouldReturnAllDefinedCurrencies()
        {
            // Act
            var currencies = await _provider.GetSupportedCurrenciesAsync();

            // Assert
            currencies.Should().HaveCount(3);
            currencies.Select(c => c.Code).Should().BeEquivalentTo("PLN", "USD", "EUR");
        }
    }
}
