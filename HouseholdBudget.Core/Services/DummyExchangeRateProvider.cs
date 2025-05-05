using HouseholdBudget.Core.Models;

namespace HouseholdBudget.Core.Services
{
    public class DummyExchangeRateProvider : IExchangeRateProvider
    {
        private readonly Dictionary<(string, string), decimal> _rates = new() {
            { ("USD", "PLN"), 4.00m },
            { ("PLN", "USD"), 0.25m },

            { ("EUR", "PLN"), 4.50m },
            { ("PLN", "EUR"), 0.22m },

            { ("USD", "EUR"), 0.9m },
            { ("EUR", "USD"), 1.1m }
        };

        private readonly Dictionary<string, Currency> _currencies = new()
        {
            { "PLN", new Currency { Code = "PLN", Symbol = "zł", Name = "Polski Złoty" } },
            { "USD", new Currency { Code = "USD", Symbol = "$",  Name = "US Dollar"    } },
            { "EUR", new Currency { Code = "EUR", Symbol = "€",  Name = "Euro"         } }
        };

        public Task<ExchangeRate> GetExchangeRateAsync(string fromCurrencyCode, string toCurrencyCode)
        {
            var key = (fromCurrencyCode.ToUpperInvariant(), toCurrencyCode.ToUpperInvariant());

            if (!_rates.TryGetValue(key, out var rate))
                throw new Exception($"No exchange rate available from {fromCurrencyCode} to {toCurrencyCode}");

            return Task.FromResult(new ExchangeRate {
                FromCurrencyCode = key.Item1,
                ToCurrencyCode   = key.Item2,
                Rate             = rate,
                RetrievedAt      = DateTime.UtcNow
            });
        }

        public Task<Currency?> GetCurrencyByCodeAsync(string currencyCode)
        {
            currencyCode = currencyCode.ToUpperInvariant();

            if (_currencies.TryGetValue(currencyCode, out var currency))
                return Task.FromResult<Currency?>(currency);

            return Task.FromResult<Currency?>(null);
        }

        public Task<IReadOnlyCollection<Currency>> GetSupportedCurrenciesAsync()
            => Task.FromResult<IReadOnlyCollection<Currency>>(_currencies.Values.ToList());
    }
}
