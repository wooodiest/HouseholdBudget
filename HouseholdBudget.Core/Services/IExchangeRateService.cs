using HouseholdBudget.Core.Models;

namespace HouseholdBudget.Core.Services
{
    public interface IExchangeRateService
    {
        Task<decimal> ConvertAsync(decimal amount, Currency from, Currency to);

        Task<decimal> ConvertAsync(decimal amount, string fromCode, string toCode);

        void ClearCache();
    }
}
