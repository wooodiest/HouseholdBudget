namespace HouseholdBudget.Core.Models
{
    public class BudgetPeriod
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; } = Guid.Empty;

        public string Name { get; set; } = string.Empty;

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public Currency BaseCurrency { get; set; } = new();
        public BudgetPeriodStatus Status { get; set; } = BudgetPeriodStatus.Planned;

        public List<Budget> Budgets { get; set; } = new();
        public string Notes { get; set; } = string.Empty;
    }

    public enum BudgetPeriodStatus
    {
        Planned,
        Active,
        Completed,
        Exceeded
    }
}
