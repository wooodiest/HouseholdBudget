using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using HouseholdBudget.Core.UserData;
using HouseholdBudget.DesktopApp.Helpers;
using HouseholdBudget.DesktopApp.Infrastructure;
using System.Windows;
using System.ComponentModel.DataAnnotations;

namespace HouseholdBudget.DesktopApp.ViewModels
{
    public class RegisterViewModel : INotifyPropertyChanged
    {
        private string _name = "";
        private string _email = "";
        private string _currency = "";
        private string _errorMessage = "";

        private Func<string> _getPassword = () => "";

        private readonly IUserAuthenticator _authenticator;
        private readonly IWindowManager _windowManager;
        private readonly IUserSessionService _session;

        public RegisterViewModel(IUserAuthenticator authenticator, IWindowManager windowManager, IUserSessionService session)
        {
            _authenticator = authenticator;
            _windowManager = windowManager;
            _session = session;

            RegisterCommand = new RelayCommand(async _ => {
                try
                {
                    ErrorMessage = "";

                    if (await _authenticator.EmailExistsAsync(Email))
                    {
                        ErrorMessage = "An account with this email address already exists.";
                        return;
                    }

                    var password = _getPassword();
                    var user = await _authenticator.RegisterAsync(Name, Email, password, Currency);

                    var success = await _session.LoginAsync(Email, password);
                    if (success)
                    {
                        _windowManager.ShowMainWindow();
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
            _session = session;
        }

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        public string Currency
        {
            get => _currency;
            set { _currency = value; OnPropertyChanged(); }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public ICommand RegisterCommand { get; }

        public void SetPasswordProvider(Func<string> provider) => _getPassword = provider;

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
