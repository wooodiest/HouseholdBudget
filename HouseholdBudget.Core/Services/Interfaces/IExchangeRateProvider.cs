using HouseholdBudget.Core.Models;

namespace HouseholdBudget.Core.Services.Interfaces
{
    /// <summary>
    /// Defines the contract for a service that provides exchange rates and metadata for supported currencies.
    /// </summary>
    public interface IExchangeRateProvider
    {
        /// <summary>
        /// Asynchronously retrieves the exchange rate between two specified currencies.
        /// </summary>
        /// <param name="fromCurrencyCode">The ISO 4217 code of the source currency (e.g., "USD").</param>
        /// <param name="toCurrencyCode">The ISO 4217 code of the target currency (e.g., "PLN").</param>
        /// <returns>
        /// A task representing the asynchronous operation. The task result contains an <see cref="ExchangeRate"/>
        /// representing the conversion rate between the given currencies.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when one or both of the provided currency codes are invalid or not supported by the provider.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when an exchange rate for the given currency pair is unavailable or cannot be retrieved.
        /// </exception>
        Task<ExchangeRate> GetExchangeRateAsync(string fromCurrencyCode, string toCurrencyCode);

        /// <summary>
        /// Asynchronously retrieves a currency by its ISO 4217 code.
        /// </summary>
        /// <param name="currencyCode">The currency code to look up (e.g., "EUR").</param>
        /// <returns>
        /// A task representing the asynchronous operation. The task result contains a <see cref="Currency"/>
        /// if found; otherwise, <c>null</c>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when the currency code is invalid or empty.
        /// </exception>
        Task<Currency?> GetCurrencyByCodeAsync(string currencyCode);

        /// <summary>
        /// Asynchronously retrieves all currencies supported by the exchange rate provider.
        /// </summary>
        /// <returns>
        /// A task representing the asynchronous operation. The task result contains a read-only collection
        /// of <see cref="Currency"/> objects.
        /// </returns>
        Task<IReadOnlyCollection<Currency>> GetSupportedCurrenciesAsync();
    }
}
