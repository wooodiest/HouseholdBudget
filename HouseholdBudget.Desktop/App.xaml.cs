using HouseholdBudget.Core.Core;
using HouseholdBudget.Core.Models;
using Microsoft.Extensions.DependencyInjection;
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

            var loginService = Services.GetRequiredService<ILoginService>();
            //loginService.Register("Michał Kuchnicki", "Brooklyn99");  
            loginService.TryLogin("Michał Kuchnicki", "Brooklyn99");    

            ////loginService.Register("Maria Mrozek", "Marysiaaa");  
            //loginService.TryLogin("Maria Mrozek", "Marysiaaa"); 

            var mainWindow = new MainWindow(Services);
            mainWindow.Show();
        }
    }

}
