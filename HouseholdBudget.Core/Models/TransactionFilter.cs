namespace HouseholdBudget.Core.Models
{
    /// <summary>
    /// Represents a set of optional filters for querying transactions based on various criteria.
    /// Used in conjunction with <see cref="ITransactionService.GetAsync(TransactionFilter)"/>.
    /// </summary>
    public class TransactionFilter
    {
        /// <summary>
        /// A list of category IDs to filter transactions by.
        /// </summary>
        public List<Guid>? CategoryIds { get; set; }

        /// <summary>
        /// Specific date to match transactions on.
        /// </summary>
        public DateTime? Date { get; set; }

        /// <summary>
        /// Start date of a date range filter.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// End date of a date range filter.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Keyword to search within transaction descriptions.
        /// </summary>
        public string? DescriptionKeyword { get; set; }

        /// <summary>
        /// Minimum transaction amount.
        /// </summary>
        public decimal? MinAmount { get; set; }

        /// <summary>
        /// Maximum transaction amount.
        /// </summary>
        public decimal? MaxAmount { get; set; }

        /// <summary>
        /// Specific currency to match transactions against.
        /// </summary>
        public Currency? Currency { get; set; }

        /// <summary>
        /// Optional filter by category type (e.g., Expense, Income).
        /// </summary>
        public CategoryType? CategoryType { get; set; }

        /// <summary>
        /// Optional filter by transaction type (e.g., Expense, Income).
        /// </summary>
        public TransactionType? TransactionType { get; set; }
    }
}
