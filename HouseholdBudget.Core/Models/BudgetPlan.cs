using System.ComponentModel.DataAnnotations;

namespace HouseholdBudget.Core.Models
{
    /// <summary>
    /// Represents a comprehensive financial budget plan for a specific time range, 
    /// including planned total allocation, per-category budget limits, and execution tracking.
    /// This plan serves as a blueprint for financial management and does not perform currency
    /// conversions or total computations internally. External services are responsible for
    /// currency normalization and financial evaluation.
    /// </summary>
    public class BudgetPlan : AuditableEntity
    {
        /// <summary>
        /// Maximum allowed length for the budget plan name.
        /// </summary>
        public const int MaxNameLength = 100;

        /// <summary>
        /// Maximum allowed length for the budget plan description.
        /// </summary>
        public const int MaxDescriptionLength = 250;

        /// <summary>
        /// Unique identifier of the user who created the budget plan.
        /// </summary>
        public Guid UserId { get; private set; }

        /// <summary>
        /// The display name of the budget plan. Used for quick identification in UI elements.
        /// Must not exceed <see cref="MaxNameLength"/> characters.
        /// </summary>
        [MaxLength(MaxNameLength)]
        public string Name { get; private set; } = string.Empty;

        /// <summary>
        /// Detailed description providing context about the budget plan's purpose or scope.
        /// Must not exceed <see cref="MaxDescriptionLength"/> characters.
        /// </summary>
        [MaxLength(MaxDescriptionLength)]
        public string Description { get; private set; } = string.Empty;

        /// <summary>
        /// The starting date of the budget period.
        /// </summary>
        [Required]
        public DateTime StartDate { get; private set; }

        /// <summary>
        /// The ending date of the budget period.
        /// </summary>
        [Required]
        public DateTime EndDate { get; private set; }

        /// <summary>
        /// Collection of category-specific budget allocations that comprise this plan.
        /// Each entry defines the monetary limit for a specific financial category.
        /// </summary>
        public List<CategoryBudgetPlan> CategoryPlans { get; private set; } = new();

        /// <summary>
        /// Private constructor for ORM compatibility and factory pattern enforcement.
        /// Prevents uncontrolled instance creation outside of factory methods.
        /// </summary>
        private BudgetPlan() { }

        /// <summary>
        /// Factory method for creating valid <see cref="BudgetPlan"/> instances with
        /// comprehensive input validation. Ensures business rule compliance at creation.
        /// </summary>
        /// <param name="userId">Unique identifier of the user creating the budget plan.</param>
        /// <param name="name">Display name for the budget plan.</param>
        /// <param name="startDate">Start date of the budget period.</param>
        /// <param name="endDate">End date of the budget period.</param>
        /// <param name="description">Optional descriptive text (max 250 chars).</param>
        /// <param name="categoryPlans">Optional initial category allocations.</param>
        /// <returns>A fully initialized, valid BudgetPlan instance.</returns>
        /// <exception cref="ValidationException">
        /// Thrown when any input parameter violates business rules or constraints.
        /// </exception>
        public static BudgetPlan Create(
            Guid userId,
            string name,
            DateTime startDate,
            DateTime endDate,
            string? description = null,
            IEnumerable<CategoryBudgetPlan>? categoryPlans = null)
        {
            ValidateDateRange(startDate, endDate);
            ValidateDescription(description);
            ValidateName(name);

            return new BudgetPlan {
                UserId        = userId,
                Name          = name.Trim(),
                StartDate     = startDate.Date,
                EndDate       = endDate.Date,
                Description   = description?.Trim() ?? string.Empty,
                CategoryPlans = categoryPlans?.ToList() ?? new()
            };
        }

        /// <summary>
        /// Resets all execution tracking to zero. Used when recalculating budget performance
        /// or resetting financial period tracking.
        /// </summary>
        public void ClearExecution()
        {
            foreach (var cp in CategoryPlans)
            {
                cp.ClearExecution();
            }
        }

        /// <summary>
        /// Determines whether a given date falls within this budget plan's active period.
        /// </summary>
        /// <param name="date">The date to check (time component ignored).</param>
        /// <returns>
        /// True if the date is between <see cref="StartDate"/> and <see cref="EndDate"/>, inclusive.
        /// </returns>
        public bool IncludesDate(DateTime date) =>
            date.Date >= StartDate.Date && date.Date <= EndDate.Date;

        /// <summary>
        /// Generates a human-readable representation of the budget plan.
        /// Prioritizes description if available, falling back to default labeling.
        /// </summary>
        /// <returns>Formatted string containing key plan identifiers.</returns>
        public override string ToString()
        {
            var label = string.IsNullOrWhiteSpace(Description) ? "Budget Plan" : Description;
            return $"{label} ({StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd})";
        }

        /// <summary>
        /// Updates the display name with validation.
        /// </summary>
        /// <param name="newName">New name value.</param>
        /// <exception cref="ValidationException">
        /// Thrown if name exceeds <see cref="MaxNameLength"/>.
        /// </exception>
        public void UpdateName(string newName)
        {
            ValidateName(newName);
            Name = newName.Trim();
            MarkAsUpdated();
        }

        /// <summary>
        /// Updates the descriptive text with validation.
        /// </summary>
        /// <param name="newDescription">New description value.</param>
        /// <exception cref="ValidationException">
        /// Thrown if description exceeds <see cref="MaxDescriptionLength"/>.
        /// </exception>
        public void UpdateDescription(string newDescription)
        {
            ValidateDescription(newDescription);
            Description = newDescription.Trim();
            MarkAsUpdated();
        }

        /// <summary>
        /// Updates the budget period dates with range validation.
        /// </summary>
        /// <param name="newStartDate">New start date.</param>
        /// <param name="newEndDate">New end date.</param>
        /// <exception cref="ValidationException">
        /// Thrown if start date is after end date.
        /// </exception>
        public void UpdateDates(DateTime newStartDate, DateTime newEndDate)
        {
            ValidateDateRange(newStartDate, newEndDate);
            StartDate = newStartDate.Date;
            EndDate = newEndDate.Date;
            MarkAsUpdated();
        }

        /// <summary>
        /// Replaces all category budget plans with a new collection.
        /// </summary>
        /// <param name="newPlans">New collection of category plans.</param>
        /// <exception cref="ValidationException">
        /// Thrown if input is null.
        /// </exception>
        public void UpdateCategoryPlans(IEnumerable<CategoryBudgetPlan> newPlans)
        {
            if (newPlans == null)
                throw new ValidationException("Category plans cannot be null.");

            CategoryPlans = newPlans.ToList();
            MarkAsUpdated();
        }

        /// <summary>
        /// Updates an existing category budget plan or adds it if it does not exist.
        /// </summary>
        /// <param name="updatedPlan">Updated category budget plan.</param>
        public void UpdateCategoryPlan(CategoryBudgetPlan updatedPlan)
        {
            if (updatedPlan == null)
                throw new ValidationException("Updated category plan cannot be null.");

            var existingPlan = CategoryPlans.FirstOrDefault(p => p.CategoryId == updatedPlan.CategoryId);
            if (existingPlan != null)
            {
                existingPlan = updatedPlan;
            }
            else
            {
                CategoryPlans.Add(updatedPlan);
            }

            MarkAsUpdated();
        }

        /// <summary>
        /// Adds a single category budget plan to the collection.
        /// </summary>
        /// <param name="plan">Category plan to add.</param>
        /// <exception cref="ValidationException">
        /// Thrown if input is null.
        /// </exception>
        public void AddCategoryPlan(CategoryBudgetPlan plan)
        {
            if (plan == null)
                throw new ValidationException("Category plan cannot be null.");

            CategoryPlans.Add(plan);
            MarkAsUpdated();
        }

        /// <summary>
        /// Removes all category plans associated with the specified category ID.
        /// </summary>
        /// <param name="categoryId">Target category identifier.</param>
        public void RemoveCategoryPlan(Guid categoryId)
        {
            CategoryPlans.RemoveAll(p => p.CategoryId == categoryId);
            MarkAsUpdated();
        }

        /// <summary>
        /// Validates name length constraints.
        /// </summary>
        /// <param name="name">Name to validate.</param>
        /// <exception cref="ValidationException">
        /// Thrown if name exceeds <see cref="MaxNameLength"/>.
        /// </exception>
        private static void ValidateName(string? name)
        {
            if (!string.IsNullOrWhiteSpace(name) && name.Length > MaxNameLength)
                throw new ValidationException($"Name cannot exceed {MaxNameLength} characters.");
        }

        /// <summary>
        /// Validates description length constraints.
        /// </summary>
        /// <param name="description">Description to validate.</param>
        /// <exception cref="ValidationException">
        /// Thrown if description exceeds <see cref="MaxDescriptionLength"/>.
        /// </exception>
        private static void ValidateDescription(string? description)
        {
            if (!string.IsNullOrWhiteSpace(description) && description.Length > MaxDescriptionLength)
                throw new ValidationException($"Description cannot exceed {MaxDescriptionLength} characters.");
        }

        /// <summary>
        /// Validates that start date precedes or equals end date.
        /// </summary>
        /// <param name="start">Proposed start date.</param>
        /// <param name="end">Proposed end date.</param>
        /// <exception cref="ValidationException">
        /// Thrown if start date is after end date.
        /// </exception>
        private static void ValidateDateRange(DateTime start, DateTime end)
        {
            if (start > end)
                throw new ValidationException("Start date must be earlier than or equal to end date.");
        }

        /// <summary>
        /// Validates that amount is non-negative.
        /// </summary>
        /// <param name="amount">Amount to validate.</param>
        /// <exception cref="ValidationException">
        /// Thrown if amount is negative.
        /// </exception>
        private static void ValidateAmount(decimal amount)
        {
            if (amount < 0)
                throw new ValidationException("Total amount must be non-negative.");
        }

        /// <summary>
        /// Validates currency is not null.
        /// </summary>
        /// <param name="currency">Currency instance to validate.</param>
        /// <exception cref="ValidationException">
        /// Thrown if currency is null.
        /// </exception>
        private static void ValidateCurrency(Currency? currency)
        {
            if (currency == null)
                throw new ValidationException("Currency must be provided.");
        }
    }
}