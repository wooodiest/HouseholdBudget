using HouseholdBudget.Core.Models;

namespace HouseholdBudget.Core.Data
{
    public interface IDatabaseManager
    {
        void SaveTransaction(Transaction transaction);

        public List<Transaction> LoadTransactionsForUser(Guid userId);

        void DeleteTransaction(Guid transactionId);

        void UpdateTransaction(Transaction transaction);

        void SaveCategory(Category category);

        public List<Category> LoadCategoriesForUser(Guid userId);

        void DeleteCategory(Guid categoryId);
    }
}
