namespace HouseholdBudget.Core.Models
{
    /// <summary>
    /// Represents an individual user of the household budgeting system.
    /// </summary>
    public class User
    {
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
        /// Gets the date and time the user account was created, in UTC.
        /// </summary>
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    }
}
