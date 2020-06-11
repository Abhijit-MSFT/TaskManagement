using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;

namespace TaskManagement.Helper
{
    public class BlobStorageHelper
    {
        private readonly IConfiguration _configuration;

        public BlobStorageHelper(IConfiguration configuration)
        {
            _configuration = configuration;

        }

        public async Task<string> GetImageUrl(string imageFile)
        {
            string accountName = this._configuration["StorageAccountName"]; 
            string key = this._configuration["StorageAccountKey"];

            var storageAccount = new CloudStorageAccount(new StorageCredentials(accountName, key), true);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("conflwtrator");
            await container.CreateIfNotExistsAsync();
            await container.SetPermissionsAsync(new BlobContainerPermissions()
            {
                PublicAccess = BlobContainerPublicAccessType.Blob
            });
            string imageName = null;
            if (imageFile != null)
            {
                imageName = Guid.NewGuid().ToString() + "-" + Path.GetFileName(imageFile);

                CloudBlockBlob cloudBlockBlob = container.GetBlockBlobReference(imageName);
                cloudBlockBlob.Properties.ContentType = "image/jpg";
                using (Stream file = File.OpenRead(imageFile))
                {
                    await cloudBlockBlob.UploadFromStreamAsync(file);
                }
                return cloudBlockBlob.Uri.ToString();
            }
            return imageName;
        }


        public async Task<string> GetFilePath(IFormFile file)
        {
            try
            {
                var uploads = Path.Combine(_configuration["BaseUri"], "uploads");
                var filePath = @"wwwroot/Uploads/" + GetUniqueFileName(file.FileName);
                using (var fileStream = System.IO.File.Create(filePath))
                {
                    await file.CopyToAsync(fileStream);
                }

                return filePath;
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        private string GetUniqueFileName(string fileName)
        {
            fileName = Path.GetFileName(fileName);
            return Path.GetFileNameWithoutExtension(fileName)
                      + "_"
                      + Guid.NewGuid().ToString().Substring(0, 4)
                      + Path.GetExtension(fileName);
        }
    }
}
