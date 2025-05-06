using HouseholdBudget.Core.Models;

namespace HouseholdBudget.Core.Services
{
    public interface IBudgetService
    {
        Budget CreateOrUpdateBudget(Guid budgetPeriodId, Guid categoryId, string name, decimal limit, Currency currency);
        void RemoveBudget(Guid budgetId);

        BudgetPeriod CreateOrUpdateBudgetPeriod(string name, DateTime startDate, DateTime endDate, Currency baseCurrency, string notes = "");
        List<BudgetPeriod> GetBudgetPeriods();
        void RemoveBudgetPeriod(Guid id);

        Task UpdateBudgetsUsageAsync();
        Task<(decimal totalLimit, decimal totalUsed)> GetTotalsForPeriodAsync(BudgetPeriod period);
    }
}
