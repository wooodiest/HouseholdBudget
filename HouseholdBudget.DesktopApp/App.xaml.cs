using System.Globalization;
using System.Windows;
using HouseholdBudget.Core.UserData;
using HouseholdBudget.DesktopApp.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace HouseholdBudget.DesktopApp
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");

            base.OnStartup(e);

            ServiceProvider = AppBootstrapper.ConfigureServices();

            var session = ServiceProvider.GetRequiredService<IUserSessionService>();
            var windowManager = ServiceProvider.GetRequiredService<IWindowManager>();

            if (!session.IsAuthenticated)
                windowManager.ShowLoginWindow();
            else
                windowManager.ShowMainWindow();
        }
    }

}
