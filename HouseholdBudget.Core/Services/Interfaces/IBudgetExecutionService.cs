using HouseholdBudget.Core.Models;
using HouseholdBudget.Core.Services.Interfaces;
using HouseholdBudget.Core.UserData;

namespace HouseholdBudget.Core.Services.Interfaces
{
    /// <summary>
    /// Defines operations for managing budget execution tracking - the process of comparing
    /// planned budgets against actual financial activity. This service maintains the integrity
    /// of budget execution data by synchronizing with transaction systems and currency services.
    /// </summary>
    public interface IBudgetExecutionService
    {
        /// <summary>
        /// Applies a single transaction's financial impact to all relevant budget plans.
        /// Efficiently updates execution status without full recalculation.
        /// </summary>
        /// <param name="transaction">The transaction to process. Must represent finalized financial activity.</param>
        /// <remarks>
        /// This method is optimized for real-time updates during transaction processing.
        /// For historical data reconciliation, use <see cref="RefreshExecutionForAllPlansAsync"/>.
        /// </remarks>
        Task ApplyTransactionAsync(Transaction transaction);

        /// <summary>
        /// Recalculates execution status for a specific budget plan by processing all
        /// relevant transactions. Ensures accuracy after plan modifications.
        /// </summary>
        /// <param name="budgetId">The unique identifier of the budget plan to refresh.</param>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when no budget plan exists with the specified ID.
        /// </exception>
        Task RefreshExecutionForPlanAsync(Guid budgetId);

        /// <summary>
        /// Performs comprehensive recalculation of execution status for all in-memory budget plans.
        /// Resynchronizes with the complete transaction history.
        /// </summary>
        /// <remarks>
        /// Use during system initialization, after bulk data imports, or when currency
        /// conversion rates have significantly changed.
        /// </remarks>
        Task RefreshExecutionForAllPlansAsync();
    }
}