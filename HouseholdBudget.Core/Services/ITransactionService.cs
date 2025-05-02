using HouseholdBudget.Core.Helpers;
using HouseholdBudget.Core.Models;

namespace HouseholdBudget.Core.Services
{
    public interface ITransactionService
    {
        void AddTransaction(Transaction transaction);

        void RemoveTransaction(Guid id);

        void Clear();

        List<Transaction> GetAll();

        List<Transaction> GetByFilter(TransactionFilter filter);

        decimal GetTotalAmount();

        decimal GetTotalExpense();

        decimal GetTotalIncome();

        decimal GetMonthlyBalance(int year, int month);
    }
}
