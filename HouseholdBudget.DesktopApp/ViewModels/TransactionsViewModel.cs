﻿using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using HouseholdBudget.Core.Models;
using HouseholdBudget.Core.Services.Interfaces;
using HouseholdBudget.DesktopApp.Commands;
using HouseholdBudget.DesktopApp.Views;
using HouseholdBudget.DesktopApp.Helpers;
using HouseholdBudget.Core.UserData;
using System.Windows.Media.Animation;
using System.Windows.Media;
using HouseholdBudget.Core.Services.Remote;
using Microsoft.Win32;

namespace HouseholdBudget.DesktopApp.ViewModels
{
    public class TransactionsViewModel : INotifyPropertyChanged
    {
        private readonly ITransactionService _transactionService;
        private readonly ICategoryService _categoryService;
        private readonly IUserSessionService _userSessionService;
        private readonly IExchangeRateProvider _exchangeRateProvider;
        private readonly IAzureBlobStorageService _azureBlobStorageService;
        private readonly IAzureDocumentAnalysisService _azureDocumentAnalysisService;

        public ObservableCollection<TransactionViewModel> Transactions { get; set; } = new();
        public ObservableCollection<Category> Categories { get; set; } = new();

        private TransactionViewModel? _selectedTransaction;
        public TransactionViewModel? SelectedTransaction
        {
            get => _selectedTransaction;
            set
            {
                if (_selectedTransaction != value)
                {
                    _selectedTransaction = value;
                    OnPropertyChanged();

                    ((BasicRelayCommand)EditTransactionCommand).RaiseCanExecuteChanged();
                    ((BasicRelayCommand)DeleteTransactionCommand).RaiseCanExecuteChanged();
                }
            }
        }

        private Category? _selectedCategory;
        public Category? SelectedCategory
        {
            get => _selectedCategory;
            set => SetField(ref _selectedCategory, value);
        }

        public List<string> TransactionTypes { get; } = new() { "All", "Income", "Expense" };

        public string FilterDescription { get; set; } = string.Empty;
        public DateTime? FilterStartDate { get; set; }
        public DateTime? FilterEndDate { get; set; }

        public string? FilterMinAmount { get; set; }
        public string? FilterMaxAmount { get; set; }
        public string FilterSelectedTransactionType { get; set; } = "All";

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

        public ICommand AddTransactionCommand { get; }
        public ICommand AddRecipeCommand { get; }
        public ICommand EditTransactionCommand { get; }
        public ICommand DeleteTransactionCommand { get; }
        public ICommand ApplyFilterCommand { get; }
        public ICommand ClearFiltersCommand { get; }
        public ICommand AddCategoryCommand { get; }
        public ICommand DeleteCategoryCommand { get; }


        public TransactionsViewModel(
            ITransactionService           transactionService,
            ICategoryService              categoryService,
            IUserSessionService           userSessionService,
            IExchangeRateProvider         exchangeRateProvider,
            IAzureBlobStorageService      azureBlobStorageService,
            IAzureDocumentAnalysisService azureDocumentAnalysisService)
        {
            _transactionService           = transactionService;
            _categoryService              = categoryService;
            _userSessionService           = userSessionService;
            _exchangeRateProvider         = exchangeRateProvider;
            _azureBlobStorageService      = azureBlobStorageService;
            _azureDocumentAnalysisService = azureDocumentAnalysisService;

            AddTransactionCommand    = new BasicRelayCommand(AddTransaction);
            AddRecipeCommand         = new BasicRelayCommand(async () => await AddRecipe());
            EditTransactionCommand   = new BasicRelayCommand(EditTransaction, () => SelectedTransaction != null);
            DeleteTransactionCommand = new BasicRelayCommand(DeleteTransaction, () => SelectedTransaction != null);
            ApplyFilterCommand       = new BasicRelayCommand(async () => await LoadTransactionsAsync());
            ClearFiltersCommand      = new BasicRelayCommand(ClearFilters);
            AddCategoryCommand       = new BasicRelayCommand(async () => await AddCategoryAsync());
            DeleteCategoryCommand    = new DelegateCommand<Category?>(async (category) => await DeleteCategoryAsync(category), c => c != null);

            _ = InitAsync();
        }

        private async Task InitAsync()
        {
            var categories = await _categoryService.GetUserCategoriesAsync();
            Categories = new ObservableCollection<Category>(categories);
            OnPropertyChanged(nameof(Categories));

            await LoadTransactionsAsync();
            await LoadCurrenciesAsync();
        }

        private async Task LoadTransactionsAsync()
        {
            var filter = new TransactionFilter {
                DescriptionKeyword = FilterDescription,
                StartDate = FilterStartDate,
                EndDate   = FilterEndDate,
                CategoryIds = SelectedCategory != null ? new() { SelectedCategory.Id } : null,
                MinAmount = !string.IsNullOrEmpty(FilterMinAmount) ? decimal.Parse(FilterMinAmount) : null,
                MaxAmount = !string.IsNullOrEmpty(FilterMaxAmount) ? decimal.Parse(FilterMaxAmount) : null,
                TransactionType = FilterSelectedTransactionType switch {
                    "Expense" => TransactionType.Expense,
                    "Income" => TransactionType.Income,
                    _ => null 
                },
                CurrencyCode = SelectedCurrency
            };

            var transactions = await _transactionService.GetAsync(filter);
            var transactionViewModels = new List<TransactionViewModel>();
            foreach (var tx in transactions)
            {
                var cat = await _categoryService.GetCategoryByIdAsync(tx.CategoryId);
                var name = cat?.Name ?? "(none)";
                transactionViewModels.Add(new TransactionViewModel(tx, name));
            }

            Transactions = new ObservableCollection<TransactionViewModel>(transactionViewModels);
            OnPropertyChanged(nameof(Transactions));

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

        private async Task AddRecipe()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select Receipt File",
                Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg",
                Multiselect = false
            };

            if (dialog.ShowDialog() == true)
            {
                string filePath = dialog.FileName;
                try
                {
                    var uploadedBlob = await _azureBlobStorageService.UploadAsync(filePath);
                    if (uploadedBlob == null)
                    {
                        MessageBox.Show("Failed to upload receipt. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var result = await _azureDocumentAnalysisService.AnalyzeReceiptFromBlobAsync(uploadedBlob);
                    if (result == null)
                    {
                        MessageBox.Show("Failed to analyze receipt. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var amount      = result.Total ?? 0.0m;
                    var description = result.MerchantName ?? "Receipt";
                    var date        = result.TransactionDate ?? DateTime.Now;
                    var categoryId  = (await _categoryService.GetCategoryByNameAsync("Receipts"))?.Id ?? Guid.Empty;
                    if (categoryId == Guid.Empty)
                    {
                        var newCategory = await _categoryService.CreateCategoryAsync("Receipts");
                        categoryId      = newCategory.Id;
                    }

                    var transaction = Transaction.Create(
                        Guid.NewGuid(),
                        categoryId,
                        amount,
                        _userSessionService.GetUser()!.DefaultCurrencyCode,
                        TransactionType.Expense,
                        description,
                        date
                    );

                    var window = new AddTransactionWindow(_transactionService, _categoryService,
                        _exchangeRateProvider, _userSessionService, false, transaction)
                    {
                        Owner = Application.Current.MainWindow
                    };

                    if (window.ShowDialog() == true && window.Result != null)
                    {
                        var cat = await _categoryService.GetCategoryByIdAsync(window.Result.CategoryId);
                        Transactions.Add(new TransactionViewModel(window.Result, cat?.Name ?? "(none)"));
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to upload recipe:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

        }

        private async Task LoadCurrenciesAsync()
        {
            var currencies = await _exchangeRateProvider.GetSupportedCurrenciesAsync();

            SupportedCurrencies.Clear();
            foreach (var c in currencies)
                SupportedCurrencies.Add(c.Code);

            OnPropertyChanged(nameof(SupportedCurrencies));
        }


        private async Task DeleteCategoryAsync(Category? category)
        {
            if (category == null) 
                return;

            var result = MessageBox.Show($"Are you sure you want to delete category '{category.Name}'?",
                                         "Confirm Deletion",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Warning);

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
        private async void ClearFilters()
        {
            FilterDescription = string.Empty;
            SelectedCategory  = null;
            FilterSelectedTransactionType = "All";
            FilterMinAmount  = string.Empty;
            FilterMaxAmount  = string.Empty;
            FilterStartDate  = null;
            FilterEndDate    = null;
            SelectedCurrency = null;

            OnPropertyChanged(nameof(FilterDescription));
            OnPropertyChanged(nameof(SelectedCategory));
            OnPropertyChanged(nameof(FilterSelectedTransactionType));
            OnPropertyChanged(nameof(FilterMinAmount));
            OnPropertyChanged(nameof(FilterMaxAmount));
            OnPropertyChanged(nameof(FilterStartDate));
            OnPropertyChanged(nameof(FilterEndDate));
            OnPropertyChanged(nameof(SelectedCurrency));

            await LoadTransactionsAsync();
        }

        private async void AddTransaction()
        {
            var window = new AddTransactionWindow(_transactionService, _categoryService,
                _exchangeRateProvider, _userSessionService)
            {
                Owner = Application.Current.MainWindow
            };

            if (window.ShowDialog() == true && window.Result != null)
            {
                var cat = await _categoryService.GetCategoryByIdAsync(window.Result.CategoryId);
                Transactions.Add(new TransactionViewModel(window.Result, cat?.Name ?? "(none)"));
            }
        }

        private async void EditTransaction()
        {
            if (SelectedTransaction == null)
                return;

            var window = new AddTransactionWindow(_transactionService, _categoryService,
                _exchangeRateProvider, _userSessionService, true, SelectedTransaction.Model)
            {
                Owner = Application.Current.MainWindow
            };


            if (window.ShowDialog() == true && window.Result != null)
            {
                await LoadTransactionsAsync();
            }
        }


        private async void DeleteTransaction()
        {
            if (SelectedTransaction == null) 
                return;

            await _transactionService.DeleteAsync(SelectedTransaction.Model.Id);
            await LoadTransactionsAsync();
        }

        // === INotifyPropertyChanged helpers ===

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
