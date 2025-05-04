using HouseholdBudget.Core.Core;
using HouseholdBudget.Core.Data;
using HouseholdBudget.Core.Helpers;
using HouseholdBudget.Core.Models;

namespace HouseholdBudget.Core.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly List<Transaction> _transactions = new();
        private readonly IDatabaseManager _db;
        private readonly ICategoryService _categoryService;
        private readonly IUserContext _userContext;

        public TransactionService(IUserContext userContext, IDatabaseManager db, ICategoryService categoryService)
        {
            _userContext = userContext;
            _db = db;
            _categoryService = categoryService;
            _transactions = _db.LoadTransactionsForUser(_userContext.CurrentUser.Id);
        }

        public void AddTransaction(Transaction transaction)
        {
            transaction.UserId = _userContext.CurrentUser.Id;
            _transactions.Add(transaction);
            _db.SaveTransaction(transaction);
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
        public decimal GetTotalAmount() =>
            _transactions.Sum(t => t.Amount);

        public decimal GetTotalExpense() =>
            _transactions
                .Where(t => _categoryService.GetById(t.CategoryId)?.Type == CategoryType.Expense)
                .Sum(t => t.Amount);

        public decimal GetTotalIncome() =>
            _transactions
                .Where(t => _categoryService.GetById(t.CategoryId)?.Type == CategoryType.Income)
                .Sum(t => t.Amount);

        public decimal GetMonthlyBalance(int year, int month) =>
            _transactions
                .Where(t => t.Date.Year == year && t.Date.Month == month)
                .Sum(t =>
                {
                    var type = _categoryService.GetById(t.CategoryId)?.Type;
                    return type == CategoryType.Income ? t.Amount : -t.Amount;
                });
    }
}
