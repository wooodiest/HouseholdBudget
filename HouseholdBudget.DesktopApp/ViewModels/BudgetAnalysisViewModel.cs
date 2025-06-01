using HouseholdBudget.Core.Services.Interfaces;
using HouseholdBudget.DesktopApp.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HouseholdBudget.DesktopApp.ViewModels
{
    public class BudgetAnalysisViewModel : INotifyPropertyChanged
    {
        private readonly IBudgetAnalysisService _analysisService;

        public BudgetAnalysisViewModel(IBudgetAnalysisService analysisService)
        {
            _analysisService = analysisService;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
