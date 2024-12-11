using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Cuttr.Business.Exceptions;
using Cuttr.Business.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cuttr.Business.Services
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<BlobStorageService> _logger;

        public BlobStorageService(IConfiguration configuration, ILogger<BlobStorageService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string containerName)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty.");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
                throw new ArgumentException("Unsupported file type.");

            // Optional: Limit file size (e.g., max 5MB)
            const long maxFileSize = 5 * 1024 * 1024;
            if (file.Length > maxFileSize)
                throw new ArgumentException("File size exceeds the limit.");

            var blobName = Guid.NewGuid().ToString() + extension;
            BlobContainerClient containerClient = GetContainerClient(containerName);
            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            try
            {
                using (var stream = file.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });
                }

                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error uploading file to Azure Blob Storage in container '{containerName}'.");
                throw new BusinessException("Error uploading image.", ex);
            }
        }

        public async Task DeleteFileAsync(string fileUrl, string containerName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileUrl))
                    return;

                Uri uri = new Uri(fileUrl);
                string blobName = Path.GetFileName(uri.LocalPath);

                BlobContainerClient containerClient = GetContainerClient(containerName);
                BlobClient blobClient = containerClient.GetBlobClient(blobName);

                await blobClient.DeleteIfExistsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting file from Azure Blob Storage. URL: {fileUrl}");
                throw new BusinessException("Error deleting image.", ex);
            }
        }

        private BlobContainerClient GetContainerClient(string containerName)
        {
            string connectionString = _configuration.GetConnectionString("AzureBlobStorage");
            var containerClient = new BlobContainerClient(connectionString, containerName);
            containerClient.CreateIfNotExists(PublicAccessType.Blob);
            return containerClient;
        }
    }
}

