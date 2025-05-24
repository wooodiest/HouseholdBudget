using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using HouseholdBudget.DesktopApp.Views.Controls;

namespace HouseholdBudget.DesktopApp.ViewModels
{
    public class AuthViewModel : INotifyPropertyChanged
    {
        public LoginViewModel LoginVM { get; }
        public RegisterViewModel RegisterVM { get; }

        private bool _isLoginVisible = true;

        public string ToggleText => _isLoginVisible
            ? "Create an account"
            : "Already have an account?";

        public UserControl CurrentView => _isLoginVisible
            ? new LoginView { DataContext = LoginVM }
            : new RegisterView { DataContext = RegisterVM };

        public AuthViewModel(LoginViewModel loginVM, RegisterViewModel registerVM)
        {
            LoginVM = loginVM;
            RegisterVM = registerVM;
        }

        public void ToggleView()
        {
            _isLoginVisible = !_isLoginVisible;
            OnPropertyChanged(nameof(CurrentView));
            OnPropertyChanged(nameof(ToggleText));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
