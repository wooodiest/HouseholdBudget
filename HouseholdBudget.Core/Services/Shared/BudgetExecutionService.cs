using HouseholdBudget.Core.Services.Interfaces;
using HouseholdBudget.Core.UserData;
using HouseholdBudget.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace HouseholdBudget.Core.Services.Shared
{
    /// <summary>
    /// Local implementation of <see cref="IBudgetExecutionService"/> that calculates budget execution
    /// metrics using in-memory data. Handles currency conversion and maintains transactional integrity
    /// between budget plans and financial activity.
    /// </summary>
    /// <remarks>
    /// Implements the following key functionality:
    /// <list type="bullet">
    /// <item>Real-time transaction application to affected budget plans</item>
    /// <item>Full plan recalculation with currency normalization</item>
    /// <item>Automatic date-range filtering of financial activity</item>
    /// </list>
    /// </remarks>
    public class LocalBudgetExecutionService : IBudgetExecutionService
    {
        private readonly IBudgetPlanService _planService;
        private readonly IExchangeRateService _exchangeRateService;
        private readonly IExchangeRateProvider _exchangeRateProvider;
        private readonly ITransactionService _transactionService;
        private readonly IUserSessionService _userSession;

        /// <summary>
        /// Initializes a new instance of the budget execution service with required dependencies.
        /// </summary>
        /// <param name="planService">Service for accessing budget plan data</param>
        /// <param name="exchangeRateService">Service for currency conversion operations</param>
        /// <param name="transactionService">Service for accessing financial transactions</param>
        /// <param name="userSession">Provides context about the authenticated user</param>
        public LocalBudgetExecutionService(
            IBudgetPlanService planService,
            IExchangeRateService exchangeRateService,
            ITransactionService transactionService,
            IUserSessionService userSession,
            IExchangeRateProvider exchangeRateProvider)
        {
            _planService         = planService;
            _exchangeRateService = exchangeRateService;
            _transactionService  = transactionService;
            _userSession         = userSession;
            _exchangeRateProvider = exchangeRateProvider;
        }

        /// <inheritdoc/>
        public async Task ApplyTransactionAsync(Transaction transaction)
        {
            if (transaction.Amount <= 0 || transaction.CurrencyCode == null)
                throw new ValidationException("Invalid transaction data");

            var plans = await _planService.GetAllPlansAsync();
            foreach (var plan in plans)
            {
                if (!plan.IncludesDate(transaction.Date))
                    continue;

                var categoryPlan = plan.CategoryPlans
                    .FirstOrDefault(cp => cp.CategoryId == transaction.CategoryId);

                if (categoryPlan != null)
                {
                    var convertedAmount = await _exchangeRateService.ConvertAsync(
                        transaction.Amount,
                        transaction.CurrencyCode,
                        categoryPlan.CurrencyCode
                    );
                    categoryPlan.AddExecution(transaction.Type == TransactionType.Income ? convertedAmount : 0.0m,
                        transaction.Type == TransactionType.Expense ? convertedAmount : 0.0m);
                }
            }
        }

        /// <inheritdoc/>
        public async Task RefreshExecutionForAllPlansAsync()
        {
            var plans = await _planService.GetAllPlansAsync();
            await RefreshExecutionForPlansAsync(plans);
        }

        /// <inheritdoc/>
        public async Task RefreshExecutionForPlanAsync(Guid budgetId)
        {
            var plan = await _planService.GetByIdAsync(budgetId);
            if (plan == null)
                throw new KeyNotFoundException($"BudgetPlan with id {budgetId} not found.");

            await RefreshExecutionForPlansAsync(new[] { plan });
        }

        /// <summary>
        /// Core method that recalculates execution status for specified budget plans.
        /// Implements a complete refresh cycle: reset → filter → convert → apply.
        /// </summary>
        /// <param name="plans">Budget plans to refresh</param>
        private async Task RefreshExecutionForPlansAsync(IEnumerable<BudgetPlan> plans)
        {
            var allTransactions = await _transactionService.GetAsync();

            foreach (var plan in plans)
            {
                plan.ClearExecution();

                var relevantTransactions = allTransactions
                    .Where(t => plan.IncludesDate(t.Date))
                    .ToList();

                foreach (var transaction in relevantTransactions)
                {
                    var categoryPlan = plan.CategoryPlans
                        .FirstOrDefault(cp => cp.CategoryId == transaction.CategoryId);

                    if (categoryPlan != null)
                    {
                        var convertedAmount = await _exchangeRateService.ConvertAsync(
                            transaction.Amount,
                            transaction.CurrencyCode,
                            categoryPlan.CurrencyCode
                        );
                        categoryPlan.AddExecution(transaction.Type == TransactionType.Income ? convertedAmount : 0.0m,
                            transaction.Type == TransactionType.Expense ? convertedAmount : 0.0m);
                    }
                }
            }
        }
    }
}