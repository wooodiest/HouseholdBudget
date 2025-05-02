namespace HouseholdBudget.Core.Models
{
    public class Transaction
    {
        public required Guid Id { get; set; }

        public required DateTime Date { get; set; }

        public required string Description { get; set; }

        public required decimal Amount { get; set; }

        public Guid CategoryId { get; set; }

        public bool IsRecurring { get; set; } = false;
    }
}
