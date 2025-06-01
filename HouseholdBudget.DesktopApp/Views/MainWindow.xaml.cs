using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using HouseholdBudget.Core.UserData;
using HouseholdBudget.DesktopApp.Infrastructure;
using HouseholdBudget.DesktopApp.ViewModels;
using HouseholdBudget.DesktopApp.Views;
using Microsoft.Extensions.DependencyInjection;

namespace HouseholdBudget.DesktopApp
{
    public partial class MainWindow : Window
    {
        private readonly IWindowManager _windowManager;
        private readonly IViewRouter _viewRouter;
        private readonly MainViewModel _viewModel;

        public MainWindow(IWindowManager windowManager, IViewRouter iViewRouter, MainViewModel viewModel)
        {
            InitializeComponent();
            _windowManager = windowManager;
            _viewRouter    = iViewRouter;
            _viewModel     = viewModel;
            DataContext = _viewModel;
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.UpdateUserDisplay();
            _viewModel.Logout();
            _windowManager.ShowLoginWindow();
            this.Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void TransactionsButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ShowTransactions();
        }

        private void AnalysisButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.ShowAnalysis();
        }

        private void AddBudget_Click(object sender, RoutedEventArgs e)
        {
        }

        private void RemoveBudget_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }
    }

}
