using System.Windows;
using System.Windows.Controls;
using HouseholdBudget.DesktopApp.ViewModels;

namespace HouseholdBudget.DesktopApp.Views.Controls
{
    public partial class RegisterView : UserControl
    {
        public RegisterView()
        {
            InitializeComponent();
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is RegisterViewModel vm)
            {
                vm.SetPasswordProvider(() => PasswordBox.Password);
                vm.RegisterCommand.Execute(null);
            }
        }

    }
}
