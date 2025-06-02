using System.ComponentModel.DataAnnotations;

namespace HouseholdBudget.Core.Models
{
    /// <summary>
    /// Represents a budget allocation for a specific category within a budget plan.
    /// </summary>
    public class CategoryBudgetPlan : AuditableEntity
    {
        /// <summary>
        /// The unique identifier of the category to which this budget allocation applies.
        /// </summary>
        [Required]
        public Guid CategoryId { get; init; }

        /// <summary>
        /// Initial planned income allocations for this category.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal IncomePlanned { get; init; } = 0.0m;

        /// <summary>
        /// Income that has been executed against this budget plan.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal IncomeExecuted { get; private set; } = 0.0m;

        /// <summary>
        /// Initial planned expense allocations for this category.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal ExpensePlanned { get; init; } = 0.0m;

        /// <summary>
        /// Expenses that have been executed against this budget plan.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal ExpenseExecuted { get; private set; } = 0.0m;

        /// <summary>
        /// The currency in which the budget amount is defined.
        /// This is typically aligned with the user's default planning currency.
        /// </summary>
        [Required]
        public Currency Currency { get; init; } = null!;

        /// <summary>
        /// Private constructor required for ORM and factory usage.
        /// </summary>
        private CategoryBudgetPlan() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CategoryBudgetPlan"/> class
        /// with the specified category, amount, and currency.
        /// </summary>
        /// <param name="categoryId">The ID of the budgeted category.</param>
        /// <param name="incomePlanned">The planned income allocation for this category.</param>
        /// <param name="expensePlanned">The planned expense allocation for this category.</param>
        /// <param name="currency">The currency in which the budget is set.</param>
        /// <exception cref="ValidationException">Thrown if input values are invalid.</exception>
        public CategoryBudgetPlan(Guid categoryId, decimal incomePlanned, decimal expensePlanned, Currency currency)
        {
            if (categoryId == Guid.Empty)
                throw new ValidationException("CategoryId is required.");
            if (incomePlanned < 0)
                throw new ValidationException("Amount must be non-negative.");
            if (expensePlanned < 0)
                throw new ValidationException("Amount must be non-negative.");
            if (currency == null)
                throw new ValidationException("Currency must be provided.");

            CategoryId     = categoryId;
            IncomePlanned  = incomePlanned;
            ExpensePlanned = expensePlanned;
            Currency       = currency;
        }

        /// <summary>
        /// Updates the budgeted amount for this category.
        public void AddExecution(decimal income, decimal expense)
        {
            IncomeExecuted  += income;
            ExpenseExecuted += expense;
        }

        /// <summary>
        /// Clears the executed amount for this budget plan,
        /// allowing for a fresh start in tracking expenses.
        public void ClearExecution()
        {
            IncomeExecuted  = 0;
            ExpenseExecuted = 0;
        }

        /// <summary>
        /// Returns a human-readable representation of the budgeted category and amount.
        /// </summary>
        public override string ToString()
        {
            var created = $"Created: {CreatedAt:u}";
            var updated = UpdatedAt.HasValue ? $" | Updated: {UpdatedAt:u}" : "";
            var amountFormatted = $"Income: {IncomePlanned:F2}/{IncomeExecuted:F2}, Expenses {ExpensePlanned:F2}/{ExpenseExecuted:F2} {Currency?.Code ?? "???"}";

            return $"{amountFormatted} | Id: {Id} | {created}{updated}";
        }
    }
}
