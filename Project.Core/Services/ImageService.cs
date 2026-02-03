using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Project.Core.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Text;

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
            // 1. التحقق إن فيه ملف أصلاً
            if (file == null || file.Length == 0)
                throw new ArgumentNullException();
            // 2. قراءة الكونكشن من الإعدادات
            var connectionString = _configuration["AzureStorage"];

            // 3. الاتصال بالكونتينر اللي اسمه "images"
            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient("images");

            // 4. تغيير اسم الملف عشان ميتكررش (UUID)
            // مثلاً: ahmed.jpg -> 550e8400-e29b-41d4-a716-446655440000.jpg
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var blobClient = containerClient.GetBlobClient(fileName);

            // 5. رفع الملف فعلياً
            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });
            }

            // 6. إرجاع الرابط المباشر للصورة
            return blobClient.Uri.ToString();
        }
    }
}
