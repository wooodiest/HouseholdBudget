using Microsoft.Extensions.DependencyInjection;
using HouseholdBudget.DesktopApp.Views;
using System.Windows;

namespace HouseholdBudget.DesktopApp.Infrastructure
{
    public class WindowManager : IWindowManager
    {
        private readonly IServiceProvider _provider;

        public WindowManager(IServiceProvider provider)
        {
            _provider = provider;
        }

        public void ShowLoginWindow()
        {
            CloseWindowsOfType<MainWindow>();

            var login = _provider.GetRequiredService<LoginWindow>();
            login.Show();

            Application.Current.MainWindow = login;
        }

        public void ShowMainWindow()
        {
            CloseWindowsOfType<LoginWindow>();

            var main = _provider.GetRequiredService<MainWindow>();
            main.Show();

            Application.Current.MainWindow = main;
        }

        private void CloseWindowsOfType<T>() where T : Window
        {
            foreach (var window in Application.Current.Windows.OfType<T>().ToList())
            {
                window.Close();
            }
        }
    }
}
