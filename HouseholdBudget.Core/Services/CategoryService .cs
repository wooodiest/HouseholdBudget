using HouseholdBudget.Core.Core;
using HouseholdBudget.Core.Data;
using HouseholdBudget.Core.Models;

namespace HouseholdBudget.Core.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly List<Category> _categories = new();

        private readonly IDatabaseManager _db;

        private readonly IUserContext _userContext;

        public CategoryService(IUserContext userContext, IDatabaseManager db)
        {
            _userContext = userContext;
            _db = db;
            _categories = _db.LoadCategoriesForUser(userContext.CurrentUser.Id);
        }

        public List<Category> GetAll() => _categories;

        public Category? GetById(Guid id) =>
            _categories.FirstOrDefault(c => c.Id == id);

        public void AddIfNotExists(Category category)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            category.UserId = _userContext.CurrentUser.Id;
            if (!_categories.Any(c => c.Id == category.Id))
            {
                _categories.Add(category);
                _db.SaveCategory(category);
            }
        }

        public void Remove(Guid id)
        {
            var category = GetById(id);
            if (category != null)
            {
                _categories.Remove(category);
                _db.DeleteCategory(id);
            }
        }
    }
}
