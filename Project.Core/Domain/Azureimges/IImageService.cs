using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Project.Core.Domain.Azureimges
{
    public interface IImageService
    {
        Task<string> UploadImageAsync(IFormFile file);
    }
}
