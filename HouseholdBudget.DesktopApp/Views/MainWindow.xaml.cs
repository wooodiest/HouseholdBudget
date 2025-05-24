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
        private readonly MainViewModel _viewModel;

        public MainWindow(IWindowManager windowManager, MainViewModel viewModel)
        {
            InitializeComponent();
            _windowManager = windowManager;
            _viewModel = viewModel;
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

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }
    }

}
