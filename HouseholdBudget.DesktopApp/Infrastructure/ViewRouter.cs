using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HouseholdBudget.DesktopApp.Infrastructure
{
    public class ViewRouter : IViewRouter
    {
        private object? _currentView;
        public object? CurrentView
        {
            get => _currentView;
            private set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }

        public void ShowView(object view)
        {
            CurrentView = view;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
