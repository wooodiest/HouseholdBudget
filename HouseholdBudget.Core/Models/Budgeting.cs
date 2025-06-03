namespace HouseholdBudget.Core.Models
{
    /// <summary>
    /// Represents aggregated totals for income and expenses, expressed in a specific currency.
    /// </summary>
    /// <param name="TotalIncome">Sum of all income transactions.</param>
    /// <param name="TotalExpenses">Sum of all expense transactions.</param>
    /// <param name="CurrencyCode">Currency in which the totals are expressed.</param>
    public record BudgetTotals(decimal TotalIncome, decimal TotalExpenses, string CurrencyCode);

    /// <summary>
    /// Represents the breakdown of budget data grouped by a specific category.
    /// </summary>
    /// <param name="CategoryId">The unique identifier of the category.</param>
    /// <param name="TotalIncome">Total income for this category within the filtered period.</param>
    /// <param name="TotalExpenses">Total expenses for this category within the filtered period.</param>
    /// <param name="CurrencyCode">The currency of the aggregated amount.</param>
    public record CategoryBudgetBreakdown(Guid CategoryId, decimal TotalIncome, decimal TotalExpenses, string CurrencyCode);

    /// <summary>
    /// Represents the daily total income and expenses used to plot budget trends.
    /// </summary>
    /// <param name="Date">Date of the record (in UTC).</param>
    /// <param name="TotalIncome">Total income for the day.</param>
    /// <param name="TotalExpenses">Total expenses for the day.</param>
    /// <param name="CurrencyCode">Currency used for all amounts.</param>
    public record DailyBudgetPoint(DateTime Date, decimal TotalIncome, decimal TotalExpenses, string CurrencyCode);

    /// <summary>
    /// Represents a summary of financial activity for a specific calendar month.
    /// Includes income/expense totals, category breakdowns, and trend data.
    /// </summary>
    public class MonthlyBudgetSummary
    {
        /// <summary>
        /// The calendar year (e.g., 2025).
        /// </summary>
        public int Year { get; init; }

        /// <summary>
        /// The calendar month (1 = January, 12 = December).
        /// </summary>
        public int Month { get; init; }

        /// <summary>
        /// Total income for the month.
        /// </summary>
        public decimal TotalIncome { get; init; }

        /// <summary>
        /// Total expenses for the month.
        /// </summary>
        public decimal TotalExpenses { get; init; }

        /// <summary>
        /// Currency in which the financial summary is expressed.
        /// </summary>
        public required string CurrencyCode { get; init; }

        /// <summary>
        /// Breakdown of totals by category (e.g., Groceries, Rent).
        /// </summary>
        public IReadOnlyList<CategoryBudgetBreakdown> Categories { get; init; } = [];

        /// <summary>
        /// Aggregated daily records for trend visualization.
        /// </summary>
        public IReadOnlyList<DailyBudgetPoint> DailyTrend { get; init; } = [];
    }
}
