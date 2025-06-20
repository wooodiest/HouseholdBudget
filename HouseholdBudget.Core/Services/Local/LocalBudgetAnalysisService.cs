﻿using HouseholdBudget.Core.Data;
using HouseholdBudget.Core.Models;
using HouseholdBudget.Core.Services.Interfaces;
using HouseholdBudget.Core.UserData;

namespace HouseholdBudget.Core.Services.Local
{
    /// <summary>
    /// Local implementation of the <see cref="IBudgetAnalysisService"/> that analyzes financial transactions
    /// associated with the currently authenticated user. This service provides aggregate budget totals,
    /// category breakdowns, and daily trend data using the user's default currency.
    /// </summary>
    public class LocalBudgetAnalysisService : IBudgetAnalysisService
    {
        private readonly ITransactionService   _transactionService;
        private readonly IUserSessionService   _userSession;
        private readonly IExchangeRateService  _exchangeRateService;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalBudgetAnalysisService"/> class.
        /// </summary>
        /// <param name="transactionService">Provides access to user transactions.</param>
        /// <param name="userSession">Provides access to the current user and their preferences.</param>
        /// <param name="exchangeRateService">Service for converting amounts between currencies.</param>
        /// <param name="exchangeRateProvider">Provider for resolving supported currencies.</param>
        public LocalBudgetAnalysisService(
            ITransactionService transactionService,
            IUserSessionService userSession,
            IExchangeRateService exchangeRateService,
            IExchangeRateProvider exchangeRateProvider)
        {
            _transactionService   = transactionService;
            _userSession          = userSession;
            _exchangeRateService  = exchangeRateService;
        }

        /// <inheritdoc />
        public async Task<BudgetTotals> GetTotalsAsync(DateTime? start = null, DateTime? end = null)
        {
            var user = EnsureAuthenticated();

            var filter       = new TransactionFilter { StartDate = start, EndDate = end };
            var transactions = await _transactionService.GetAsync(filter);

            decimal income = 0, expenses = 0;

            foreach (var transaction in transactions)
            {
                var converted = await _exchangeRateService.ConvertAsync(transaction.Amount, transaction.CurrencyCode, user.DefaultCurrencyCode);
                if (transaction.Type == TransactionType.Income)
                    income += converted;
                else if (transaction.Type == TransactionType.Expense)
                    expenses += converted;
            }

            return new BudgetTotals(income, expenses, user.DefaultCurrencyCode);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<CategoryBudgetBreakdown>> GetCategoryBreakdownAsync(DateTime start, DateTime end)
        {
            var user = EnsureAuthenticated();

            var filter       = new TransactionFilter { StartDate = start, EndDate = end };
            var transactions = await _transactionService.GetAsync(filter);

            var grouped = new Dictionary<Guid, CategoryBudgetBreakdown>();
            foreach (var transaction in transactions)
            {
                var converted  = await _exchangeRateService.ConvertAsync(transaction.Amount, transaction.CurrencyCode, user.DefaultCurrencyCode);
                var categoryId = transaction.CategoryId;

                if (!grouped.ContainsKey(categoryId))
                    grouped[categoryId] = new CategoryBudgetBreakdown(categoryId, 0, 0, user.DefaultCurrencyCode);

                var current = grouped[categoryId];

                if (transaction.Type == TransactionType.Income)
                {
                    grouped[categoryId] = current with { TotalIncome = current.TotalIncome + converted };
                }
                else if (transaction.Type == TransactionType.Expense)
                {
                    grouped[categoryId] = current with { TotalExpenses = current.TotalExpenses + converted };
                }
            }

            return grouped.Values.ToList();
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<DailyBudgetPoint>> GetDailyTrendAsync(DateTime start, DateTime end)
        {
            var user = EnsureAuthenticated();

            var filter = new TransactionFilter { StartDate = start, EndDate = end };
            var transactions = await _transactionService.GetAsync(filter);

            var transactionsByDate = transactions
                .GroupBy(t => t.Date.Date)
                .ToDictionary(g => g.Key, g => g.ToList()); 

            var trend = new List<DailyBudgetPoint>();
            for (var date = start.Date; date <= end.Date; date = date.AddDays(1))
            {
                decimal income = 0, expenses = 0;

                if (transactionsByDate.TryGetValue(date, out var dailyTransactions))
                {
                    foreach (var transaction in dailyTransactions)
                    {
                        var converted = await _exchangeRateService.ConvertAsync(transaction.Amount, transaction.CurrencyCode, user.DefaultCurrencyCode);
                        if (transaction.Type == TransactionType.Income)
                            income += converted;
                        else
                            expenses += converted;
                    }
                }

                trend.Add(new DailyBudgetPoint(date, income, expenses, user.DefaultCurrencyCode));
            }

            return trend;
        }


        /// <inheritdoc />
        public async Task<MonthlyBudgetSummary> GetMonthlySummaryAsync(int year, int month)
        {
            var user = EnsureAuthenticated();

            var start = new DateTime(year, month, 1);
            var end   = start.AddMonths(1).AddDays(-1);

            var totals     = await GetTotalsAsync(start, end);
            var categories = await GetCategoryBreakdownAsync(start, end);
            var trend      = await GetDailyTrendAsync(start, end);

            return new MonthlyBudgetSummary {
                Year          = year,
                Month         = month,
                TotalIncome   = totals.TotalIncome,
                TotalExpenses = totals.TotalExpenses,
                CurrencyCode  = totals.CurrencyCode,
                Categories    = categories,
                DailyTrend    = trend
            };
        }

        /// <summary>
        /// Ensures that a user is currently authenticated.
        /// </summary>
        /// <returns>The authenticated user.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the user is not authenticated.</exception>
        private User EnsureAuthenticated() =>
            _userSession.GetUser() ?? throw new InvalidOperationException("User is not authenticated.");
    }
}
