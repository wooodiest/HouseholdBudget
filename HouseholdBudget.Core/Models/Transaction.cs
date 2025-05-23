using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace HouseholdBudget.Core.Models
{
    /// <summary>
    /// Defines the type of a transaction.
    /// </summary>
    public enum TransactionType
    {
        /// <summary>
        /// Represents a spending action.
        /// </summary>
        Expense,

        /// <summary>
        /// Represents an income or earning.
        /// </summary>
        Income
    }

    /// <summary>
    /// Represents a single financial transaction within a household budget, 
    /// including its amount, type, currency, and associated metadata.
    /// </summary>
    [DebuggerDisplay("{ToString(),nq}")]
    public class Transaction : AuditableEntity
    {
        /// <summary>
        /// Maximum allowed length for the transaction description.
        /// </summary>
        public const int MaxDescriptionLength = 250;

        /// <summary>
        /// Unique identifier of the user who created the transaction.
        /// </summary>
        public Guid UserId { get; private set; } = Guid.Empty;

        /// <summary>
        /// Identifier of the category to which the transaction belongs.
        /// </summary>
        public Guid CategoryId { get; private set; } = Guid.Empty;

        /// <summary>
        /// The UTC date and time the transaction occurred.
        /// </summary>
        public DateTime Date { get; private set; } = DateTime.UtcNow;

        /// <summary>
        /// Optional textual description providing context or details about the transaction.
        /// </summary>
        [MaxLength(MaxDescriptionLength)]
        public string Description { get; private set; } = string.Empty;

        /// <summary>
        /// A list of optional tags associated with the transaction.
        /// </summary>
        public List<string> Tags { get; private set; } = [];

        /// <summary>
        /// The amount of money involved in the transaction. Must be a positive value.
        /// </summary>
        [Required]
        [Range(0.000001, double.MaxValue)]
        public decimal Amount { get; private set; } = 0.0m;

        /// <summary>
        /// The currency in which the transaction was made.
        /// </summary>
        [Required]
        public Currency Currency { get; private set; } = null!;

        /// <summary>
        /// Defines whether the transaction is an income or expense.
        /// </summary>
        public TransactionType Type { get; set; } = TransactionType.Expense;

        /// <summary>
        /// Private constructor required for ORM and factory usage.
        /// </summary>
        private Transaction() { }

        /// <summary>
        /// Creates a new transaction instance after validating the input data.
        /// </summary>
        /// <param name="userId">The ID of the user associated with this transaction.</param>
        /// <param name="categoryId">The category ID under which this transaction falls.</param>
        /// <param name="amount">The monetary amount of the transaction.</param>
        /// <param name="currency">The currency used for the transaction.</param>
        /// <param name="type">Indicates if the transaction is income or expense.</param>
        /// <param name="description">Optional text describing the transaction.</param>
        /// <param name="tags">Optional list of tags for classification.</param>
        /// <param name="date">The date and time the transaction occurred (UTC). Defaults to now if null.</param>
        /// <returns>A new <see cref="Transaction"/> instance.</returns>
        /// <exception cref="ValidationException">Thrown when validation fails.</exception>
        public static Transaction Create(
            Guid                 userId,
            Guid                 categoryId,
            decimal              amount,
            Currency             currency,
            TransactionType      type        = TransactionType.Expense,
            string?              description = null,
            IEnumerable<string>? tags        = null,
            DateTime?            date        = null)
        {
            EnsureIsValid(userId, categoryId, amount, currency, description, tags);

            return new Transaction {
                UserId      = userId,
                CategoryId  = categoryId,
                Amount      = amount,
                Currency    = currency,
                Type        = type,
                Description = description ?? string.Empty,
                Tags        = tags?.ToList() ?? new List<string>(),
                Date        = date ?? DateTime.UtcNow
            };
        }

        /// <summary>
        /// Updates the description of the transaction with validation.
        /// </summary>
        /// <param name="newDescription">The new description text.</param>
        /// <exception cref="ValidationException">Thrown if the new description is too long.</exception>
        public void UpdateDescription(string newDescription)
        {
            var errors = ValidateDescription(newDescription).ToList();
            if (errors.Count > 0)
                throw new ValidationException(string.Join("; ", errors));

            Description = newDescription;
            MarkAsUpdated();
        }

        /// <summary>
        /// Updates the transaction tags after validating their structure and uniqueness.
        /// </summary>
        /// <param name="newTags">The list of new tags to assign.</param>
        /// <exception cref="ValidationException">Thrown if any tag is invalid or duplicated.</exception>
        public void UpdateTags(IEnumerable<string>? newTags)
        {
            var errors = ValidateTags(newTags).ToList();
            if (errors.Count > 0)
                throw new ValidationException(string.Join("; ", errors));

            Tags = newTags?.ToList() ?? new List<string>();
            MarkAsUpdated();
        }

        /// <summary>
        /// Updates the transaction amount after validating the new value.
        /// </summary>
        /// <param name="newAmount">The new amount value to set.</param>
        /// <exception cref="ValidationException">Thrown if the new amount is invalid.</exception>
        public void UpdateAmount(decimal newAmount)
        {
            var errors = ValidateAmount(newAmount).ToList();
            if (errors.Count > 0)
                throw new ValidationException(string.Join("; ", errors));

            Amount = newAmount;
            MarkAsUpdated();
        }

        /// <summary>
        /// Changes the currency of the transaction after validating the new value.
        /// </summary>
        /// <param name="newCurrency">The new currency to assign.</param>
        /// <exception cref="ValidationException">Thrown if the new currency is null.</exception>
        public void ChangeCurrency(Currency newCurrency)
        {
            var errors = ValidateCurrency(newCurrency).ToList();
            if (errors.Count > 0)
                throw new ValidationException(string.Join("; ", errors));

            Currency = newCurrency;
            MarkAsUpdated();
        }


        /// <summary>
        /// Changes the category of the transaction.
        /// </summary>
        /// <param name="newCategoryId">The new category ID.</param>
        /// <exception cref="ValidationException">Thrown if the new ID is invalid.</exception>
        public void ChangeCategory(Guid newCategoryId)
        {
            var errors = ValidateCategoryId(newCategoryId).ToList();
            if (errors.Count > 0)
                throw new ValidationException(string.Join("; ", errors));

            CategoryId = newCategoryId;
            MarkAsUpdated();
        }

        /// <summary>
        /// Updates the date of the transaction.
        /// </summary>
        /// <param name="newDate">The new UTC date value to assign.</param>
        public void UpdateDate(DateTime newDate)
        {
            Date = newDate;
            MarkAsUpdated();
        }

        /// <summary>
        /// Updates the transaction type (e.g., from Expense to Income).
        /// </summary>
        /// <param name="newType">The new transaction type to assign.</param>
        public void UpdateType(TransactionType newType)
        {
            if (Type != newType)
            {
                Type = newType;
                MarkAsUpdated();
            }
        }

        /// <summary>
        /// Validates the user ID.
        /// </summary>
        /// <param name="userId">The user ID to validate.</param>
        /// <returns>Validation error message if invalid; otherwise, empty.</returns>
        private static IEnumerable<string> ValidateUserId(Guid userId)
        {
            if (userId == Guid.Empty)
                yield return "UserId is required.";
        }

        /// <summary>
        /// Validates the category ID.
        /// </summary>
        /// <param name="categoryId">The category ID to validate.</param>
        /// <returns>Validation error message if invalid; otherwise, empty.</returns>
        private static IEnumerable<string> ValidateCategoryId(Guid categoryId)
        {
            if (categoryId == Guid.Empty)
                yield return "CategoryId is required.";
        }

        /// <summary>
        /// Validates the transaction amount.
        /// </summary>
        /// <param name="amount">The transaction amount to validate.</param>
        /// <returns>Validation error message if invalid; otherwise, empty.</returns>
        private static IEnumerable<string> ValidateAmount(decimal amount)
        {
            if (amount <= 0)
                yield return "Amount must be greater than zero.";
        }

        /// <summary>
        /// Validates the currency object.
        /// </summary>
        /// <param name="currency">The currency instance to validate.</param>
        /// <returns>Validation error message if null; otherwise, empty.</returns>
        private static IEnumerable<string> ValidateCurrency(Currency? currency)
        {
            if (currency == null)
                yield return "Currency is required.";
        }

        /// <summary>
        /// Validates the transaction description text.
        /// </summary>
        /// <param name="description">The optional description to validate.</param>
        /// <returns>Validation error message if too long; otherwise, empty.</returns>
        private static IEnumerable<string> ValidateDescription(string? description)
        {
            if (!string.IsNullOrWhiteSpace(description) && description.Length > MaxDescriptionLength)
                yield return $"Description cannot exceed {MaxDescriptionLength} characters.";
        }

        /// <summary>
        /// Validates a collection of transaction tags.
        /// </summary>
        /// <param name="tags">The list of tags to validate.</param>
        /// <returns>Validation error messages, if any.</returns>
        private static IEnumerable<string> ValidateTags(IEnumerable<string>? tags)
        {
            if (tags == null) yield break;

            var tagList = tags.ToList();

            if (tagList.Any(string.IsNullOrWhiteSpace))
                yield return "Tags cannot contain empty or whitespace values.";

            if (tagList.Distinct(StringComparer.OrdinalIgnoreCase).Count() != tagList.Count)
                yield return "Tags must be unique (case-insensitive).";
        }


        /// <summary>
        /// Validates the provided transaction fields and returns a list of validation errors.
        /// </summary>
        /// <param name="userId">User ID to validate.</param>
        /// <param name="categoryId">Category ID to validate.</param>
        /// <param name="amount">Transaction amount to validate.</param>
        /// <param name="currency">Currency to validate.</param>
        /// <param name="description">Optional description to validate.</param>
        /// <param name="tags">Optional tags to validate.</param>
        /// <returns>A list of error messages. Empty if all fields are valid.</returns>
        public static IReadOnlyList<string> Validate(
            Guid                 userId, 
            Guid                 categoryId,
            decimal              amount,
            Currency?            currency,
            string?              description = null,
            IEnumerable<string>? tags        = null )
        {
            return  ValidateUserId(userId)
                .Concat(ValidateCategoryId(categoryId))
                .Concat(ValidateAmount(amount))
                .Concat(ValidateCurrency(currency))
                .Concat(ValidateDescription(description))
                .Concat(ValidateTags(tags))
                .ToList();
        }

        /// <summary>
        /// Throws a <see cref="ValidationException"/> if the provided data is invalid.
        /// </summary>
        /// <param name="userId">The user ID to validate.</param>
        /// <param name="categoryId">The category ID to validate.</param>
        /// <param name="amount">The transaction amount to validate.</param>
        /// <param name="currency">The currency to validate.</param>
        /// <param name="description">Optional description to validate.</param>
        /// <param name="tags">Optional tags to validate.</param>
        private static void EnsureIsValid(
            Guid                 userId,
            Guid                 categoryId,
            decimal              amount,
            Currency?            currency,
            string?              description,
            IEnumerable<string>? tags)
        {
            var errors = Validate(userId, categoryId, amount, currency, description, tags);
            if (errors.Count > 0)
                throw new ValidationException(string.Join("; ", errors));
        }

        public override string ToString()
        {
            var created = $"Created: {CreatedAt:u}";
            var updated = UpdatedAt.HasValue ? $" | Updated: {UpdatedAt:u}" : "";

            var amountFormatted = $"{Amount:F2} {Currency?.Code ?? "???"}";
            var descriptionPart = string.IsNullOrWhiteSpace(Description) ? "" : $" | \"{Description}\"";

            return $"{Type}: {amountFormatted} on {Date:u}{descriptionPart} | Id: {Id} | {created}{updated}";
        }

    }

}
