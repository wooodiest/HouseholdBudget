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
    /// Interaction logic for AddBudgetWindow.xaml
    /// </summary>
    public partial class AddBudgetWindow : Window
    {
        public BudgetPlan? Result { get; private set; }

        private readonly AddBudgetViewModel _viewModel;
        private readonly IBudgetPlanService _budgetService;
        private readonly IUserSessionService _session;

        private readonly bool _isEditMode;
        private readonly BudgetPlan? _existing;

        public AddBudgetWindow(IBudgetPlanService budgetService, IUserSessionService userSessionService, BudgetPlan? existing = null)
        {
            InitializeComponent();

            _budgetService = budgetService;
            _session       = userSessionService;
            _existing      = existing;
            _isEditMode    = existing != null;

            _viewModel = new AddBudgetViewModel(existing);
            DataContext = _viewModel;
        }

        private async void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_viewModel.Name) || _viewModel.StartDate == null || _viewModel.EndDate == null)
            {
                MessageBox.Show("Please provide a name and valid date range.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (_isEditMode)
                {
                    // TODO: Update existing budget
                    Result = _existing;
                }
                else
                {
                    Result = await _budgetService.CreatePlanAsync(_session.GetUser()!.Id,
                        _viewModel.Name,
                        _viewModel.StartDate.Value,
                        _viewModel.EndDate.Value,
                        _viewModel.Description);

                    DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while saving budget:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
