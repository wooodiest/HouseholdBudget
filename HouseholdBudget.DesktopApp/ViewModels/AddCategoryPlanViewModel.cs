using HouseholdBudget.Core.Models;
using HouseholdBudget.Core.Services.Interfaces;
using HouseholdBudget.Core.UserData;
using HouseholdBudget.DesktopApp.Commands;
using HouseholdBudget.DesktopApp.Helpers;
using HouseholdBudget.DesktopApp.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace HouseholdBudget.DesktopApp.ViewModels
{
    public class AddCategoryPlanViewModel
    {
        private readonly ICategoryService _categoryService;

        private readonly IUserSessionService _session;

        private readonly IExchangeRateProvider _exchangeRateProvider;

        private bool _isEditMode;

        public bool IsEditMode
        {
            get => _isEditMode;
            set
            {
                _isEditMode = value;
                OnPropertyChanged();
            }
        }

        public string HeaderText => _isEditMode ? "Modify Category Plan" : "Add Category Plan";
        public string ButtonText => _isEditMode ? "Modify" : "Add";

        public ObservableCollection<Category> Categories { get; set; } = new();
        public Category? SelectedCategory { get; set; }

        private readonly CategoryBudgetPlan? _existingCategoryPlan;

        private readonly BudgetPlan _budgetPlan;

        public string IncomeText { get; set; } = "0,00";
        public string ExpenseText { get; set; } = "0,00";

        private string? _selectedCurrency;
        public ObservableCollection<string> SupportedCurrencies { get; } = new();

        public string? SelectedCurrency
        {
            get => _selectedCurrency;
            set
            {
                _selectedCurrency = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoadCategoriesCommand { get; }

        public AddCategoryPlanViewModel(BudgetPlan budgetPlan, ICategoryService categoryService, IUserSessionService userSessionService,
            IExchangeRateProvider exchangeRateProvider, CategoryBudgetPlan? existingCategoryPlan = null)
        {
            _budgetPlan           = budgetPlan;
            _categoryService      = categoryService;
            _session              = userSessionService;
            _exchangeRateProvider = exchangeRateProvider;
            _existingCategoryPlan = existingCategoryPlan;
            IsEditMode = existingCategoryPlan != null;

            LoadCategoriesCommand = new BasicRelayCommand(async () => await LoadCategoriesAsync());

            _ = LoadCurrenciesAsync();
            _ = LoadCategoriesAsync();

            if (_existingCategoryPlan != null)
            {
                IncomeText       = _existingCategoryPlan.IncomePlanned.ToString("F2");
                ExpenseText      = _existingCategoryPlan.ExpensePlanned.ToString("F2");
                SelectedCurrency = _existingCategoryPlan.CurrencyCode;
            }
        }

        private async Task LoadCategoriesAsync()
        {
            var existingCategoryIds = _budgetPlan.CategoryPlans
                .Select(cp => cp.CategoryId);

            var categories = await _categoryService.GetUserCategoriesAsync();

            var editingCategoryId = _existingCategoryPlan?.CategoryId;

            categories = categories
                .Where(c => !existingCategoryIds.Contains(c.Id) || c.Id == editingCategoryId)
                .OrderBy(c => c.Name)
                .ToList();

            Categories = new ObservableCollection<Category>(categories);

            if (_existingCategoryPlan != null)
            {
                SelectedCategory = Categories.FirstOrDefault(c => c.Id == _existingCategoryPlan.CategoryId);
                OnPropertyChanged(nameof(SelectedCategory));
            }

            OnPropertyChanged(nameof(Categories));
        }


        private async Task LoadCurrenciesAsync()
        {
            var currencies = await _exchangeRateProvider.GetSupportedCurrenciesAsync();

            SupportedCurrencies.Clear();
            foreach (var c in currencies)
                SupportedCurrencies.Add(c.Code);

            string? defaultCurrency = _session.GetUser()?.DefaultCurrencyCode;

            if (defaultCurrency != null)
            {
                SelectedCurrency = SupportedCurrencies.FirstOrDefault(c => c == defaultCurrency)
                                 ?? SupportedCurrencies.FirstOrDefault();
            }
            else
            {
                SelectedCurrency = SupportedCurrencies.FirstOrDefault();
            }

            OnPropertyChanged(nameof(SupportedCurrencies));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
