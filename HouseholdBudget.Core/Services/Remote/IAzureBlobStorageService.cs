using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HouseholdBudget.Core.Services.Remote
{
    public class BlobObject
    {
        public string Name { get; set; }
        public string ImageUrl { get; set; }
    }

    public interface IAzureBlobStorageService
    {
        Task<BlobObject> UploadAsync(string filePath);
        Task<IEnumerable<BlobObject>> GetBlobsByUserAsync();
    }
}
