using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Azure.Storage.Sas;

namespace InvestNaijaAuth.Servicies
{
    public class AzureBlobService
    {
        private readonly AzureBlobSettings _settings;

        public AzureBlobService(IOptions<AzureBlobSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType)
        {
            var blobUri = $"https://{_settings.AccountName}.blob.core.windows.net";
            var credential = new Azure.Storage.StorageSharedKeyCredential(_settings.AccountName, _settings.AccountKey);
            var blobServiceClient = new BlobServiceClient(new Uri(blobUri), credential);
            var containerClient = blobServiceClient.GetBlobContainerClient(_settings.ContainerName);
            await containerClient.CreateIfNotExistsAsync(); // optional but safer
            var blobClient = containerClient.GetBlobClient(fileName);
            await blobClient.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = contentType });

            return blobClient.Uri.ToString();
        }

        public string GetSasUri(string fileName, int expiryMinutes = 30)
        {
            // Build the full blob URI
            var blobUri = $"https://{_settings.AccountName}.blob.core.windows.net/{_settings.ContainerName}/{fileName}";

            // Use storage account credentials
            var credential = new Azure.Storage.StorageSharedKeyCredential(_settings.AccountName, _settings.AccountKey);

            // Create a blob client
            var blobClient = new BlobClient(new Uri(blobUri), credential);

            // Build a SAS token
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _settings.ContainerName,
                BlobName = fileName,
                Resource = "b", // "b" means blob
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(expiryMinutes)
            };

            // Only allow read access
            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            // Generate the full SAS token
            var sasToken = sasBuilder.ToSasQueryParameters(credential).ToString();

            // Return full link with SAS token
            return $"{blobClient.Uri}?{sasToken}";
        }
        public async Task<string> UploadVideoAsync(IFormFile file, string fileName)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file was uploaded", nameof(file));

            using var stream = file.OpenReadStream();
            await UploadAsync(stream, fileName, file.ContentType);

            // Return time-limited secure access link
            return GetSasUri(fileName);
        }
        public async Task<bool> DeleteAsync(string fileName)
        {
            var blobUri = $"https://{_settings.AccountName}.blob.core.windows.net";
            var credential = new Azure.Storage.StorageSharedKeyCredential(_settings.AccountName, _settings.AccountKey);
            var blobServiceClient = new BlobServiceClient(new Uri(blobUri), credential);
            var containerClient = blobServiceClient.GetBlobContainerClient(_settings.ContainerName);

            var blobClient = containerClient.GetBlobClient(fileName);

            var result = await blobClient.DeleteIfExistsAsync();

            return result.Value; // true = deleted, false = did not exist
        }
        public async Task<List<string>> GetAllVideosAsync()
        {
            var blobUri = $"https://{_settings.AccountName}.blob.core.windows.net";
            var credential = new Azure.Storage.StorageSharedKeyCredential(_settings.AccountName, _settings.AccountKey);
            var blobServiceClient = new BlobServiceClient(new Uri(blobUri), credential);
            var containerClient = blobServiceClient.GetBlobContainerClient(_settings.ContainerName);

            var blobUrls = new List<string>();

            await foreach (var blobItem in containerClient.GetBlobsAsync())
            {
                // Optional: Generate SAS for each blob
                var sasUrl = GetSasUri(blobItem.Name);
                blobUrls.Add(sasUrl);
            }

            return blobUrls;
        }
        public async Task<string?> GetVideoByFileNameAsync(string fileName)
        {
            var blobUri = $"https://{_settings.AccountName}.blob.core.windows.net";
            var credential = new Azure.Storage.StorageSharedKeyCredential(_settings.AccountName, _settings.AccountKey);
            var blobServiceClient = new BlobServiceClient(new Uri(blobUri), credential);
            var containerClient = blobServiceClient.GetBlobContainerClient(_settings.ContainerName);

            var blobClient = containerClient.GetBlobClient(fileName);

            // Check if the blob exists
            if (await blobClient.ExistsAsync())
            {
                return GetSasUri(fileName); // Return time-limited secure access URL
            }

            return null; // Not found
        }

    }
}

