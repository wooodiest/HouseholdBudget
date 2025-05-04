using System.ComponentModel.DataAnnotations;

namespace HouseholdBudget.Core.Models
{
    public class Transaction
    {
        public Guid Id { get; set; } = Guid.Empty;

        public Guid UserId { get; set; } = Guid.Empty;

        public DateTime Date { get; set; } = DateTime.UtcNow;

        public Guid CategoryId { get; set; } = Guid.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal Amount { get; set; } = 0;

        public bool IsRecurring { get; set; } = false;
    }
}
