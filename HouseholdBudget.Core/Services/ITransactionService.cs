using HouseholdBudget.Core.Helpers;
using HouseholdBudget.Core.Models;

namespace HouseholdBudget.Core.Services
{
    public interface ITransactionService
    {
        public Transaction AddTransaction(string description, decimal amount, Currency currency, Guid categoryID, DateTime dateTime, bool isRecurring);

        void RemoveTransaction(Guid id);

        void Clear();

        List<Transaction> GetAll();

        List<Transaction> GetByFilter(TransactionFilter filter);

        Task<decimal> GetTotalAmountAsync(Currency? targetCurrency = null);

        Task<decimal> GetTotalExpenseAsync(Currency? targetCurrency = null);

        Task<decimal> GetTotalIncomeAsync(Currency? targetCurrency = null);

        Task<decimal> GetMonthlyBalanceAsync(int year, int month, Currency? targetCurrency = null);
    }
}
