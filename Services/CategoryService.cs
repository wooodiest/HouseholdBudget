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
            LoadFromFile();
        }

        public List<Category> GetAll() => _categories;

        public Category? GetById(Guid id) =>
            _categories.FirstOrDefault(c => c.Id == id);

        public void Add(Category category)
        {
            _categories.Add(category);
            SaveToFile();
        }

        public void AddIfNotExists(Category category)
        {
            if (!_categories.Any(c => c.Id == category.Id))
                _categories.Add(category);

            SaveToFile();
        }

        public void Remove(Guid id)
        {
            var category = GetById(id);
            if (category != null)
                _categories.Remove(category);

            SaveToFile();
        }

        public void LoadFromFile()
        {
            if (!File.Exists(Path))
                return;

            var json = File.ReadAllText(Path);
            var loaded = JsonSerializer.Deserialize<List<Category>>(json);

            if (loaded != null)
            {
                _categories.Clear();
                _categories.AddRange(loaded);
            }
        }

        public void SaveToFile()
        {
            var json = JsonSerializer.Serialize(_categories, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(Path, json);
        }
    }
}
