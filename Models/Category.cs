using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HouseholdBudget.Models
{
    public enum CategoryType
    {
        Income,
        Expense
    }

    public class Category
    {
        public required Guid Id { get; set; }

        public required string Name { get; set; }

        public required CategoryType Type { get; set; }

    }
}
