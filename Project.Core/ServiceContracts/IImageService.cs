using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Project.Core.ServiceContracts
{
    public interface IImageService
    {
        Task<string> UploadImageAsync(IFormFile file);
    }
}
