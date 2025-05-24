using System.Windows;
using System.Windows.Controls;
using HouseholdBudget.DesktopApp.ViewModels;

namespace HouseholdBudget.DesktopApp.Views.Controls
{
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel vm)
            {
                vm.SetPasswordProvider(() => PasswordBox.Password);
                vm.LoginCommand.Execute(null);
            }
        }
    }
}
