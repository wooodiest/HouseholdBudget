using HouseholdBudget.Core.Models;

namespace HouseholdBudget.Core.Services
{
    /// <summary>
    /// Provides a local, in-memory implementation of <see cref="IExchangeRateProvider"/> 
    /// using predefined exchange rates and currency definitions.
    /// Intended for testing, prototyping, or offline scenarios.
    /// </summary>
    public class LocalExchangeRateProvider : IExchangeRateProvider
    {
        /// <summary>
        /// A dictionary containing static exchange rates between currency pairs.
        /// The key is a tuple of (source currency code, target currency code).
        /// </summary>
        private readonly Dictionary<(string, string), decimal> _rates = new();

        /// <summary>
        /// A dictionary containing supported currencies indexed by their ISO 4217 codes.
        /// </summary>
        private readonly Dictionary<string, Currency> _currencies = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalExchangeRateProvider"/> class
        /// with hardcoded currencies and exchange rates.
        /// </summary>
        public LocalExchangeRateProvider()
        {
            _currencies.Add("PLN", Currency.Create("PLN", "zł", "Polski Złoty"));
            _currencies.Add("USD", Currency.Create("USD", "$", "US Dollar"));
            _currencies.Add("EUR", Currency.Create("EUR", "€", "Euro"));

            _rates.Add(("USD", "PLN"), 4.0m); _rates.Add(("PLN", "USD"), 0.25m);
            _rates.Add(("EUR", "PLN"), 4.5m); _rates.Add(("PLN", "EUR"), 0.22m);
            _rates.Add(("USD", "EUR"), 0.9m); _rates.Add(("EUR", "USD"), 1.1m);
        }

        /// <inheritdoc />
        public Task<Currency?> GetCurrencyByCodeAsync(string currencyCode)
        {
            currencyCode = currencyCode.ToUpperInvariant();

            if (_currencies.TryGetValue(currencyCode, out var currency))
                return Task.FromResult<Currency?>(currency);

            return Task.FromResult<Currency?>(null);
        }

        /// <inheritdoc />
        public Task<ExchangeRate> GetExchangeRateAsync(string fromCurrencyCode, string toCurrencyCode)
        {
            fromCurrencyCode = fromCurrencyCode.ToUpperInvariant();
            toCurrencyCode = toCurrencyCode.ToUpperInvariant();

            if (!_currencies.ContainsKey(fromCurrencyCode) || !_currencies.ContainsKey(toCurrencyCode))
                throw new ArgumentException("One or both currency codes are not supported.");

            if (!_rates.TryGetValue((fromCurrencyCode, toCurrencyCode), out var rate))
                throw new InvalidOperationException($"Exchange rate from {fromCurrencyCode} to {toCurrencyCode} is not available.");

            return Task.FromResult(ExchangeRate.Create(fromCurrencyCode, toCurrencyCode, rate));
        }

        /// <inheritdoc />
        public Task<IReadOnlyCollection<Currency>> GetSupportedCurrenciesAsync()
        {
            var result = _currencies.Values.ToList().AsReadOnly();
            return Task.FromResult<IReadOnlyCollection<Currency>>(result);
        }
    }
}
