using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HouseholdBudget.src.Core.Models
{
    public enum CategoryType
    {
        Income,
        Expense
    }

    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public CategoryType Type { get; set; }

        public Category(int id, string name, CategoryType type)
        {
            Id   = id;
            Name = name;
            Type = type;
        }

        public void EditCategory(string name, CategoryType type)
        {
            Name = name;
            Type = type;
        }

        public override string ToString()
        {
            return $"ID: {Id}, Name: {Name}, Type: {Type}";
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }
}
