using HouseholdBudget.Core.Core;
using HouseholdBudget.Core.Models;
using HouseholdBudget.Core.Services;
using HouseholdBudget.ViewModels;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace HouseholdBudget.Desktop.ViewModels
{
    public class TransactionViewModel : BaseViewModel
    {
        private readonly ITransactionService _transactionService;
        private readonly ICategoryService    _categoryService;
        private readonly IUserContext        _userContext;

        public ICommand AddTransactionCommand { get; }
        public ICommand AddCategoryCommand { get; }

        public ObservableCollection<TransactionViewModelItem> Transactions { get; } = new();
        public ObservableCollection<Category> Categories { get; } = new();

        public string CurrentUserName => _userContext.CurrentUser.Name;
        public string NewCategoryName { get; set; } = "";

        public Transaction NewTransaction { get; set; } = new() { Description = "Empty description",};

        private Category? _selectedCategory;
        public Category? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged();
            }
        }

        public TransactionViewModel(ITransactionService service, ICategoryService categoryService, IUserContext userContext)
        {
            _transactionService = service;
            _categoryService = categoryService;
            _userContext = userContext;

            LoadTransactions();
            LoadCategories();

            AddTransactionCommand = new RelayCommand(AddTransaction);
            AddCategoryCommand = new RelayCommand(AddCategory);
        }

        private void LoadTransactions()
        {
            Transactions.Clear();

            foreach (var tx in _transactionService.GetAll())
            {
                Transactions.Add(new TransactionViewModelItem
                {
                    Id          = tx.Id,
                    Date        = tx.Date,
                    Description = tx.Description,
                    Amount      = tx.Amount,
                    CategoryName = _categoryService.GetById(tx.CategoryId)?.Name ?? "(none)"
                });
            }
        }

        private void LoadCategories()
        {
            Categories.Clear();
            foreach (var cat in _categoryService.GetAll())
                Categories.Add(cat);
        }

        private void AddTransaction()
        {
            if (NewTransaction.Amount <= 0 || SelectedCategory == null)
                return;

            NewTransaction.CategoryId  = SelectedCategory.Id;
            NewTransaction.IsRecurring = false;

            _transactionService.AddTransaction(NewTransaction);

            Transactions.Add(new TransactionViewModelItem
            {
                Id           = NewTransaction.Id,
                Date         = NewTransaction.Date,
                Description  = NewTransaction.Description,
                Amount       = NewTransaction.Amount,
                CategoryName = SelectedCategory.Name
            });

            NewTransaction = new Transaction {
                Description = "Empty description",
            };
            OnPropertyChanged(nameof(NewTransaction));
        }

        private void AddCategory()
        {
            if (string.IsNullOrWhiteSpace(NewCategoryName))
                return;

            var category = _categoryService.GetOrAddCategory(NewCategoryName, CategoryType.Expense, out bool isNew);

            if (isNew)
                Categories.Add(category);

            SelectedCategory = category;
            NewCategoryName = "";
            OnPropertyChanged(nameof(NewCategoryName));
        }
    }
}
