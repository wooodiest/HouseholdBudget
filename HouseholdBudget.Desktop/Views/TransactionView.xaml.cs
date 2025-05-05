using HouseholdBudget.Core.Core;
using HouseholdBudget.Core.Services;
using HouseholdBudget.Desktop.ViewModels;
using Microsoft.Extensions.DependencyInjection;

using System.Windows.Controls;


namespace HouseholdBudget.Desktop.Views
{
    public partial class TransactionView : UserControl
    {
        public TransactionView()
        {
            InitializeComponent();
        }

        public void Initialize(IServiceProvider provider)
        {
            var tx   = provider.GetRequiredService<ITransactionService>();
            var cat  = provider.GetRequiredService<ICategoryService>();
            var user = provider.GetRequiredService<IUserContext>();
            var prov = provider.GetRequiredService<IExchangeRateProvider>();
            var rate = provider.GetRequiredService<IExchangeRateService>();

            DataContext = new TransactionViewModel(tx, cat, user, prov, rate);
        }
    }

}
