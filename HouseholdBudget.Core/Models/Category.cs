namespace HouseholdBudget.Core.Models
{
    public enum CategoryType
    {
        Income,
        Expense
    }

    public class Category
    {
        public Guid Id { get; set; } = Guid.Empty;

        public Guid UserId { get; set; } = Guid.Empty;

        public string Name { get; set; } = string.Empty;

        public CategoryType Type { get; set; } = CategoryType.Expense;

    }
}
