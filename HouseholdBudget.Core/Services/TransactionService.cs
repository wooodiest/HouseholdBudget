using HouseholdBudget.Core.Core;
using HouseholdBudget.Core.Data;
using HouseholdBudget.Core.Helpers;
using HouseholdBudget.Core.Models;

namespace HouseholdBudget.Core.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly List<Transaction> _transactions = new();

        private readonly IDatabaseManager      _database;
        private readonly ICategoryService      _categoryService;
        private readonly IUserContext          _userContext;
        private readonly IExchangeRateProvider _exchangeRateProvider;
        private readonly IExchangeRateService  _exchangeRateService;

        public TransactionService(IUserContext userContext, IDatabaseManager database, ICategoryService categoryService,
            IExchangeRateProvider exchangeRateProvider, IExchangeRateService exchangeRateService)
        {
            _userContext          = userContext;
            _database             = database;
            _categoryService      = categoryService;
            _exchangeRateProvider = exchangeRateProvider;
            _exchangeRateService  = exchangeRateService;

            _transactions = _database.LoadTransactionsForUser(_userContext.CurrentUser.Id);   
        }
        public Transaction AddTransaction(string description, decimal amount, Currency currency, Guid categoryID, DateTime dateTime, bool isRecurring)
        {
            var transaction = new Transaction
            {
                Id          = Guid.NewGuid(),
                UserId      = _userContext.CurrentUser.Id,
                Date        = dateTime,
                Description = description,
                Amount      = amount,
                Currency    = currency,
                CategoryId  = categoryID,
                IsRecurring = isRecurring
            };

            _transactions.Add(transaction);
            _database.SaveTransaction(transaction);

            return transaction;
        }

        public void RemoveTransaction(Guid id)
        {
            var tx = _transactions.FirstOrDefault(t => t.Id == id);
            if (tx != null)
                _transactions.Remove(tx);
        }

        public void Clear() => _transactions.Clear();

        public List<Transaction> GetAll() => _transactions;

        public List<Transaction> GetByFilter(TransactionFilter filter)
        {
            IEnumerable<Transaction> query = _transactions;

            if (filter.CategoryIds != null)
                query = query.Where(t => filter.CategoryIds.Contains(t.CategoryId));

            if (filter.Date != null)
                query = query.Where(t => t.Date.Date == filter.Date.Value.Date);

            if (filter.StartDate != null)
                query = query.Where(t => t.Date.Date >= filter.StartDate.Value.Date);

            if (filter.EndDate != null)
                query = query.Where(t => t.Date.Date <= filter.EndDate.Value.Date);

            if (!string.IsNullOrWhiteSpace(filter.DescriptionKeyword))
                query = query.Where(t => t.Description != null &&
                    t.Description.Contains(filter.DescriptionKeyword, StringComparison.OrdinalIgnoreCase));

            if (filter.MinAmount != null)
                query = query.Where(t => t.Amount >= filter.MinAmount.Value);

            if (filter.MaxAmount != null)
                query = query.Where(t => t.Amount <= filter.MaxAmount.Value);

            if (filter.Currency != null)
                query = query.Where(t => t.Currency.Code == filter.Currency.Code);

            if (filter.IsRecurring != null)
                query = query.Where(t => t.IsRecurring == filter.IsRecurring.Value);

            if (filter.CategoryType != null)
                query = query.Where(t =>
                {
                    var category = _categoryService.GetById(t.CategoryId);
                    return category != null && category.Type == filter.CategoryType.Value;
                });

            return query.ToList();
        }

        public async Task<decimal> GetTotalAmountAsync(Currency? targetCurrency = null)
        {
            targetCurrency ??= await ResolveUserCurrencyAsync();

            decimal total = 0m;
            foreach (var tx in _transactions)
            {
                var converted = await _exchangeRateService.ConvertAsync(tx.Amount, tx.Currency, targetCurrency);
                total += converted;
            }

            return total;
        }

        public async Task<decimal> GetTotalExpenseAsync(Currency? targetCurrency = null)
        {
            targetCurrency ??= await ResolveUserCurrencyAsync();

            decimal total = 0m;
            var expenses = _transactions
                .Where(t => _categoryService.GetById(t.CategoryId)?.Type == CategoryType.Expense);

            foreach (var tx in expenses)
            {
                var converted = await _exchangeRateService.ConvertAsync(tx.Amount, tx.Currency, targetCurrency);
                total += converted;
            }

            return total;
        }

        public async Task<decimal> GetTotalIncomeAsync(Currency? targetCurrency = null)
        {
            targetCurrency ??= await ResolveUserCurrencyAsync();

            decimal total = 0m;
            var incomes = _transactions
                .Where(t => _categoryService.GetById(t.CategoryId)?.Type == CategoryType.Income);

            foreach (var tx in incomes)
            {
                var converted = await _exchangeRateService.ConvertAsync(tx.Amount, tx.Currency, targetCurrency);
                total += converted;
            }

            return total;
        }

        public async Task<decimal> GetMonthlyBalanceAsync(int year, int month, Currency? targetCurrency = null)
        {
            targetCurrency ??= await ResolveUserCurrencyAsync();

            decimal total = 0m;

            var filtered = _transactions.Where(t => t.Date.Year == year && t.Date.Month == month);

            foreach (var tx in filtered)
            {
                var type = _categoryService.GetById(tx.CategoryId)?.Type;
                if (type == null)
                    continue;

                var converted = await _exchangeRateService.ConvertAsync(tx.Amount, tx.Currency, targetCurrency);
                total += type == CategoryType.Income ? converted : -converted;
            }

            return total;
        }

        private async Task<Currency> ResolveUserCurrencyAsync()
        {
            var code = _userContext.CurrentUser?.DefaultCurrencyCode
                ?? throw new InvalidOperationException("User not set or missing default currency");

            var currency = await _exchangeRateProvider.GetCurrencyByCodeAsync(code);

            return currency ?? throw new InvalidOperationException($"Currency '{code}' not found");
        }

    }
}
