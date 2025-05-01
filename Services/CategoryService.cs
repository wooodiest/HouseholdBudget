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

        public readonly string Path;

        public CategoryService(string path)
        {
            Path = path;
            LoadFromFile(Path);
        }

        public List<Category> GetAll() => _categories;

        public Category? GetById(Guid id) =>
            _categories.FirstOrDefault(c => c.Id == id);

        public void Add(Category category)
        {
            _categories.Add(category);
            SaveToFile(Path);
        }

        public void AddIfNotExists(Category category)
        {
            if (!_categories.Any(c => c.Id == category.Id))
                _categories.Add(category);

            SaveToFile(Path);
        }

        public void Remove(Guid id)
        {
            var category = GetById(id);
            if (category != null)
                _categories.Remove(category);

            SaveToFile(Path);
        }

        public void LoadFromFile(string path)
        {
            if (!File.Exists(path))
                return;

            var json = File.ReadAllText(path);
            var loaded = JsonSerializer.Deserialize<List<Category>>(json);

            if (loaded != null)
            {
                _categories.Clear();
                _categories.AddRange(loaded);
            }
        }

        public void SaveToFile(string path)
        {
            var json = JsonSerializer.Serialize(_categories, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(path, json);
        }
    }
}
