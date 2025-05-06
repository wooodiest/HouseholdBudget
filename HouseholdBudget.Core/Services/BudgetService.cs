using HouseholdBudget.Core.Core;
using HouseholdBudget.Core.Data;
using HouseholdBudget.Core.Models;

namespace HouseholdBudget.Core.Services
{
    public class BudgetService : IBudgetService
    {
        private readonly IUserContext         _userContext;
        private readonly IDatabaseManager     _database;
        private readonly ITransactionService  _transactionService;
        private readonly IExchangeRateService _exchangeRateService;

        private readonly List<Budget> _budgets;
        private readonly List<BudgetPeriod> _periods;

        public BudgetService(IUserContext userContext, IDatabaseManager database, ITransactionService transactionService,
            IExchangeRateService exchangeRateService)
        {
            _userContext         = userContext;
            _database            = database;
            _transactionService  = transactionService;
            _exchangeRateService = exchangeRateService;

            _budgets = _database.LoadBudgetsForUser(_userContext.CurrentUser.Id);
            _periods = _database.LoadBudgetPeriodsForUser(_userContext.CurrentUser.Id);
        }

        public Budget CreateOrUpdateBudget(Guid budgetPeriodId, Guid categoryId, string name, decimal limit, Currency currency)
        {
            var budget = _budgets.FirstOrDefault(b =>
                b.BudgetPeriodId == budgetPeriodId &&
                b.CategoryId     == categoryId &&
                b.UserId         == _userContext.CurrentUser.Id);

            if (budget == null)
            {
                budget = new Budget {
                    Id             = Guid.NewGuid(),
                    CreatedAt      = DateTime.UtcNow,
                    UserId         = _userContext.CurrentUser.Id,
                    BudgetPeriodId = budgetPeriodId,
                    CategoryId     = categoryId,
                    Name           = name,
                    Limit          = limit,
                    Currency       = currency
                };
                _budgets.Add(budget);
            }
            else
            {
                budget.Name     = name;
                budget.Limit    = limit;
                budget.Currency = currency;
            }

            _database.SaveBudget(budget);
            return budget;
        }

        public void RemoveBudget(Guid budgetId)
        {
            var budget = _budgets.FirstOrDefault(b => 
                b.Id     == budgetId &&
                b.UserId == _userContext.CurrentUser.Id);

            if (budget != null)
            {
                _budgets.Remove(budget);
                _database.DeleteBudget(budgetId);
            }
        }

        public List<Budget> GetBudgetsForPeriod(Guid budgetPeriodId)
        {
            return _budgets.Where(b => 
                b.BudgetPeriodId == budgetPeriodId &&
                b.UserId == _userContext.CurrentUser.Id).ToList();
        }

        public BudgetPeriod CreateOrUpdateBudgetPeriod(string name, DateTime startDate, DateTime endDate, Currency baseCurrency, string notes = "")
        {
            var period = _periods.FirstOrDefault(p =>
                p.UserId    == _userContext.CurrentUser.Id &&
                p.StartDate == startDate &&
                p.EndDate   == endDate);

            if (period == null)
            {
                period = new BudgetPeriod {
                    Id           = Guid.NewGuid(),
                    UserId       = _userContext.CurrentUser.Id,
                    Name         = name,
                    StartDate    = startDate,
                    EndDate      = endDate,
                    BaseCurrency = baseCurrency,
                    Notes        = notes
                };
                _periods.Add(period);
            }
            else
            {
                period.Name         = name;
                period.BaseCurrency = baseCurrency;
                period.Notes        = notes;
            }

            _database.SaveBudgetPeriod(period);
            return period;
        }

        public List<BudgetPeriod> GetBudgetPeriods()
        {
            return _periods.Where(p => p.UserId == _userContext.CurrentUser.Id).ToList();
        }

        public void RemoveBudgetPeriod(Guid id)
        {
            var period = _periods.FirstOrDefault(p => 
                p.Id == id && 
                p.UserId == _userContext.CurrentUser.Id);
            if (period != null)
            {
                _periods.Remove(period);
                _database.DeleteBudgetPeriod(id);
            }
        }

        public async Task UpdateBudgetsUsageAsync()
        {
            foreach (var budget in _budgets)
            {
                var period = _periods.FirstOrDefault(p => p.Id == budget.BudgetPeriodId);
                if (period == null || period.StartDate == null || period.EndDate == null)
                    continue;

                var transactions = _transactionService.GetAll()
                    .Where(t =>
                        t.UserId     == budget.UserId &&
                        t.CategoryId == budget.CategoryId &&
                        t.Date       >= period.StartDate.Value &&
                        t.Date       <= period.EndDate.Value);

                decimal used = 0m;
                foreach (var tx in transactions)
                {
                    var converted = await _exchangeRateService.ConvertAsync(tx.Amount, tx.Currency, budget.Currency);
                    used += converted;
                }

                budget.Used = used;
                _database.SaveBudget(budget);
            }
        }

        public async Task<(decimal totalLimit, decimal totalUsed)> GetTotalsForPeriodAsync(BudgetPeriod period)
        {
            decimal totalLimit = 0m;
            decimal totalUsed  = 0m;

            var budgets = _budgets.Where(b => 
                b.BudgetPeriodId == period.Id &&
                b.UserId         == _userContext.CurrentUser.Id);

            foreach (var budget in budgets)
            {
                var convertedLimit = await _exchangeRateService.ConvertAsync(budget.Limit, budget.Currency, period.BaseCurrency);
                var convertedUsed  = await _exchangeRateService.ConvertAsync(budget.Used,  budget.Currency, period.BaseCurrency);

                totalLimit += convertedLimit;
                totalUsed  += convertedUsed;
            }

            return (totalLimit, totalUsed);
        }
    }
}
