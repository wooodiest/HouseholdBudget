using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace HouseholdBudget.Core.Models
{
    /// <summary>
    /// Represents a currency used for budgeting and transactions (e.g., USD, EUR).
    /// </summary>
    [DebuggerDisplay("{ToString(),nq}")]
    public class Currency : AuditableEntity
    {
        /// <summary>
        /// The length of the currency code (e.g., "USD").
        /// </summary>
        public const int CodeLength = 3;

        /// <summary>
        /// The maximum allowed length of the currency symbol (e.g., "$").
        /// </summary>
        public const int MaxSymbolLength = 5;

        /// <summary>
        /// The maximum allowed length of the currency name (e.g., "US Dollar").
        /// </summary>
        public const int MaxNameLength = 100;

        /// <summary>
        /// The 3-letter ISO code of the currency.
        /// </summary>
        [Required]
        [MaxLength(CodeLength)]
        [StringLength(CodeLength, MinimumLength = CodeLength)]
        public string Code { get; private set; } = string.Empty;

        /// <summary>
        /// The currency symbol (e.g., $, €, zł).
        /// </summary>
        [Required, MaxLength(MaxSymbolLength)]
        public string Symbol { get; private set; } = string.Empty;

        /// <summary>
        /// The full name of the currency.
        /// </summary>
        [Required, MaxLength(MaxNameLength)]
        public string Name { get; private set; } = string.Empty;

        /// <summary>
        /// Private constructor required for ORM and factory usage.
        /// </summary>
        private Currency() { }

        /// <summary>
        /// Creates a new instance of <see cref="Currency"/> with validation.
        /// </summary>
        /// <param name="code">The 3-letter ISO currency code.</param>
        /// <param name="symbol">The currency symbol (e.g., "$").</param>
        /// <param name="name">The full currency name (e.g., "US Dollar").</param>
        /// <returns>A new valid <see cref="Currency"/> instance.</returns>
        /// <exception cref="ValidationException">Thrown when one or more values are invalid.</exception>
        public static Currency Create(string code, string symbol, string name)
        {
            EnsureIsValid(code, symbol, name);

            return new Currency {
                Code   = code.ToUpperInvariant(),
                Symbol = symbol,
                Name   = name
            };
        }

        /// <summary>
        /// Validates the currency code format.
        /// </summary>
        /// <param name="code">The currency code to validate.</param>
        /// <returns>Validation errors if any; otherwise, empty list.</returns>
        public static IEnumerable<string> ValidateCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code) || !Regex.IsMatch(code, @"^[A-Za-z]{3}$"))
                yield return "Currency code must be a valid 3-letter ISO code.";
        }

        /// <summary>
        /// Validates the currency symbol.
        /// </summary>
        /// <param name="symbol">The currency symbol to validate.</param>
        /// <returns>Validation errors if any; otherwise, empty list.</returns>
        public static IEnumerable<string> ValidateSymbol(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
                yield return "Currency symbol is required.";
            else if (symbol.Length > MaxSymbolLength)
                yield return $"Currency symbol cannot exceed {MaxSymbolLength} characters.";
        }

        /// <summary>
        /// Validates the currency name.
        /// </summary>
        /// <param name="name">The currency name to validate.</param>
        /// <returns>Validation errors if any; otherwise, empty list.</returns>
        public static IEnumerable<string> ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                yield return "Currency name is required.";
            else if (name.Length > MaxNameLength)
                yield return $"Currency name cannot exceed {MaxNameLength} characters.";
        }

        /// <summary>
        /// Validates all currency fields and returns a combined list of error messages.
        /// </summary>
        /// <param name="code">Currency code to validate.</param>
        /// <param name="symbol">Currency symbol to validate.</param>
        /// <param name="name">Currency name to validate.</param>
        /// <returns>A list of validation errors; empty if valid.</returns>
        public static IReadOnlyList<string> Validate(string code, string symbol, string name)
        {
            return ValidateCode(code)
                .Concat(ValidateSymbol(symbol))
                .Concat(ValidateName(name))
                .ToList();
        }

        /// <summary>
        /// Validates currency fields and throws a <see cref="ValidationException"/> if invalid.
        /// </summary>
        /// <param name="code">The currency code to validate.</param>
        /// <param name="symbol">The currency symbol to validate.</param>
        /// <param name="name">The currency name to validate.</param>
        /// <exception cref="ValidationException">Thrown if any value is invalid.</exception>
        private static void EnsureIsValid(string code, string symbol, string name)
        {
            var errors = Validate(code, symbol, name);
            if (errors.Count > 0)
                throw new ValidationException(string.Join("; ", errors));
        }

        public override string ToString()
        {
            var created = $"Created: {CreatedAt:u}";
            var updated = UpdatedAt.HasValue ? $" | Updated: {UpdatedAt:u}" : "";

            return $"{Code} ({Symbol}) : {Name} | Id: {Id} | {created}{updated}";
        }

    }
}
