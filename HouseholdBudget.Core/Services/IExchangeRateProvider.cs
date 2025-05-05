using HouseholdBudget.Core.Models;

namespace HouseholdBudget.Core.Services
{
    public interface IExchangeRateProvider
    {
        Task<ExchangeRate> GetExchangeRateAsync(string fromCurrencyCode, string toCurrencyCode);

        Task<Currency?> GetCurrencyByCodeAsync(string currencyCode);

        Task<IReadOnlyCollection<Currency>> GetSupportedCurrenciesAsync();
    }
}
