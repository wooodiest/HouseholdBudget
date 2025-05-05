using HouseholdBudget.Core.Models;

namespace HouseholdBudget.Core.Services
{
    public class ExchangeRateService : IExchangeRateService
    {
        private readonly IExchangeRateProvider _provider;

        private readonly Dictionary<(string, string), (decimal rate, DateTime timestamp)> _cache = new();

        private readonly TimeSpan _cacheTTL = TimeSpan.FromHours(1);

        public ExchangeRateService(IExchangeRateProvider provider)
        {
            _provider = provider;
        }

        public void ClearCache() => _cache.Clear();

        public async Task<decimal> ConvertAsync(decimal amount, Currency fromCurrency, Currency toCurrency)
        {
            if (fromCurrency.Code == toCurrency.Code)
                return amount;

            var key = (fromCurrency.Code.ToUpper(), toCurrency.Code.ToUpper());
            var now = DateTime.UtcNow;

            if (_cache.TryGetValue(key, out var cached) && now - cached.timestamp < _cacheTTL)
            {
                return amount * cached.rate;
            }

            var rateObj = await _provider.GetExchangeRateAsync(fromCurrency.Code, toCurrency.Code);
            _cache[key] = (rateObj.Rate, rateObj.RetrievedAt);

            return amount * rateObj.Rate;
        }

        public Task<decimal> ConvertAsync(decimal amount, string fromCode, string toCode)
        {
            return ConvertAsync(amount, new Currency { Code = fromCode }, new Currency { Code = toCode });
        }
    }
}
