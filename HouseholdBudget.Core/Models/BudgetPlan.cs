using System.ComponentModel.DataAnnotations;

namespace HouseholdBudget.Core.Models
{
    /// <summary>
    /// Represents a financial budget plan for a specific time range, including planned total allocation
    /// and per-category budget limits. This plan does not compute totals itself, and assumes external services
    /// will handle normalization and evaluation based on user's currency.
    /// </summary>
    public class BudgetPlan : AuditableEntity
    {
        /// <summary>
        /// The maximum allowed length of the plan description.
        /// </summary>
        public const int MaxDescriptionLength = 250;

        /// <summary>
        /// Optional description to help identify or label this budget plan.
        /// </summary>
        [MaxLength(MaxDescriptionLength)]
        public string Description { get; private set; } = string.Empty;

        /// <summary>
        /// Start date of the budget plan period.
        /// </summary>
        [Required]
        public DateTime StartDate { get; private set; }

        /// <summary>
        /// End date of the budget plan period.
        /// </summary>
        [Required]
        public DateTime EndDate { get; private set; }

        /// <summary>
        /// The total amount allocated for the entire budget plan.
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal TotalAmount { get; private set; } = 0.0m;

        /// <summary>
        /// The base currency used for the budget plan totals.
        /// </summary>
        [Required]
        public Currency Currency { get; private set; } = null!;

        /// <summary>
        /// The category-specific breakdown of the plan.
        /// </summary>
        public List<CategoryBudgetPlan> CategoryPlans { get; private set; } = new();

        private BudgetPlan() { }

        /// <summary>
        /// Factory method to create a validated instance of <see cref="BudgetPlan"/>.
        /// </summary>
        /// <param name="startDate">The starting date of the budget plan.</param>
        /// <param name="endDate">The ending date of the budget plan.</param>
        /// <param name="totalAmount">Total amount planned for this budget.</param>
        /// <param name="currency">Currency of the planned amount.</param>
        /// <param name="description">Optional description of the plan.</param>
        /// <param name="categoryPlans">Optional category plans associated with this budget.</param>
        /// <returns>A validated instance of <see cref="BudgetPlan"/>.</returns>
        /// <exception cref="ValidationException">Thrown if input values are invalid.</exception>
        public static BudgetPlan Create(
            DateTime startDate,
            DateTime endDate,
            decimal totalAmount,
            Currency currency,
            string? description = null,
            IEnumerable<CategoryBudgetPlan>? categoryPlans = null)
        {
            var errors = Validate(startDate, endDate, totalAmount, currency, description);
            if (errors.Count > 0)
                throw new ValidationException(string.Join("; ", errors));

            return new BudgetPlan {
                StartDate     = startDate.Date,
                EndDate       = endDate.Date,
                TotalAmount   = totalAmount,
                Currency      = currency,
                Description   = description?.Trim() ?? string.Empty,
                CategoryPlans = categoryPlans?.ToList() ?? new()
            };
        }

        /// <summary>
        /// Checks if the given date falls within the plan's time range.
        /// </summary>
        public bool IncludesDate(DateTime date) =>
            date.Date >= StartDate.Date && date.Date <= EndDate.Date;

        /// <summary>
        /// Returns a display-friendly summary of the plan.
        /// </summary>
        public override string ToString()
        {
            var label = string.IsNullOrWhiteSpace(Description) ? "Budget Plan" : Description;
            return $"{label} ({StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd})";
        }

        /// <summary>
        /// Validates all properties of a budget plan and returns a list of error messages.
        /// </summary>
        public static IReadOnlyList<string> Validate(
            DateTime startDate,
            DateTime endDate,
            decimal totalAmount,
            Currency? currency,
            string? description)
        {
            var errors = new List<string>();

            if (startDate > endDate)
                errors.Add("StartDate must be earlier than or equal to EndDate.");

            if (totalAmount < 0)
                errors.Add("TotalAmount must be non-negative.");

            if (currency == null)
                errors.Add("Currency is required.");

            if (!string.IsNullOrWhiteSpace(description) && description.Length > MaxDescriptionLength)
                errors.Add($"Description cannot exceed {MaxDescriptionLength} characters.");

            return errors;
        }
    }
}
