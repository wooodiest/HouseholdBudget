using HouseholdBudget.Core.Services;
using HouseholdBudget.Desktop.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            var tx  = provider.GetRequiredService<ITransactionService>();
            var cat = provider.GetRequiredService<ICategoryService>();
            DataContext = new TransactionViewModel(tx, cat);
        }
    }

}
