using HouseholdBudget.Core.Models;
using HouseholdBudget.Core.Services.Interfaces;
using HouseholdBudget.Core.UserData;
using HouseholdBudget.DesktopApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HouseholdBudget.DesktopApp.Views
{
    /// <summary>
    /// Interaction logic for AddCategoryPlanWindow.xaml
    /// </summary>
    public partial class AddCategoryPlanWindow : Window
    {
        public CategoryBudgetPlan? Result { get; private set; }

        private readonly AddCategoryPlanViewModel _viewModel;

        private readonly bool _isEditMode;
        private readonly CategoryBudgetPlan? _existingBudgetPlan;
        private BudgetPlan _budgetPlan;

        private readonly ICategoryService _categoryService;
        private readonly IUserSessionService _session;
        private readonly IExchangeRateProvider _exchangeRateProvider;
        private readonly IBudgetPlanService _budgetPlanService;

        public AddCategoryPlanWindow(IUserSessionService userSessionService, ICategoryService categoryService,
             IExchangeRateProvider exchangeRateProvider, IBudgetPlanService budgetPlanService, BudgetPlan plan, CategoryBudgetPlan? exsisting = null)
        {
            _budgetPlan           = plan;
            _categoryService      = categoryService;
            _session              = userSessionService;
            _exchangeRateProvider = exchangeRateProvider;
            _budgetPlanService    = budgetPlanService;
            _isEditMode           = exsisting != null;
            _existingBudgetPlan   = exsisting;

            InitializeComponent();

            _viewModel = new AddCategoryPlanViewModel(
                _categoryService,
                _session,
                exchangeRateProvider,
                exsisting);

            DataContext = _viewModel;
        }

        private async void Action_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_viewModel.SelectedCategory == null)
                {
                    MessageBox.Show("Category must be selected.");
                    return;
                }

                if (_budgetPlan.CategoryPlans.Any(p => p.CategoryId == _viewModel.SelectedCategory.Id))
                {
                    MessageBox.Show("A plan for this category already exists.");
                    return;
                }

                var incomeText = _viewModel.IncomeText?.Trim();
                if (!decimal.TryParse(incomeText, out var parsedIncome) || parsedIncome < 0)
                {
                    MessageBox.Show("Income must be a positive number.");
                    return;
                }

                var expenseText = _viewModel.ExpenseText?.Trim();
                if (!decimal.TryParse(expenseText, out var parsedExpense) || parsedExpense < 0)
                {
                    MessageBox.Show("Expense must be a positive number.");
                    return;
                }

                var catBudgetPlan = new CategoryBudgetPlan(
                    _budgetPlan.Id,
                    _viewModel.SelectedCategory.Id,
                    parsedIncome,
                    parsedExpense,
                    _viewModel.SelectedCurrency);

                Result = catBudgetPlan;
                await _budgetPlanService.AddCategoryPlanAsync(_budgetPlan.Id, catBudgetPlan);

                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to add budget category plan:\n{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
