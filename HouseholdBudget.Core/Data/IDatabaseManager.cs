using HouseholdBudget.Core.Models;

namespace HouseholdBudget.Core.Data
{
    public interface IDatabaseManager
    {
        public List<Transaction> LoadTransactionsForUser(Guid userId);

        void SaveTransaction(Transaction transaction);

        void DeleteTransaction(Guid transactionId);


        public List<Category> LoadCategoriesForUser(Guid userId);

        void SaveCategory(Category category);

        void DeleteCategory(Guid categoryId);


        public List<Budget> LoadBudgetsForUser(Guid userId);

        public void SaveBudget(Budget budget);

        public void DeleteBudget(Guid budgetId);


        public List<BudgetPeriod> LoadBudgetPeriodsForUser(Guid userId);

        public void SaveBudgetPeriod(BudgetPeriod budgetPeriod);

        public void DeleteBudgetPeriod(Guid budgetPeriodId);


    }
}
