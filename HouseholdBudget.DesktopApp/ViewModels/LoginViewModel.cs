using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using HouseholdBudget.Core.UserData;
using HouseholdBudget.DesktopApp.Infrastructure;
using HouseholdBudget.DesktopApp.Helpers;
using System.ComponentModel.DataAnnotations;

namespace HouseholdBudget.DesktopApp.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private string _email = "";
        private string _errorMessage = "";

        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public ICommand LoginCommand { get; }

        private Func<string> _getPassword = () => "Brooklyn99#";
        private Window? _currentWindow;
        private readonly IUserSessionService _session;
        private readonly IWindowManager _windowManager;

        public LoginViewModel(IUserSessionService session, IWindowManager windowManager)
        {
            _session = session;
            _windowManager = windowManager;

            LoginCommand = new RelayCommand(async _ => {
                try
                {
                    ErrorMessage = "";
                    var success = await _session.LoginAsync(Email, _getPassword());
                    if (success)
                    {
                        _windowManager.ShowMainWindow();
                        _currentWindow?.Close();
                    }
                    else
                    {
                        ErrorMessage = "Invalid credentials.";
                    }
                }
                catch (ValidationException ex)
                {
                    ErrorMessage = ex.Message;
                }
                catch (System.Exception ex)
                {
                    ErrorMessage = "Unexpected error: " + ex.Message;
                }
                
            });
        }

        public void SetPasswordProvider(Func<string> provider) => _getPassword = provider;
        public void AttachWindow(Window window) => _currentWindow = window;

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    
}
