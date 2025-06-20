﻿using HouseholdBudget.Core.Models;
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
using System.Windows;
using System.Windows.Input;

namespace HouseholdBudget.DesktopApp.ViewModels
{
    public class BudgetDetailsViewModel : INotifyPropertyChanged
    {
        private readonly ICategoryService _categoryService;

        private readonly IUserSessionService _userSessionService;

        private readonly IExchangeRateProvider _exchangeRateProvider;

        private readonly IBudgetPlanService _budgetPlanService;

        private readonly MainViewModel _mainViewModel;

        private BudgetPlan _budgetPlan;

        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public ObservableCollection<CategoryPlanViewModel> CategoryPlans { get; } = new();

        private CategoryPlanViewModel? _selectedCategoryPlan;
        public CategoryPlanViewModel? SelectedCategoryPlan
        {
            get => _selectedCategoryPlan;
            set
            {
                if (_selectedCategoryPlan != value)
                {
                    _selectedCategoryPlan = value;
                    OnPropertyChanged();

                    ((BasicRelayCommand)EditBudgetCategoryCommand).RaiseCanExecuteChanged();
                    ((BasicRelayCommand)DeleteBudgetCategoryCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand EditBudgetCommand { get; }


        public ICommand AddBudgetCategoryCommand { get; }
        public ICommand EditBudgetCategoryCommand { get; }
        public ICommand DeleteBudgetCategoryCommand { get; }

        public BudgetDetailsViewModel(ICategoryService categoryService, IUserSessionService userSessionService,
            IExchangeRateProvider exchangeRateProvider, IBudgetPlanService budgetPlanService, MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            _categoryService = categoryService;
            _userSessionService = userSessionService;
            _exchangeRateProvider = exchangeRateProvider;
            _budgetPlanService = budgetPlanService;

            EditBudgetCommand           = new BasicRelayCommand(EditBudget);
            AddBudgetCategoryCommand    = new BasicRelayCommand(AddCategoryPlan);
            EditBudgetCategoryCommand   = new DelegateCommand<CategoryPlanViewModel>(EditCategoryPlan);
            DeleteBudgetCategoryCommand = new DelegateCommand<CategoryPlanViewModel>(DeleteCategoryPlan);

        }

        public async Task Load(BudgetPlan plan)
        {
            _budgetPlan = plan;

            Name        = _budgetPlan.Name;
            Description = _budgetPlan.Description ?? "";
            StartDate   = _budgetPlan.StartDate;
            EndDate     = _budgetPlan.EndDate;

            CategoryPlans.Clear();
            foreach (var cat in _budgetPlan.CategoryPlans)
            {
                string catName = cat.CategoryId == Guid.Empty
                    ? "Uncategorized"
                    : (await _categoryService.GetCategoryByIdAsync(cat.CategoryId))?.Name ?? "Unknown";
                CategoryPlans.Add(new CategoryPlanViewModel(cat, catName));
            }

            OnPropertyChanged(null);
        }
        private async void AddCategoryPlan()
        {
            var window = new AddCategoryPlanWindow(_userSessionService, _categoryService,
                _exchangeRateProvider, _budgetPlanService, _budgetPlan)
            {
                Owner = Application.Current.MainWindow
            };

            if (window.ShowDialog() == true && window.Result != null)
            {
                var cat = await _categoryService.GetCategoryByIdAsync(window.Result.CategoryId);
                await Load(await _budgetPlanService.GetByIdAsync(_budgetPlan.Id) ?? _budgetPlan);

            }
        }

        private async void EditCategoryPlan(CategoryPlanViewModel? plan)
        {
            if (plan == null) return;

            var window = new AddCategoryPlanWindow(_userSessionService, _categoryService,
                _exchangeRateProvider, _budgetPlanService, _budgetPlan, plan.Model)
            {
                Owner = Application.Current.MainWindow
            };

            if (window.ShowDialog() == true && window.Result != null)
            {
                await Load(_budgetPlan);
            }
        }

        private async void DeleteCategoryPlan(object? param)
        {
            if (param is not CategoryPlanViewModel plan)
                return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete the category plan for '{plan.CategoryName}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                await _budgetPlanService.DeleteCategoryPlanAsync(_budgetPlan.Id, plan.Model.CategoryId);
                await Load(await _budgetPlanService.GetByIdAsync(_budgetPlan.Id) ?? _budgetPlan);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete category plan:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }




        private async void EditBudget()
        {
            var window = new AddBudgetWindow(_budgetPlanService, _userSessionService, _budgetPlan)
            {
                Owner = Application.Current.MainWindow
            };
            if (window.ShowDialog() == true && window.Result != null)
            {
                _budgetPlan = window.Result;
                await Load(_budgetPlan);
                await _mainViewModel.LoadBudgetsAsync();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class CategoryPlanViewModel
    {
        public CategoryBudgetPlan Model { get; set; }
        public string CategoryName { get; set; } = "Loading...";
        public decimal IncomePlanned { get; set; }
        public decimal IncomeExecuted { get; set; }
        public decimal ExpensePlanned { get; set; }
        public decimal ExpenseExecuted { get; set; }
        public string CurrencySymbol { get; set; }

        public string IncomePlannedFormatted => $"Income Planned: {IncomePlanned:N2} {CurrencySymbol}";

        public string IncomeExecutedFormatted => $"Income Executed: {IncomeExecuted:N2} {CurrencySymbol}";

        public string ExpensePlannedFormatted => $"Expense Planned: {ExpensePlanned:N2} {CurrencySymbol}";

        public string ExpenseExecutedFormatted => $"Expense Executed: {ExpenseExecuted:N2} {CurrencySymbol}";

        public double IncomeProgress => IncomePlanned > 0 ? (double)(IncomeExecuted / IncomePlanned) : 0;
        public double ExpenseProgress => ExpensePlanned > 0 ? (double)(ExpenseExecuted / ExpensePlanned) : 0;

        public CategoryPlanViewModel(CategoryBudgetPlan model, string categoryName)
        {
            Model           = model;  
            CategoryName    = categoryName;
            IncomePlanned   = model.IncomePlanned;
            IncomeExecuted  = model.IncomeExecuted;
            ExpensePlanned  = model.ExpensePlanned;
            ExpenseExecuted = model.ExpenseExecuted;
            CurrencySymbol  = model.CurrencyCode;
        }
    }
}
