namespace HouseholdBudget.Desktop.ViewModels
{
    public class TransactionViewModelItem
    {
        public Guid Id { get; set; }

        public DateTime Date { get; set; }

        public string Description { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string CategoryName { get; set; } = string.Empty;
    }
}
