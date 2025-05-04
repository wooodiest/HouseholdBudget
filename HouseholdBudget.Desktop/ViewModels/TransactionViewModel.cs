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
        public ObservableCollection<Transaction> Transactions { get; } = new();

        public ObservableCollection<Category> Categories { get; } = new();

        public Transaction NewTransaction { get; set; } = new()
        {
            Id = Guid.NewGuid(),
            Date = DateTime.Now,
            Description = "Empty description",
            Amount = 0
        };

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

        public string NewCategoryName { get; set; } = "";

        public string CurrentUserName => _userContext.CurrentUser.Name;

        public ICommand AddTransactionCommand { get; }
        public ICommand AddCategoryCommand { get; }

        private readonly ITransactionService _transactionService;

        private readonly ICategoryService _categoryService;

        private readonly IUserContext _userContext;

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
                Transactions.Add(tx);
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

            NewTransaction.Id = Guid.NewGuid();
            NewTransaction.CategoryId = SelectedCategory.Id;

            _transactionService.AddTransaction(NewTransaction);
            Transactions.Add(NewTransaction);

            NewTransaction = new Transaction
            {
                Id = Guid.NewGuid(),
                Description = "Empty description",
                Date = DateTime.Now,
                Amount = 0,
                CategoryId = SelectedCategory.Id

            };
            OnPropertyChanged(nameof(NewTransaction));
        }

        private void AddCategory()
        {
            if (string.IsNullOrWhiteSpace(NewCategoryName))
                return;

            var newCat = new Category
            {
                Id = Guid.NewGuid(),
                Name = NewCategoryName,
                Type = CategoryType.Expense
            };

            _categoryService.AddIfNotExists(newCat);
            Categories.Add(newCat);
            SelectedCategory = newCat;
            NewCategoryName = "";
            OnPropertyChanged(nameof(NewCategoryName));
        }
    }
}
