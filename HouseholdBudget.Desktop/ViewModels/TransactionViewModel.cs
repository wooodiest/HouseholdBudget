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
        public string NewTransactionDescription { get; set; } = "Empty description";
        public decimal NewTransactionAmount { get; set; }
        public DateTime NewTransactionDate { get; set; } = DateTime.Now;
        public bool NewTransactionIsRecurring { get; set; } = false;

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
            if (NewTransactionAmount <= 0 || SelectedCategory == null)
                return;

            var created = _transactionService.AddTransaction(
                description: NewTransactionDescription,
                amount:      NewTransactionAmount,
                categoryID:  SelectedCategory.Id,
                dateTime:    NewTransactionDate,
                isRecurring: NewTransactionIsRecurring
            );


            Transactions.Add(new TransactionViewModelItem
            {
                Id           = created.Id,
                Date         = created.Date,
                Description  = created.Description,
                Amount       = created.Amount,
                CategoryName = SelectedCategory.Name
            });

            NewTransactionDescription = "Empty description";
            NewTransactionAmount      = 0;
            NewTransactionDate        = DateTime.Now;
            NewTransactionIsRecurring = false;

            OnPropertyChanged(nameof(NewTransactionDescription));
            OnPropertyChanged(nameof(NewTransactionAmount));
            OnPropertyChanged(nameof(NewTransactionDate));
            OnPropertyChanged(nameof(NewTransactionIsRecurring));
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
