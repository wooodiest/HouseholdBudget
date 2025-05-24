using HouseholdBudget.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace HouseholdBudget.Core.UserData
{
    /// <summary>
    /// Represents an individual user of the household budgeting system.
    /// </summary>
    public class User
    {
        /// <summary>
        /// The maximum allowed length for a user's name.
        /// </summary>
        public const int MaxNameLength = 100;

        /// <summary>
        /// The maximum allowed length for a user's email address.
        /// </summary>
        public const int MaxEmailLength = 150;

        /// <summary>
        /// The expected length for a currency code (e.g., "USD").
        /// </summary>
        public const int MaxCurrencyCodeLength = 3;

        /// <summary>
        /// Gets the unique identifier of the user.
        /// </summary>
        public required Guid Id { get; init; }

        /// <summary>
        /// Gets the full name of the user.
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Gets the email address associated with the user.
        /// </summary>
        public required string Email { get; init; }

        /// <summary>
        /// Gets the hashed password used for user authentication.
        /// </summary>
        public required string PasswordHash { get; init; }

        /// <summary>
        /// Gets the default currency code (e.g., "USD", "EUR") for the user's transactions.
        /// </summary>
        public required string DefaultCurrencyCode { get; init; }

        /// <summary>
        /// Gets the date and time when the user account was created (in UTC).
        /// </summary>
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

        /// <summary>
        /// Creates a new <see cref="User"/> instance with validated and normalized data.
        /// This factory method ensures that all domain rules and constraints are enforced
        /// at the point of creation, preventing invalid or incomplete user objects from being instantiated.
        /// </summary>
        /// <param name="name">The full name of the user.</param>
        /// <param name="email">The user's email address.</param>
        /// <param name="passwordHash">The hashed password string (already hashed, not raw).</param>
        /// <param name="defaultCurrencyCode">The user's default currency code (e.g., "USD").</param>
        /// <returns>A fully constructed and validated <see cref="User"/> instance.</returns>
        /// <exception cref="ValidationException">
        /// Thrown when one or more of the input fields fail domain validation.
        /// </exception>
        public static User Create(
            string name,
            string email,
            string passwordHash,
            string defaultCurrencyCode)
        {
            EnsureIsValid(name, email, passwordHash, defaultCurrencyCode);

            return new User {
                Id                  = Guid.NewGuid(),
                Name                = name.Trim(),
                Email               = email.Trim().ToLowerInvariant(),
                PasswordHash        = passwordHash,
                DefaultCurrencyCode = defaultCurrencyCode.Trim().ToUpperInvariant(),
            };
        }

        /// <summary>
        /// Validates the user's name and returns a list of validation error messages.
        /// </summary>
        /// <param name="name">The name to validate.</param>
        public static IReadOnlyList<string> ValidateName(string name)
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(name))
                errors.Add("Name is required.");
            else if (name.Length > MaxNameLength)
                errors.Add($"Name cannot exceed {MaxNameLength} characters.");

            return errors;
        }

        /// <summary>
        /// Validates the user's email and returns a list of validation error messages.
        /// </summary>
        /// <param name="email">The email address to validate.</param>
        public static IReadOnlyList<string> ValidateEmail(string email)
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(email))
            {
                errors.Add("Email is required.");
            }
            else
            {
                if (email.Length > MaxEmailLength)
                    errors.Add($"Email cannot exceed {MaxEmailLength} characters.");
                if (!new EmailAddressAttribute().IsValid(email))
                    errors.Add("Email format is invalid.");
            }

            return errors;
        }

        /// <summary>
        /// Validates the user's password hash and returns a list of validation error messages.
        /// </summary>
        /// <param name="hash">The password hash to validate.</param>
        public static IReadOnlyList<string> ValidatePasswordHash(string hash)
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(hash))
                errors.Add("Password hash is required.");

            return errors;
        }

        /// <summary>
        /// Validates the user's default currency code and returns a list of validation error messages.
        /// </summary>
        /// <param name="code">The currency code to validate.</param>
        public static IEnumerable<string> ValidateCurrencyCode(string code)
        {
            return Currency.ValidateCode(code);
        }

        /// <summary>
        /// Validates all user fields and returns a list of validation error messages.
        /// </summary>
        /// <param name="name">The user's name.</param>
        /// <param name="email">The user's email.</param>
        /// <param name="passwordHash">The hashed password.</param>
        /// <param name="defaultCurrencyCode">The default currency code.</param>
        public static IReadOnlyList<string> Validate(
            string name,
            string email,
            string passwordHash,
            string defaultCurrencyCode)
        {
            return ValidateName(name)
                .Concat(ValidateEmail(email))
                .Concat(ValidatePasswordHash(passwordHash))
                .Concat(ValidateCurrencyCode(defaultCurrencyCode))
                .ToList();
        }

        /// <summary>
        /// Validates all fields and throws a <see cref="ValidationException"/> if any are invalid.
        /// </summary>
        /// <param name="name">The user's name.</param>
        /// <param name="email">The user's email.</param>
        /// <param name="passwordHash">The hashed password.</param>
        /// <param name="defaultCurrencyCode">The default currency code.</param>
        /// <exception cref="ValidationException">Thrown when one or more fields are invalid.</exception>
        public static void EnsureIsValid(
            string name,
            string email,
            string passwordHash,
            string defaultCurrencyCode)
        {
            var errors = Validate(name, email, passwordHash, defaultCurrencyCode);
            if (errors.Count > 0)
                throw new ValidationException(string.Join("; ", errors));
        }
    }
}
