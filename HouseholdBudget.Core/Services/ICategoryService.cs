using HouseholdBudget.Core.Models;

namespace HouseholdBudget.Core.Services
{
    public interface ICategoryService
    {
        List<Category> GetAll();
        Category? GetById(Guid id);
        void AddIfNotExists(Category category);
        void Remove(Guid id);
    }
}
