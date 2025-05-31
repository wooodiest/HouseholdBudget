using System.ComponentModel;

namespace HouseholdBudget.DesktopApp.Infrastructure
{
    public interface IViewRouter : INotifyPropertyChanged
    {
        object? CurrentView { get; }
        void ShowView(object view);
    }

}
