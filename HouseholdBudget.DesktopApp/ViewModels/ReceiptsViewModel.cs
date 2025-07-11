using HouseholdBudget.Core.Services.Remote;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HouseholdBudget.DesktopApp.ViewModels
{
    public class ReceiptsViewModel
    {
        private readonly IAzureBlobStorageService _blobService;

        public ObservableCollection<BlobObject> Blobs { get; } = new();

        public ReceiptsViewModel(IAzureBlobStorageService blobService)
        {
            _blobService = blobService;
            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            Blobs.Clear();
            foreach (var blob in await _blobService.GetBlobsByUserAsync())
                Blobs.Add(blob);
        }
    }
}
