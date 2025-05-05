using HouseholdBudget.Core.Models;

namespace HouseholdBudget.Core.Services
{
    public interface ICategoryService
    {
        Category GetOrAddCategory(string name, CategoryType type, out bool isNew);

        bool Exists(string name, CategoryType type);

        List<Category> GetAll();

        Category? GetById(Guid id);

        Category? GetByName(string name);

        void Remove(Guid id);

        void ClearAll();

        void Rename(Guid id, string newName);

    }
}
