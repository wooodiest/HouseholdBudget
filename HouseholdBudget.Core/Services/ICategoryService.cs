using HouseholdBudget.Core.Models;

namespace HouseholdBudget.Core.Services
{
    public interface ICategoryService
    {
        List<Category> GetAll();

        Category? GetById(Guid id);

        public Category GetOrAddCategory(string name, CategoryType type, out bool isNew);

        void Remove(Guid id);
    }
}
