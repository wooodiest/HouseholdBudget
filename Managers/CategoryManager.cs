using HouseholdBudget.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HouseholdBudget.Managers
{
    public class CategoryManager
    {
        private static CategoryManager? _instance;

        public static CategoryManager Instance => _instance ??= new CategoryManager();

        private readonly List<Category> _categories = new();

        public List<Category> GetAll() => _categories;

        public Category? GetById(Guid id)
        {
            return _categories.FirstOrDefault(c => c.Id == id);
        }

        public void AddIfNotExists(Category category)
        {
            if (!_categories.Any(c => c.Id == category.Id))
            {
                _categories.Add(category);
            }
        }
    }
}
