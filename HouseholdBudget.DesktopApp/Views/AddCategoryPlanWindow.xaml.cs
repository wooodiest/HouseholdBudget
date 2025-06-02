using HouseholdBudget.Core.Models;
using HouseholdBudget.Core.Services.Interfaces;
using HouseholdBudget.Core.UserData;
using HouseholdBudget.DesktopApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
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
            if (_viewModel.SelectedCategory == null)
            {
                MessageBox.Show("Fill in required fields");
                return;
            }
            try
            {
                var currency = await _exchangeRateProvider.GetCurrencyByCodeAsync(_viewModel.SelectedCurrency);
                if (currency == null)
                {
                    MessageBox.Show("Selected currency is not supported.");
                    return;
                }

                var incomeText = _viewModel.IncomeText?.Trim();
                if (!decimal.TryParse(incomeText, out var parsedIncome) || parsedIncome <= 0)
                {
                    MessageBox.Show($"Amount must be a positive number: {parsedIncome}");
                    return;
                }

                var expenseText = _viewModel.IncomeText?.Trim();
                if (!decimal.TryParse(expenseText, out var parsedExpense) || parsedExpense <= 0)
                {
                    MessageBox.Show($"Amount must be a positive number: {parsedExpense}");
                    return;
                }

                if (_isEditMode)
                {
                    
                }
                else
                {


                    var catBudgetPlan = new CategoryBudgetPlan(
                        _budgetPlan.Id, _viewModel.SelectedCategory.Id,
                        parsedIncome,
                        parsedExpense,
                        currency);
                    Result = catBudgetPlan;
                    await _budgetPlanService.AddCategoryPlanAsync(_budgetPlan.Id, catBudgetPlan);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to add budget category plan:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
