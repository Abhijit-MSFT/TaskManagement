using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;

namespace TaskManagement.Helper
{
    public class BlobStorageHelper
    {
        const string accountName = "conflwtrator";
        const string key = "4humkgy0oBD5x4ywTTht0ps38yorIfrk74LSslH/JJNbGSNiaxDtoUAF2Wm/dNJ4XWTBn9AAhahrbOhiuD8U+w==";

        public static async Task<string> GetImageUrl(string imageFile)
        {
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
    }
}
