using HouseholdBudget.Core.Core;
using HouseholdBudget.Core.Data;
using HouseholdBudget.Core.Models;
using System.Xml.Linq;

namespace HouseholdBudget.Core.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly List<Category> _categories = new();

        private readonly IDatabaseManager _database;

        private readonly IUserContext _userContext;

        public CategoryService(IUserContext userContext, IDatabaseManager database)
        {
            _userContext = userContext;
            _database    = database;

            _categories = _database.LoadCategoriesForUser(userContext.CurrentUser.Id);
        }

        public List<Category> GetAll() => _categories;


        public Category? GetById(Guid id) =>
            _categories.FirstOrDefault(c => c.Id == id);

        public Category? GetByName(string name)
        {
            var userId = _userContext.CurrentUser.Id;

            return _categories.FirstOrDefault(c =>
                c.Name.Equals(name, StringComparison.OrdinalIgnoreCase) &&
                c.UserId == userId);
        }

        public bool Exists(string name, CategoryType type)
        {
            var userId = _userContext.CurrentUser.Id;

            return _categories.Any(c =>
                c.Name.Equals(name, StringComparison.OrdinalIgnoreCase) &&
                c.Type == type &&
                c.UserId == userId);
        }

        public Category GetOrAddCategory(string name, CategoryType type, out bool isNew)
        {
            var userId = _userContext.CurrentUser.Id;

            var existing = _categories.FirstOrDefault(c =>
                c.Name.Equals(name, StringComparison.OrdinalIgnoreCase) &&
                c.Type == type &&
                c.UserId == userId);

            if (existing != null)
            {
                isNew = false;
                return existing;
            }

            var category = new Category
            {
                Id     = Guid.NewGuid(),
                Name   = name,
                Type   = type,
                UserId = userId
            };

            _categories.Add(category);
            _database.SaveCategory(category);

            isNew = true;
            return category;
        }

        public void Remove(Guid id)
        {
            var category = GetById(id);
            if (category != null)
            {
                _categories.Remove(category);
                _database.DeleteCategory(id);
            }
        }

        public void ClearAll()
        {
            foreach (var cat in _categories)
            {
                _categories.Remove(cat);
                _database.DeleteCategory(cat.Id);
            }
        }

        public void Rename(Guid id, string newName)
        {
            var category = GetById(id);
            if (category == null) 
                return;

            category.Name = newName;
            _database.SaveCategory(category);
        }
    }
}
