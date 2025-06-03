using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using HouseholdBudget.Core.Models;
using HouseholdBudget.Core.Services.Interfaces;
using HouseholdBudget.Core.UserData;
using HouseholdBudget.DesktopApp.Commands;
using HouseholdBudget.DesktopApp.Infrastructure;
using HouseholdBudget.DesktopApp.Views.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace HouseholdBudget.DesktopApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IUserSessionService _session;
        private readonly IViewRouter _viewRouter;
        private readonly IBudgetPlanService _budgetService;
        private readonly ICategoryService _categoryService;

        public ObservableCollection<BudgetPlan> Budgets { get; } = new();
        private BudgetPlan? _selectedBudget;
        public BudgetPlan? SelectedBudget
        {
            get => _selectedBudget;
            set
            {
                if (_selectedBudget != value)
                {
                    _selectedBudget = value;
                    OnPropertyChanged();
                    if (value != null)
                        ShowBudget(value);
                }
            }
        }

        public ICommand ShowTransactionsCommand { get; }

        public IViewRouter ViewRouter => _viewRouter;

        public MainViewModel(IServiceProvider serviceProvider, IUserSessionService session, 
            IViewRouter viewRouter, IBudgetPlanService budgetPlanService, ICategoryService categoryService)
        {
            _serviceProvider = serviceProvider;
            _session = session;
            _viewRouter = viewRouter;
            _budgetService = budgetPlanService;
            _categoryService = categoryService;
            UpdateUserDisplay();

            ShowTransactionsCommand = new BasicRelayCommand(ShowTransactions);
            ShowTransactions();

            _ = LoadBudgetsAsync();
            _ = _categoryService.InitAsync();
        }

        private string _loggedInUserName = "Unknown";
        public string LoggedInUserName
        {
            get => _loggedInUserName;
            set { _loggedInUserName = value; OnPropertyChanged(); }
        }

        public void UpdateUserDisplay()
        {
            var user = _session.GetUser();
            LoggedInUserName = $"Logged in as: {user?.Name ?? "Unknown"}";
        }

        public void ShowTransactions()
        {
            SelectedBudget = null;
            var vm = _serviceProvider.GetRequiredService<TransactionsViewModel>();
            var view = new TransactionsView { DataContext = vm };
            _viewRouter.ShowView(view);
        }

        public void ShowAnalysis()
        {
            SelectedBudget = null;
            var view = new AnalysisView(_serviceProvider);
            _viewRouter.ShowView(view);
        }

        public async void ShowBudget(BudgetPlan plan)
        {
            var vm = _serviceProvider.GetRequiredService<BudgetDetailsViewModel>();
            await vm.Load(plan);
            var view = new BudgetDetailsView { DataContext = vm };
            _viewRouter.ShowView(view);
        }

        public async Task LoadBudgetsAsync()
        {
            await _budgetService.InitAsync();
            var plans = await _budgetService.GetAllPlansAsync();
            Budgets.Clear();
            foreach (var plan in plans)
                Budgets.Add(plan);
        }

        public async Task DeleteBudget()
        {
            if (SelectedBudget == null) 
                return;

            int deletedIndex = Budgets.IndexOf(SelectedBudget);

            await _budgetService.DeletePlanAsync(SelectedBudget.Id);
            Budgets.Remove(SelectedBudget);

            if (Budgets.Count > 0)
            {
                int newIndex = Math.Max(0, deletedIndex - 1);
                SelectedBudget = Budgets[newIndex];
            }
            else
            {
                ShowTransactions();
                SelectedBudget = null;
            }
        }

        public void Logout()
        {
            _session.Logout();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
