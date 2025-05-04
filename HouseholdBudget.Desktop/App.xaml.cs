using HouseholdBudget.Core.Data;
using HouseholdBudget.Core.Services;
using System.Windows;

namespace HouseholdBudget.Desktop
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Services = AppBootstrapper.Configure();

            var mainWindow = new MainWindow(Services);
            mainWindow.Show();
        }
    }

}
