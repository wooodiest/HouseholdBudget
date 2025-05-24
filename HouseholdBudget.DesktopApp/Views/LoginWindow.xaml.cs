using System.Windows;
using System.Windows.Input;
using HouseholdBudget.DesktopApp.ViewModels;
using HouseholdBudget.DesktopApp.Views.Controls;

namespace HouseholdBudget.DesktopApp.Views
{
    public partial class LoginWindow : Window
    {
        private readonly AuthViewModel _authViewModel;

        public LoginWindow(AuthViewModel viewModel)
        {
            InitializeComponent();
            _authViewModel = viewModel;
            DataContext = _authViewModel;
            ShowCurrentView();
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            _authViewModel.ToggleView();
            ShowCurrentView();
        }

        private void ShowCurrentView()
        {
            ContentContainer.Content = _authViewModel.CurrentView;
            ToggleButton.Content = _authViewModel.ToggleText;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                this.DragMove();
        }

    }
}
