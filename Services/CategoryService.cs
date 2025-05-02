using HouseholdBudget.Data;
using HouseholdBudget.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace HouseholdBudget.Services
{
    public class CategoryService
    {
        private readonly List<Category> _categories = new();

        private readonly IDatabaseManager _db;

        public CategoryService(IDatabaseManager db)
        {
            _db = db;
            _categories = _db.LoadCategories();
        }

        public List<Category> GetAll() => _categories;

        public Category? GetById(Guid id) =>
            _categories.FirstOrDefault(c => c.Id == id);

        public void AddIfNotExists(Category category)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

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
