using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using HouseholdBudget.Core.UserData;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace HouseholdBudget.Core.Services.Remote
{
    public class AzureBlobStorageService : IAzureBlobStorageService
    {
        private readonly IUserSessionService _userSessionService;

        private readonly string _storageConnectionString;
        private readonly string _storageContainerName;


        public AzureBlobStorageService(IConfiguration configuration, IUserSessionService userSessionService)
        {
            _userSessionService = userSessionService;

            _storageConnectionString = configuration["AzureBlobStorage:ConnectionString"];
            _storageContainerName    = configuration["AzureBlobStorage:ContainerName"];
        }

        public async Task<IEnumerable<BlobObject>> GetBlobsByUserAsync()
        {
            try
            {
                var result = new List<BlobObject>();
                var userId = _userSessionService.GetUser().Id;
                var prefix = $"{userId}/";

                var containerClient = GetBlobContainerClient();
                await foreach (BlobItem item in containerClient.GetBlobsAsync(prefix: prefix))
                {
                    var blobClient = containerClient.GetBlobClient(item.Name);
                    result.Add(new BlobObject
                    {
                        Name = item.Name,
                        ImageUrl = blobClient.Uri.ToString()
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while getting the blob {ex.Message}");
            }      
        }

        public async Task<BlobObject> UploadAsync(string filePath)
        {
            try
            {
                var containerClient = GetBlobContainerClient();

                var userId = _userSessionService.GetUser().Id;
                var fileName = Path.GetFileName(filePath);
                var blobName = $"{userId}/{fileName}";

                var client = containerClient.GetBlobClient(blobName);

                await using var data = File.OpenRead(filePath);
                await client.UploadAsync(data, overwrite: true);

                return new BlobObject {
                    Name     = blobName,
                    ImageUrl = client.Uri.ToString()
                };

            }
            catch (Exception ex)
            {
                throw new Exception($"Error uploading blob {ex.Message}");
            }
        }

        private BlobContainerClient GetBlobContainerClient()
        {
            return new BlobContainerClient(_storageConnectionString, _storageContainerName);
        }
    }
}
