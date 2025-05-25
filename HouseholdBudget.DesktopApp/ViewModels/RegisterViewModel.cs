using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using HouseholdBudget.Core.UserData;
using HouseholdBudget.DesktopApp.Helpers;
using HouseholdBudget.DesktopApp.Infrastructure;
using System.Windows;
using System.ComponentModel.DataAnnotations;
using HouseholdBudget.Core.Models;
using System.Collections.ObjectModel;
using HouseholdBudget.Core.Services;

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
        private readonly IExchangeRateProvider _exchangeRateProvider;

        public RegisterViewModel(IUserAuthenticator authenticator, IWindowManager windowManager,
            IUserSessionService session, IExchangeRateProvider exchangeRateProvider)
        {
            _authenticator = authenticator;
            _windowManager = windowManager;
            _session = session;
            _exchangeRateProvider = exchangeRateProvider;

            RegisterCommand = new RelayCommand(async _ =>
            {
                try
                {
                    ErrorMessage = "";

                    if (SelectedCurrency == null)
                    {
                        ErrorMessage = "Please select a currency.";
                        return;
                    }

                    if (await _authenticator.EmailExistsAsync(Email))
                    {
                        ErrorMessage = "An account with this email address already exists.";
                        return;
                    }

                    var password = _getPassword();
                    var user = await _authenticator.RegisterAsync(Name, Email, password, SelectedCurrency.Code);

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

            _ = LoadCurrenciesAsync();
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

        private Currency? _selectedCurrency;
        public ObservableCollection<Currency> SupportedCurrencies { get; } = new();

        public Currency? SelectedCurrency
        {
            get => _selectedCurrency;
            set
            {
                _selectedCurrency = value;
                OnPropertyChanged();
            }
        }

        private async Task LoadCurrenciesAsync()
        {
            var currencies = await _exchangeRateProvider.GetSupportedCurrenciesAsync();
            SupportedCurrencies.Clear();
            foreach (var c in currencies)
                SupportedCurrencies.Add(c);

            SelectedCurrency = SupportedCurrencies.FirstOrDefault();
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
