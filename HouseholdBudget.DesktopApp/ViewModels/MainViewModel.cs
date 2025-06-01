using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
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

        public ICommand ShowTransactionsCommand { get; }

        public IViewRouter ViewRouter => _viewRouter;

        public MainViewModel(IServiceProvider serviceProvider, IUserSessionService session, IViewRouter viewRouter)
        {
            _serviceProvider = serviceProvider;
            _session = session;
            _viewRouter = viewRouter;
            UpdateUserDisplay();

            ShowTransactionsCommand = new BasicRelayCommand(ShowTransactions);
            ShowTransactions();
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
            var vm = _serviceProvider.GetRequiredService<TransactionsViewModel>();
            var view = new TransactionsView { DataContext = vm };
            _viewRouter.ShowView(view);
        }

        public void ShowAnalysis()
        {
            var vm = _serviceProvider.GetRequiredService<BudgetAnalysisViewModel>();
            var view = new AnalysisView { DataContext = vm };
            _viewRouter.ShowView(view);
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
