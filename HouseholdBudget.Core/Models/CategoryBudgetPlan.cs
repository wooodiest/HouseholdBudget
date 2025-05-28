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
        /// The amount of money allocated for this category.
        /// This value must be non-negative and expressed in the associated currency.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal Amount { get; init; } = 0.0m;

        /// <summary>
        /// The amount of money that has been executed (spent) against this budget.
        [Range(0, double.MaxValue)]
        public decimal ExecutedAmount { get; private set; } = 0.0m;

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
        /// <param name="amount">The planned monetary allocation for this category.</param>
        /// <param name="currency">The currency in which the budget is set.</param>
        /// <exception cref="ValidationException">Thrown if input values are invalid.</exception>
        public CategoryBudgetPlan(Guid categoryId, decimal amount, Currency currency)
        {
            if (categoryId == Guid.Empty)
                throw new ValidationException("CategoryId is required.");
            if (amount < 0)
                throw new ValidationException("Amount must be non-negative.");
            if (currency == null)
                throw new ValidationException("Currency must be provided.");

            CategoryId = categoryId;
            Amount     = amount;
            Currency   = currency;
        }

        /// <summary>
        /// Updates the budgeted amount for this category.
        public void AddExecution(decimal amount)
        {
            ExecutedAmount += amount;
        }

        /// <summary>
        /// Clears the executed amount for this budget plan,
        /// allowing for a fresh start in tracking expenses.
        public void ClearExecution()
        {
            ExecutedAmount = 0;
        }

        /// <summary>
        /// Returns a human-readable representation of the budgeted category and amount.
        /// </summary>
        public override string ToString()
        {
            var created = $"Created: {CreatedAt:u}";
            var updated = UpdatedAt.HasValue ? $" | Updated: {UpdatedAt:u}" : "";
            var amountFormatted = $"{Amount:F2}/{ExecutedAmount:F2} {Currency?.Code ?? "???"}";

            return $"{amountFormatted} | Id: {Id} | {created}{updated}";
        }
    }
}
