using HouseholdBudget.Core.Models;

namespace HouseholdBudget.Core.Services
{
    /// <summary>
    /// Defines the contract for a service responsible for converting monetary amounts between different currencies.
    /// </summary>
    public interface IExchangeRateService
    {
        /// <summary>
        /// Asynchronously converts a specified amount of money from one currency to another using currency objects.
        /// </summary>
        /// <param name="amount">The monetary amount to convert.</param>
        /// <param name="from">The source currency.</param>
        /// <param name="to">The target currency.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains the converted monetary amount in the target currency.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either <paramref name="from"/> or <paramref name="to"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the exchange rate between the specified currencies is unavailable.
        /// </exception>
        Task<decimal> ConvertAsync(decimal amount, Currency from, Currency to);

        /// <summary>
        /// Asynchronously converts a specified amount of money from one currency to another using ISO 4217 currency codes.
        /// </summary>
        /// <param name="amount">The monetary amount to convert.</param>
        /// <param name="fromCode">The ISO 4217 code of the source currency (e.g., "USD").</param>
        /// <param name="toCode">The ISO 4217 code of the target currency (e.g., "PLN").</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains the converted monetary amount in the target currency.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when one or both currency codes are null, empty, or not supported.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the exchange rate between the specified currency codes is unavailable.
        /// </exception>
        Task<decimal> ConvertAsync(decimal amount, string fromCode, string toCode);
    }
}
