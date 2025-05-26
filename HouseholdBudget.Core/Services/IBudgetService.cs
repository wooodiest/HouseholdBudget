using HouseholdBudget.Core.Models;

namespace HouseholdBudget.Core.Services;

/// <summary>
/// Provides high-level operations for analyzing, aggregating, and reporting
/// budgeting information for the currently authenticated user.
/// This service abstracts monthly, yearly, and category-level budget summaries
/// and offers insights into spending and earnings patterns.
/// </summary>
public interface IBudgetService
{
    /// <summary>
    /// Retrieves a detailed summary of the user's budget for the specified month.
    /// </summary>
    /// <param name="year">The year of interest (e.g., 2025).</param>
    /// <param name="month">The month of interest (1-12).</param>
    /// <returns>A structured <see cref="MonthlyBudgetSummary"/> with aggregated data.</returns>
    Task<MonthlyBudgetSummary> GetMonthlySummaryAsync(int year, int month);

    /// <summary>
    /// Retrieves the user's total income and expenses across all time or a filtered range.
    /// </summary>
    /// <param name="start">Optional start date for the analysis.</param>
    /// <param name="end">Optional end date for the analysis.</param>
    /// <returns>A <see cref="BudgetTotals"/> object with total amounts per type.</returns>
    Task<BudgetTotals> GetTotalsAsync(DateTime? start = null, DateTime? end = null);

    /// <summary>
    /// Groups and summarizes all transactions by category for a given time range.
    /// </summary>
    /// <param name="start">Start date for the summary (inclusive).</param>
    /// <param name="end">End date for the summary (inclusive).</param>
    /// <returns>A list of <see cref="CategoryBudgetBreakdown"/> entries.</returns>
    Task<IReadOnlyList<CategoryBudgetBreakdown>> GetCategoryBreakdownAsync(DateTime start, DateTime end);

    /// <summary>
    /// Retrieves a chronological history of daily income and expenses for the given date range.
    /// Can be used to generate charts or trends.
    /// </summary>
    /// <param name="start">Start date (inclusive).</param>
    /// <param name="end">End date (inclusive).</param>
    /// <returns>List of <see cref="DailyBudgetPoint"/> representing daily aggregates.</returns>
    Task<IReadOnlyList<DailyBudgetPoint>> GetDailyTrendAsync(DateTime start, DateTime end);
}
