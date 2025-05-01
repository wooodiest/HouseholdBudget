using HouseholdBudget.Data;
using HouseholdBudget.Managers;
using HouseholdBudget.Services;
using System.Configuration;
using System.Data;
using System.Windows;

namespace HouseholdBudget
{
    public partial class App : Application
    {
        public static DatabaseManager DatabaseManager { get; private set; } = null!;
        public static TransactionService TransactionService { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // CategoryManager.Instance.LoadDefaultCategories();
            DatabaseManager = new DatabaseManager("householdBudget.db");
            TransactionService = new TransactionService(DatabaseManager);

            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }

}
