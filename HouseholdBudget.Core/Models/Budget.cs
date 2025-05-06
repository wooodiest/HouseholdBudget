namespace HouseholdBudget.Core.Models
{
    public class Budget
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; } = Guid.Empty;
        public Guid BudgetPeriodId { get; set; } = Guid.Empty;
        public string Name { get; set; } = string.Empty;

        public Guid CategoryId { get; set; } = Guid.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public decimal Limit { get; set; }
        public decimal Used { get; set; } = 0;
        public Currency Currency { get; set; } = new();

        public bool IsExceeded => Used > Limit;
    }
}
