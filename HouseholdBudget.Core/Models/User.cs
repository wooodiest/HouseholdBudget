namespace HouseholdBudget.Core.Models
{
    public class User
    {
        public required Guid Id { get; init; }

        public required string Name { get; init; } = string.Empty;

        public required string PasswordHash { get; init; } = string.Empty;

        public required string DefaultCurrencyCode { get; init; }

        public required DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    }
}
