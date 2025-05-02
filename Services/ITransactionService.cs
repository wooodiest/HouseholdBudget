using HouseholdBudget.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transaction = HouseholdBudget.Models.Transaction;

namespace HouseholdBudget.Services
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
