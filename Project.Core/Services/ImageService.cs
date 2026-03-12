using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Project.Core.ServiceContracts;

namespace Project.Core.Services
{
    public class ImageService : IImageService
    {
        private readonly IConfiguration _configuration;

        public ImageService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            // 1. التحقق إن فيه ملف
            if (file == null || file.Length == 0)
                throw new ArgumentNullException(nameof(file), "File is null or empty.");

            // 2. التحقق من الـ Connection String
            var connectionString = _configuration["AzureStorage"];
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException("Azure Storage connection string is missing.");

            // 3. الاتصال بالكونتينر
            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient("osamaimages99");

            // ✅ لو الكونتينر مش موجود هيتعمل أوتوماتيك بصلاحية Public
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            // 4. تغيير اسم الملف عشان ميتكررش
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var blobClient = containerClient.GetBlobClient(fileName);

            // 5. رفع الملف
            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });
            }

            // 6. إرجاع الرابط
            return blobClient.Uri.ToString();
        }
    }
}