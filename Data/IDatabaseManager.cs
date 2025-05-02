using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transaction = HouseholdBudget.Models.Transaction;

namespace HouseholdBudget.Data
{
    public interface IDatabaseManager
    {
        void SaveTransaction(Transaction transaction);

        List<Transaction> LoadTransactions();

        void DeleteTransaction(Guid transactionId);

        void UpdateTransaction(Transaction transaction);
    }
}
