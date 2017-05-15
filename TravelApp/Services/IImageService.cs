using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TravelApp.Services
{
    public interface IImageService
    {
        Task<string> SaveImage(IFormFile image, string uploadDir, int length = 25);
        bool SaveMinifiedImage(string imageName, string pathToImage, string uploadDir, int size = 150, int quality = 75);
    }
}
