using HouseholdBudget.Core.Services.Interfaces;
using HouseholdBudget.Core.Services.Remote;
using HouseholdBudget.DesktopApp.ViewModels;
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

namespace HouseholdBudget.DesktopApp.Views.Controls
{
   
    /// <summary>
    /// Interaction logic for ReceiptsView.xaml
    /// </summary>
    public partial class ReceiptsView : UserControl
    {
        private readonly ReceiptsViewModel _vm;

        public ReceiptsView(IServiceProvider serviceProvider)
        {
            InitializeComponent();

            var blobService = serviceProvider.GetRequiredService<IAzureBlobStorageService>();
            _vm = new ReceiptsViewModel(blobService);
            this.DataContext = _vm;
        }
    }
}
