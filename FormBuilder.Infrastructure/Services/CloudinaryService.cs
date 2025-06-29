using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using FormBuilder.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace FormBuilder.Infrastructure.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        private readonly IConfiguration _configuration;

        public CloudinaryService(IConfiguration configuration)
        {
            _configuration = configuration;
            
            var account = new Account(
                configuration["Cloudinary:CloudName"],
                configuration["Cloudinary:ApiKey"],
                configuration["Cloudinary:ApiSecret"]
            );
            
            _cloudinary = new Cloudinary(account);
        }

        public async Task<string> UploadImageAsync(Stream fileStream, string fileName)
        {
            try
            {
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(fileName, fileStream),
                    Transformation = new Transformation()
                        .Width(800)
                        .Height(600)
                        .Crop("limit")
                        .Quality("auto")
                        .FetchFormat("auto")
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                
                if (uploadResult.Error != null)
                {
                    throw new Exception($"Cloudinary upload failed: {uploadResult.Error.Message}");
                }

                return uploadResult.SecureUrl.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("Image upload failed", ex);
            }
        }

        public async Task<bool> DeleteImageAsync(string publicId)
        {
            try
            {
                if (string.IsNullOrEmpty(publicId))
                    return true;

                var deleteParams = new DeletionParams(publicId);
                var result = await _cloudinary.DestroyAsync(deleteParams);
                
                return result.Result == "ok";
            }
            catch
            {
                return false;
            }
        }

        public string GetImageUrl(string publicId, int width, int height)
        {
            if (string.IsNullOrEmpty(publicId))
                return "/images/placeholder.jpg";

            var transformation = new Transformation()
                .Width(width)
                .Height(height)
                .Crop("fill")
                .Quality("auto")
                .FetchFormat("auto");

            return _cloudinary.Api.UrlImgUp
                .Transform(transformation)
                .BuildUrl(publicId);
        }
    }
}