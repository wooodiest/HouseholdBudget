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
using System.Windows;
using System.Windows.Input;

namespace HouseholdBudget.DesktopApp.ViewModels
{

    public class AddTransactionViewModel : INotifyPropertyChanged
    {
        private readonly ICategoryService _categoryService;

        private readonly IExchangeRateProvider _exchangeRateProvider;

        private readonly IUserSessionService _session;

        private readonly bool _isEditMode;
        public string HeaderText => _isEditMode ? "Modify Transaction" : "Add New Transaction";
        public string ButtonText => _isEditMode ? "Modify" : "Add";

        public ObservableCollection<Category> Categories { get; set; } = new();

        private readonly Transaction? _existingTransaction;

        public ObservableCollection<string> TransactionTypes { get; } =
            new(new[] { "Income", "Expense" });


        public string SelectedType { get; set; } = "Expense";
        public Category? SelectedCategory { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public string Description { get; set; } = string.Empty;
        public string AmountText { get; set; } = "0,00";

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
        public ICommand AddCategoryCommand { get; }
        public ICommand DeleteCategoryCommand { get; }

        public AddTransactionViewModel(ICategoryService categoryService, IExchangeRateProvider exchangeRateProvider,
            IUserSessionService userSessionService, bool editMode = false, Transaction? existingTransaction = null)
        {
            _categoryService      = categoryService;
            _exchangeRateProvider = exchangeRateProvider;
            _session              = userSessionService;
            _existingTransaction  = existingTransaction;
            _isEditMode           = editMode;

            LoadCategoriesCommand = new BasicRelayCommand(async () => await LoadCategoriesAsync());
            AddCategoryCommand    = new BasicRelayCommand(async () => await AddCategoryAsync());
            DeleteCategoryCommand = new DelegateCommand<Category?>(async (c) => await DeleteCategoryAsync(c), c => c != null);

            _ = LoadCurrenciesAsync();

            if (_existingTransaction != null)
            {
                SelectedType     = _existingTransaction.Type == TransactionType.Expense ? "Expense" : "Income";
                Date             = _existingTransaction.Date;
                Description      = _existingTransaction.Description;
                AmountText       = _existingTransaction.Amount.ToString("F2");
                SelectedCurrency = _existingTransaction.CurrencyCode;
            }
        }

        private async Task LoadCategoriesAsync()
        {
            var categories = await _categoryService.GetUserCategoriesAsync();
            Categories = new ObservableCollection<Category>(categories);

            if (_existingTransaction != null)
            {
                SelectedCategory = Categories.FirstOrDefault(c => c.Id == _existingTransaction.CategoryId);
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


        private async Task AddCategoryAsync()
        {
            var window = new AddCategoryWindow(_categoryService)
            {
                Owner = Application.Current.MainWindow
            };
            if (window.ShowDialog() == true)
            {
                try
                {
                    var newCategory = await _categoryService.CreateCategoryAsync(window.CategoryName!);
                    Categories.Add(newCategory);
                    SelectedCategory = newCategory;

                    OnPropertyChanged(nameof(Categories));
                    OnPropertyChanged(nameof(SelectedCategory));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to add category:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task DeleteCategoryAsync(Category? category)
        {
            if (category == null) 
                return;

            var result = MessageBox.Show($"Delete category '{category.Name}'?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                await _categoryService.DeleteCategoryAsync(category.Id);
                Categories.Remove(category);
                if (SelectedCategory == category)
                    SelectedCategory = null;

                OnPropertyChanged(nameof(Categories));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete category:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
