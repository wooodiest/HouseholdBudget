using HouseholdBudget.Core.Models;

namespace HouseholdBudget.Core.Data
{
    public interface IDatabaseManager
    {
        void SaveTransaction(Transaction transaction);

        List<Transaction> LoadTransactions();

        void DeleteTransaction(Guid transactionId);

        void UpdateTransaction(Transaction transaction);

        void SaveCategory(Category category);

        List<Category> LoadCategories();

        void DeleteCategory(Guid categoryId);
    }
}
