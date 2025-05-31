using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media;

namespace HouseholdBudget.DesktopApp.Views.Controls
{
    /// <summary>
    /// Interaction logic for TransactionsView.xaml
    /// </summary>
    public partial class TransactionsView : UserControl
    {

        private bool _filtersVisible = false;

        public TransactionsView()
        {
            InitializeComponent();
        }

        private void ToggleFilterPanel_Click(object sender, RoutedEventArgs e)
        {
            double from = _filtersVisible ? 0 : 300;
            double to = _filtersVisible ? 300 : 0;

            var animation = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = new Duration(TimeSpan.FromMilliseconds(300)),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            FilterPanelTransform.BeginAnimation(TranslateTransform.XProperty, animation);

            if (_filtersVisible)
            {
                FilterPanelColumn.Width = new GridLength(0);
            }
            else
            {
                FilterPanelColumn.Width = new GridLength(300); 
            }

            _filtersVisible = !_filtersVisible;
        }

    }
}