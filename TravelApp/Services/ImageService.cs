using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TravelApp.Services
{
    public class ImageService : IImageService
    {
        IToolsService _tools = new ToolsService();

        public async Task<string> SaveImage(IFormFile image, string uploadDir, int length = 25)
        {
            if (image == null)
                return "error";

            var allowedExtensions = new[] { ".jpeg", ".jpg", ".png" };
            if (!allowedExtensions.Contains(Path.GetExtension(image.FileName)))
                return "error";

            var allowedContentTypes = new[] { "image/jpeg", "image/png" };
            if (!allowedContentTypes.Contains(image.ContentType.ToLower()))
                return "error";
            
            var uploadFile = "";
            var uploadCounter = 0;
            do
            {
                uploadCounter++;
                uploadFile = _tools.RandomString(length) + Path.GetExtension(image.FileName);
            } while (uploadCounter < 10 && System.IO.File.Exists(Path.Combine(uploadDir, uploadFile)));

            using (var stream = new FileStream(Path.Combine(uploadDir, uploadFile), FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            return uploadFile;
        }

        public bool SaveMinifiedImage(string imageName, string pathToImage, string uploadDir, int size = 250, int quality = 75)
        {
            if (!File.Exists(Path.Combine(pathToImage, imageName))) return false;

            using (var image = new Bitmap(System.Drawing.Image.FromFile(Path.Combine(pathToImage, imageName))))
            {
                int width, height;
                if (image.Width > image.Height)
                {
                    width = size;
                    height = Convert.ToInt32(image.Height * size / (double)image.Width);
                }
                else
                {
                    width = Convert.ToInt32(image.Width * size / (double)image.Height);
                    height = size;
                }
                var resized = new Bitmap(width, height);
                using (var graphics = Graphics.FromImage(resized))
                {
                    graphics.CompositingQuality = CompositingQuality.HighSpeed;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.DrawImage(image, 0, 0, width, height);
                    using (var output = File.Open(Path.Combine(uploadDir, imageName), FileMode.Create))
                    {
                        var qualityParamId = Encoder.Quality;
                        var encoderParameters = new EncoderParameters(1);
                        encoderParameters.Param[0] = new EncoderParameter(qualityParamId, quality);
                        var codec = ImageCodecInfo.GetImageDecoders()
                            .FirstOrDefault(c => c.FormatID == ImageFormat.Jpeg.Guid);
                        resized.Save(output, codec, encoderParameters);
                    }
                }
            }

            return true;
        }
    }
}
