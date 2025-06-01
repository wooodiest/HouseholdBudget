using HouseholdBudget.Core.Models;
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
using System.Windows.Shapes;

namespace HouseholdBudget.DesktopApp.Views
{
    /// <summary>
    /// Interaction logic for AddBudgetWindow.xaml
    /// </summary>
    public partial class AddBudgetWindow : Window
    {
        public BudgetPlan? Result { get; private set; }

        public AddBudgetWindow()
        {
            InitializeComponent();
        }
    }
}
