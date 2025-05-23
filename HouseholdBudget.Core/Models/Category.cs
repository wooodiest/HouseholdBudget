using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace HouseholdBudget.Core.Models
{
    /// <summary>
    /// Defines the type of a financial category.
    /// </summary>
    public enum CategoryType
    {
        /// <summary>
        /// Represents a source of income.
        /// </summary>
        Income,

        /// <summary>
        /// Represents an expense or cost.
        /// </summary>
        Expense
    }

    /// <summary>
    /// Represents a financial category used to group transactions for a specific user.
    /// </summary>
    [DebuggerDisplay("{ToString(),nq}")]
    public class Category : AuditableEntity
    {
        /// <summary>
        /// The maximum allowed length for a category name.
        /// </summary>
        public const int MaxNameLength = 100;

        /// <summary>
        /// The identifier of the user who owns this category.
        /// </summary>
        public Guid UserId { get; private set; } = Guid.Empty;

        /// <summary>
        /// The name of the category.
        /// </summary>
        [Required, MaxLength(MaxNameLength)]
        public string Name { get; private set; } = string.Empty;

        /// <summary>
        /// Indicates whether the category is for income or expenses.
        /// </summary>
        public CategoryType Type { get; private set; } = CategoryType.Expense;

        /// <summary>
        /// Private constructor required for ORM and factory usage.
        /// </summary>
        private Category() { }

        /// <summary>
        /// Creates a new instance of <see cref="Category"/> with validation.
        /// </summary>
        /// <param name="userId">The ID of the user who owns the category.</param>
        /// <param name="name">The name of the category.</param>
        /// <param name="type">The type of the category (Income or Expense).</param>
        /// <returns>A new valid <see cref="Category"/> instance.</returns>
        /// <exception cref="ValidationException">Thrown when the name is invalid.</exception>
        public static Category Create(Guid userId, string name, CategoryType type = CategoryType.Expense)
        {
            EnsureNameIsValid(name);

            return new Category {
                UserId = userId,
                Name   = name,
                Type   = type
            };
        }

        /// <summary>
        /// Changes the name of the category with validation.
        /// </summary>
        /// <param name="newName">The new name to assign to the category.</param>
        /// <exception cref="ValidationException">Thrown when the new name is invalid.</exception>
        public void Rename(string newName)
        {
            EnsureNameIsValid(newName);
            Name = newName;

            MarkAsUpdated();
        }

        /// <summary>
        /// Changes the type of the category (e.g., from Expense to Income).
        /// </summary>
        /// <param name="newType">The new category type to assign.</param>
        public void ChangeType(CategoryType newType)
        {
            if (Type != newType)
            {
                Type = newType;
                MarkAsUpdated();
            }
        }

        /// <summary>
        /// Validates the given category name and returns a list of error messages if any.
        /// </summary>
        /// <param name="name">The category name to validate.</param>
        /// <returns>A list of validation error messages. Empty if the name is valid.</returns>
        public static IReadOnlyList<string> ValidateName(string name)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(name))
                errors.Add("Category name is required.");
            if (name.Length > MaxNameLength)
                errors.Add($"Category name cannot exceed {MaxNameLength} characters.");

            return errors;
        }

        /// <summary>
        /// Validates the given category name and throws a <see cref="ValidationException"/> if invalid.
        /// </summary>
        /// <param name="name">The category name to validate.</param>
        /// <exception cref="ValidationException">Thrown when the name is invalid.</exception>
        private static void EnsureNameIsValid(string name)
        {
            var errors = ValidateName(name);
            if (errors.Count > 0)
                throw new ValidationException(string.Join("; ", errors));
        }

        public override string ToString()
        {
            var created = $"Created: {CreatedAt:u}";
            var updated = UpdatedAt.HasValue ? $" | Updated: {UpdatedAt:u}" : "";

            return $"{Type}: {Name ?? "???"} | UserId: {UserId} | Id: {Id} | {created}{updated}";
        }

    }
}
