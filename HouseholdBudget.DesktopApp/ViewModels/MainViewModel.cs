using System.ComponentModel;
using System.Runtime.CompilerServices;
using HouseholdBudget.Core.UserData;

namespace HouseholdBudget.DesktopApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IUserSessionService _session;

        public MainViewModel(IUserSessionService session)
        {
            _session = session;
            UpdateUserDisplay();
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

        public void Logout()
        {
            _session.Logout();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
