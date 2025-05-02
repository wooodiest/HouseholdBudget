using HouseholdBudget.Data;
using HouseholdBudget.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transaction = HouseholdBudget.Models.Transaction;

namespace HouseholdBudget.Services
{
    public class TransactionService
    {
        private readonly List<Transaction> _transactions = new();

        private readonly IDatabaseManager _db;

        private readonly CategoryService _categoryService;

        public TransactionService(IDatabaseManager db, CategoryService categoryService)
        {
            _db = db;
            _categoryService = categoryService;
            _transactions = _db.LoadTransactions();
        }

        public void AddTransaction(Transaction transaction)
        {
            _categoryService.AddIfNotExists(transaction.Category);
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

        /// 

        public List<Transaction> GetAll() => _transactions;

        public List<Transaction> GetByCategory(Guid categoryId) =>
            _transactions.Where(t => t.CategoryId == categoryId).ToList();

        public List<Transaction> GetByCategoryList(List<Guid> categoryIds) =>
           _transactions.Where(t => categoryIds.Contains(t.CategoryId)).ToList();

        public List<Transaction> GetByDate(DateTime date) =>
            _transactions.Where(t => t.Date.Date == date.Date).ToList();

        public List<Transaction> GetByDateRange(DateTime start, DateTime end) =>
            _transactions.Where(t => t.Date.Date >= start.Date && t.Date.Date <= end.Date).ToList();

        public List<Transaction> GetByMonth(int year, int month) =>
            _transactions.Where(t => t.Date.Year == year && t.Date.Month == month).ToList();

        public List<Transaction> GetRecurringTransactions() =>
            _transactions.Where(t => t.IsRecurring).ToList();

        public List<Transaction> GetByDescriptionKeyword(string keyword) =>
           _transactions.Where(t => t.Description != null && t.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase)).ToList();

        public List<Transaction> GetByAmountRange(decimal min, decimal max) =>
            _transactions.Where(t => t.Amount >= min && t.Amount <= max).ToList();

        public List<Transaction> GetByCategoryType(CategoryType type) =>
            _transactions.Where(t => _categoryService.GetById(t.CategoryId)?.Type == type).ToList();

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
            GetByMonth(year, month).Sum(t =>
            {
                var type = _categoryService.GetById(t.CategoryId)?.Type;
                return type == CategoryType.Income ? t.Amount : -t.Amount;
            });

        
    }
}
