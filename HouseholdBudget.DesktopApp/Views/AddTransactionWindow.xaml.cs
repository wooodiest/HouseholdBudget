using HouseholdBudget.Core.Services.Interfaces;
using HouseholdBudget.DesktopApp.ViewModels;
using System.Windows;
using HouseholdBudget.Core.Models;
using HouseholdBudget.Core.UserData;

namespace HouseholdBudget.DesktopApp.Views
{
    /// <summary>
    /// Interaction logic for AddTransactionWindow.xaml
    /// </summary>
    public partial class AddTransactionWindow : Window
    {
        public Transaction? Result { get; private set; }

        private readonly AddTransactionViewModel _viewModel;

        private readonly ITransactionService _transactionService;
        private readonly ICategoryService _categoryService;
        private readonly IExchangeRateProvider _exchangeRateProvider;
        private readonly IUserSessionService _session;

        private readonly bool _isEditMode;
        private readonly Transaction? _existingTransaction;

        public AddTransactionWindow(ITransactionService transactionService, ICategoryService categoryService,
            IExchangeRateProvider exchangeRateProvider, IUserSessionService userSessionService, Transaction? existing = null)
        {
            _transactionService   = transactionService;
            _categoryService      = categoryService;
            _exchangeRateProvider = exchangeRateProvider;
            _session              = userSessionService;

            _isEditMode = existing != null;
            _existingTransaction = existing;

            InitializeComponent();
            _viewModel = new AddTransactionViewModel(_categoryService, _exchangeRateProvider, _session, existing);
            DataContext = _viewModel;

            Loaded += async (_, _) => await Task.Run(() => _viewModel.LoadCategoriesCommand.Execute(null));
        }

        private async void Action_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedCategory == null || string.IsNullOrWhiteSpace(_viewModel.Description))
            {
                MessageBox.Show("Fill in required fields");
                return;
            }

            try
            {
                var currency = await _exchangeRateProvider.GetCurrencyByCodeAsync(_viewModel.SelectedCurrency);

                var amountText = _viewModel.AmountText?.Trim();
                if (!decimal.TryParse(amountText, out var parsedAmount) || parsedAmount <= 0)
                {
                    MessageBox.Show($"Amount must be a positive number: {parsedAmount}");
                    return;
                }     

                Result = await _transactionService.CreateAsync(
                    _viewModel.SelectedCategory.Id,
                    parsedAmount,
                    currency!,
                    _viewModel.SelectedType,
                    _viewModel.Description,
                    null,
                    _viewModel.Date);
               
                if (_isEditMode)
                    await _transactionService.DeleteAsync(_existingTransaction!.Id);
                
            } catch (Exception ex)
            {
                MessageBox.Show($"Failed to add transaction:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
