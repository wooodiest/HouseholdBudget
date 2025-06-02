using HouseholdBudget.Core.Models;
using HouseholdBudget.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HouseholdBudget.DesktopApp.ViewModels
{
    public class BudgetDetailsViewModel : INotifyPropertyChanged
    {
        private readonly ICategoryService _categoryService;

        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public ObservableCollection<CategoryPlanViewModel> CategoryPlans { get; } = new();

        public BudgetDetailsViewModel(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async void Load(BudgetPlan plan)
        {
            Name        = plan.Name;
            Description = plan.Description ?? "";
            StartDate   = plan.StartDate;
            EndDate     = plan.EndDate;

            CategoryPlans.Clear();
            foreach (var cat in plan.CategoryPlans)
            {
                string catName = cat.CategoryId == Guid.Empty
                    ? "Uncategorized"
                    : (await _categoryService.GetCategoryByIdAsync(cat.CategoryId))?.Name ?? "Unknown";
                CategoryPlans.Add(new CategoryPlanViewModel(cat, catName));
            }

            OnPropertyChanged(null);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class CategoryPlanViewModel
    {
        public string CategoryName { get; set; } = "Loading...";
        public decimal IncomePlanned { get; set; }
        public decimal IncomeExecuted { get; set; }
        public decimal ExpensePlanned { get; set; }
        public decimal ExpenseExecuted { get; set; }
        public string CurrencySymbol { get; set; }

        public double IncomeProgress => IncomePlanned > 0 ? (double)(IncomeExecuted / IncomePlanned) : 0;
        public double ExpenseProgress => ExpensePlanned > 0 ? (double)(ExpenseExecuted / ExpensePlanned) : 0;

        public CategoryPlanViewModel(CategoryBudgetPlan model, string categoryName)
        {
            CategoryName    = categoryName;
            IncomePlanned   = model.IncomePlanned;
            IncomeExecuted  = model.IncomeExecuted;
            ExpensePlanned  = model.ExpensePlanned;
            ExpenseExecuted = model.ExpenseExecuted;
            CurrencySymbol  = model.Currency?.Symbol ?? "$";
        }
    }
}
