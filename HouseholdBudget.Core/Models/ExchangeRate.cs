using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace HouseholdBudget.Core.Models
{
    /// <summary>
    /// Represents an exchange rate between two currencies at a specific point in time.
    /// </summary>
    [DebuggerDisplay("{ToString(),nq}")]
    public class ExchangeRate : AuditableEntity
    {
        /// <summary>
        /// The ISO currency code for the source currency.
        /// </summary>
        [Required]
        [MaxLength(Currency.CodeLength)]
        [StringLength(Currency.CodeLength, MinimumLength = Currency.CodeLength)]
        public string FromCurrencyCode { get; private set; } = string.Empty;

        /// <summary>
        /// The ISO currency code for the target currency.
        /// </summary>
        [Required]
        [MaxLength(Currency.CodeLength)]
        [StringLength(Currency.CodeLength, MinimumLength = Currency.CodeLength)]
        public string ToCurrencyCode { get; private set; } = string.Empty;

        /// <summary>
        /// The exchange rate value from <see cref="FromCurrencyCode"/> to <see cref="ToCurrencyCode"/>.
        /// Must be greater than zero.
        /// </summary>
        [Required]
        [Range(0.000001, double.MaxValue)]
        public decimal Rate { get; private set; }

        /// <summary>
        /// The UTC date and time when the exchange rate was retrieved.
        /// </summary>
        public DateTime RetrievedAt { get; private set; } = DateTime.UtcNow;

        /// <summary>
        /// Private constructor required for ORM and factory usage.
        /// </summary>
        private ExchangeRate() { }

        /// <summary>
        /// Creates a new instance of <see cref="ExchangeRate"/> with validation.
        /// </summary>
        /// <param name="fromCurrencyCode">The source currency ISO code.</param>
        /// <param name="toCurrencyCode">The target currency ISO code.</param>
        /// <param name="rate">The exchange rate value.</param>
        /// <returns>A valid <see cref="ExchangeRate"/> instance.</returns>
        /// <exception cref="ValidationException">Thrown when input values are invalid.</exception>
        public static ExchangeRate Create(string fromCurrencyCode, string toCurrencyCode, decimal rate)
        {
            EnsureIsValid(fromCurrencyCode, toCurrencyCode, rate);

            return new ExchangeRate {
                FromCurrencyCode = fromCurrencyCode.ToUpperInvariant(),
                ToCurrencyCode   = toCurrencyCode  .ToUpperInvariant(),
                Rate             = rate,
                RetrievedAt      = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Updates the exchange rate value and retrieval timestamp.
        /// </summary>
        /// <param name="newRate">The new exchange rate to set.</param>
        /// <exception cref="ValidationException">Thrown if the new rate is invalid or zero/negative.</exception>
        public void UpdateRate(decimal newRate)
        {
            var err = ValidateRate(newRate);
            if (err.Any())
                throw new ValidationException(string.Join("; ", err));

            if (Rate != newRate)
            {
                Rate        = newRate;
                RetrievedAt = DateTime.UtcNow;

                MarkAsUpdated();
            }
        }


        /// <summary>
        /// Validates the exchange rate value.
        /// </summary>
        /// <param name="rate">Exchange rate value to validate.</param>
        /// <returns>Validation error message(s), if any.</returns>
        public static IEnumerable<string> ValidateRate(decimal rate)
        {
            if (rate <= 0)
                yield return "Exchange rate must be greater than zero.";
        }

        /// <summary>
        /// Validates input values and returns a list of validation errors.
        /// </summary>
        /// <param name="from">Source currency code.</param>
        /// <param name="to">Target currency code.</param>
        /// <param name="rate">Exchange rate value.</param>
        /// <returns>List of validation errors; empty if valid.</returns>
        public static IReadOnlyList<string> Validate(string from, string to, decimal rate)
        {
            return Currency.ValidateCode(from)
                .Select(err => $"FromCurrencyCode: {err}")
                .Concat(Currency.ValidateCode(to)
                .Select(err => $"ToCurrencyCode: {err}"))
                .Concat(ValidateRate(rate))
                .ToList();
        }

        /// <summary>
        /// Validates input values and throws a <see cref="ValidationException"/> if invalid.
        /// </summary>
        /// <param name="from">Source currency code.</param>
        /// <param name="to">Target currency code.</param>
        /// <param name="rate">Exchange rate value.</param>
        /// <exception cref="ValidationException">Thrown when input values are invalid.</exception>
        private static void EnsureIsValid(string from, string to, decimal rate)
        {
            var errors = Validate(from, to, rate);
            if (errors.Count > 0)
                throw new ValidationException(string.Join("; ", errors));
        }

        public override string ToString()
        {
            var created = $"Created: {CreatedAt:u}";
            var updated = UpdatedAt.HasValue ? $" | Updated: {UpdatedAt:u}" : "";

            return $"{FromCurrencyCode ?? "???"} -> {ToCurrencyCode ?? "???"} : Rate -> {Rate:F6} | RetrievedAt -> {RetrievedAt:u} | Id: {Id} | {created}{updated}";
        }
    }
}
