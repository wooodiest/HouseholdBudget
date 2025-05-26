using HouseholdBudget.Core.Models;
using HouseholdBudget.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HouseholdBudget.Core.Services.Shared
{
    public class ExchangeRateService : IExchangeRateService
    {
        /// <summary>
        /// Defines the time-to-live duration for exchange rate cache entries.
        /// </summary>
        public readonly TimeSpan TTLCacheTimeSpan = TimeSpan.FromMinutes(10);

        /// <summary>
        /// Stores cached exchange rates along with the time they were retrieved, to reduce external calls.
        /// </summary>
        private readonly Dictionary<(string, string), (decimal rate, DateTime timestamp)> _cache = new();

        /// <summary>
        /// Reference to the provider responsible for retrieving exchange rate and currency data.
        /// </summary>
        private readonly IExchangeRateProvider _provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExchangeRateService"/> class.
        /// </summary>
        /// <param name="provider">The underlying exchange rate provider used to fetch rates.</param>
        public ExchangeRateService(IExchangeRateProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// Retrieves the exchange rate between two currencies, using a cached value if available and valid.
        /// </summary>
        /// <param name="fromCode">The ISO 4217 code of the source currency.</param>
        /// <param name="toCode">The ISO 4217 code of the target currency.</param>
        /// <returns>The exchange rate as a decimal value.</returns>
        private async Task<decimal> GetExchangeRateAsync(string fromCode, string toCode)
        {
            var key = (fromCode, toCode);
            var now = DateTime.UtcNow;

            if (_cache.TryGetValue(key, out var cached) && now - cached.timestamp < TTLCacheTimeSpan)
                return cached.rate;

            var rate = await _provider.GetExchangeRateAsync(fromCode, toCode);
            _cache[key] = (rate.Rate, now);

            return rate.Rate;
        }

        /// <inheritdoc />
        public async Task<decimal> ConvertAsync(decimal amount, Currency from, Currency to)
        {
            if (from == null || to == null)
                throw new ArgumentNullException();

            if (from == to)
                return amount;

            var rate = await GetExchangeRateAsync(from.Code, to.Code);
            return amount * rate;
        }

        /// <inheritdoc />
        public async Task<decimal> ConvertAsync(decimal amount, string fromCode, string toCode)
        {
            return await ConvertAsync(amount,
                await _provider.GetCurrencyByCodeAsync(fromCode)
                ?? throw new ArgumentException($"Unsupported currency code: {fromCode}", nameof(fromCode)),
                await _provider.GetCurrencyByCodeAsync(toCode)
                ?? throw new ArgumentException($"Unsupported currency code: {toCode}", nameof(toCode)));
        }
    }
}
