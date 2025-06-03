using System.ComponentModel.DataAnnotations;

namespace HouseholdBudget.Core.Models
{
    /// <summary>
    /// Represents a budget allocation for a specific category within a budget plan.
    /// </summary>
    public class CategoryBudgetPlan : AuditableEntity
    {
        /// <summary>
        /// The unique identifier of the budget plan to which this category budget allocation belongs.
        /// </summary>
        public Guid BudgetPlanId { get; set; }

        /// <summary>
        /// The unique identifier of the category to which this budget allocation applies.
        /// </summary>
        [Required]
        public Guid CategoryId { get; init; }

        /// <summary>
        /// Initial planned income allocations for this category.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal IncomePlanned { get; private set; } = 0.0m;

        /// <summary>
        /// Income that has been executed against this budget plan.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal IncomeExecuted { get; private set; } = 0.0m;

        /// <summary>
        /// Initial planned expense allocations for this category.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal ExpensePlanned { get; private set; } = 0.0m;

        /// <summary>
        /// Expenses that have been executed against this budget plan.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal ExpenseExecuted { get; private set; } = 0.0m;

        /// <summary>
        /// Currency code in which the budget is set, e.g., "USD", "EUR".
        /// </summary>
        public string CurrencyCode { get; private set; } = string.Empty;

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
        public CategoryBudgetPlan(Guid planID,  Guid categoryId, decimal incomePlanned, decimal expensePlanned, string currencyCode)
        {
            BudgetPlanId = planID;
            if (categoryId == Guid.Empty)
                throw new ValidationException("CategoryId is required.");
            if (incomePlanned < 0)
                throw new ValidationException("Amount must be non-negative.");
            if (expensePlanned < 0)
                throw new ValidationException("Amount must be non-negative.");
            if (currencyCode == null)
                throw new ValidationException("Currency must be provided.");

            CategoryId     = categoryId;
            IncomePlanned  = incomePlanned;
            ExpensePlanned = expensePlanned;
            CurrencyCode   = currencyCode;
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
        /// Sets the currency code for this budget plan.
        /// <summary>
        /// <param name="currencyCode" >The currency code to set, e.g., "USD", "EUR".</param>
        public void SetCurrency(string currencyCode)
        {
            if (string.IsNullOrWhiteSpace(currencyCode))
                throw new ValidationException("Currency must be provided.");
            CurrencyCode = currencyCode;
        }

        /// <summary>
        /// Sets the planned income and expense amounts for this budget plan.
        /// <summary>
        /// <param name="incomePlanned">The planned income allocation for this category.</param>
        public void SetIncomePlanned(decimal incomePlanned)
        {
            if (incomePlanned < 0)
                throw new ValidationException("Income planned must be non-negative.");

            IncomePlanned = incomePlanned;
        }

        /// <summary>
        /// Sets the planned expense and expense amounts for this budget plan.
        /// <summary>
        /// <param name="incomePlanned">The planned expense allocation for this category.</param>
        public void SetExpensePlanned(decimal expensePlanned)
        {
            if (expensePlanned < 0)
                throw new ValidationException("Expense planned must be non-negative.");
            ExpensePlanned = expensePlanned;
        }

        /// <summary>
        /// Returns a human-readable representation of the budgeted category and amount.
        /// </summary>
        public override string ToString()
        {
            var created = $"Created: {CreatedAt:u}";
            var updated = UpdatedAt.HasValue ? $" | Updated: {UpdatedAt:u}" : "";
            var amountFormatted = $"Income: {IncomePlanned:F2}/{IncomeExecuted:F2}, Expenses {ExpensePlanned:F2}/{ExpenseExecuted:F2} {CurrencyCode ?? "???"}";

            return $"{amountFormatted} | Id: {Id} | {created}{updated}";
        }
    }
}
