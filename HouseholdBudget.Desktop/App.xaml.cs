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

            var userContext = Services.GetRequiredService<IUserContext>();
            userContext.SetUser(new User {
                Id           = Guid.NewGuid(),
                Name         = "Michał Kuchnicki",
                Email        = "michalkuchnickiisc@gmail.com",
                PasswordHash = "password",
                CreatedAt    = DateTime.UtcNow,
            });

            var mainWindow = new MainWindow(Services);
            mainWindow.Show();
        }
    }

}
