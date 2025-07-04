using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using FormBuilder.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FormBuilder.Infrastructure.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<CloudinaryService> _logger;
        private const int MAX_FILE_SIZE_MB = 5;

        public CloudinaryService(IConfiguration configuration, ILogger<CloudinaryService> logger)
        {
            _logger = logger;
            _cloudinary = InitializeCloudinary(configuration);
        }

        public async Task<string> UploadImageAsync(Stream fileStream, string fileName)
        {
            try
            {
                ValidateFileSize(fileStream);
                var uploadParams = CreateUploadParams(fileStream, fileName);
                var uploadResult = await PerformUpload(uploadParams);
                return uploadResult.SecureUrl.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Image upload failed");
                throw;
            }
        }

        public async Task<bool> DeleteImageAsync(string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl)) return true;
                
                var publicId = ExtractPublicId(imageUrl);
                if (string.IsNullOrEmpty(publicId)) return false;
                
                return await DeleteFromCloudinary(publicId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image");
                return false;
            }
        }

        public string GetImageUrl(string publicId, int width, int height)
        {
            if (string.IsNullOrEmpty(publicId))
                return "/images/placeholder.jpg";

            var transformation = CreateTransformation(width, height);
            return _cloudinary.Api.UrlImgUp
                .Transform(transformation)
                .BuildUrl(publicId);
        }
        private Cloudinary InitializeCloudinary(IConfiguration configuration)
        {
            var credentials = GetCloudinaryCredentials(configuration);
            var account = new Account(
                credentials.CloudName, 
                credentials.ApiKey, 
                credentials.ApiSecret);
            return new Cloudinary(account);
        }

        private (string CloudName, string ApiKey, string ApiSecret) GetCloudinaryCredentials(
            IConfiguration configuration)
        {
            var cloudName = configuration["Cloudinary:CloudName"];
            var apiKey = configuration["Cloudinary:ApiKey"];
            var apiSecret = configuration["Cloudinary:ApiSecret"];
            
            if (string.IsNullOrEmpty(cloudName) || 
                string.IsNullOrEmpty(apiKey) || 
                string.IsNullOrEmpty(apiSecret))
            {
                throw new InvalidOperationException(
                    "Cloudinary credentials are not configured");
            }
            
            return (cloudName, apiKey, apiSecret);
        }

        private void ValidateFileSize(Stream fileStream)
        {
            var maxSizeBytes = MAX_FILE_SIZE_MB * 1024 * 1024;
            if (fileStream.Length > maxSizeBytes)
            {
                throw new InvalidOperationException(
                    $"File size exceeds {MAX_FILE_SIZE_MB}MB limit");
            }
        }

        private ImageUploadParams CreateUploadParams(Stream fileStream, string fileName)
        {
            return new ImageUploadParams
            {
                File = new FileDescription(fileName, fileStream),
                Folder = "formbuilder",
                Transformation = CreateUploadTransformation(),
                UseFilename = true,
                UniqueFilename = true,
                Overwrite = false
            };
        }

        private Transformation CreateUploadTransformation()
        {
            return new Transformation()
                .Width(1200)
                .Height(800)
                .Crop("limit")
                .Quality("auto:good")
                .FetchFormat("auto");
        }

        private Transformation CreateTransformation(int width, int height)
        {
            return new Transformation()
                .Width(width)
                .Height(height)
                .Crop("fill")
                .Gravity("auto")
                .Quality("auto")
                .FetchFormat("auto");
        }

        private async Task<ImageUploadResult> PerformUpload(ImageUploadParams uploadParams)
        {
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            
            if (uploadResult.Error != null)
            {
                _logger.LogError("Cloudinary error: {Error}", uploadResult.Error.Message);
                throw new Exception($"Upload failed: {uploadResult.Error.Message}");
            }

            _logger.LogInformation("Image uploaded: {PublicId}", uploadResult.PublicId);
            return uploadResult;
        }

        private string ExtractPublicId(string imageUrl)
        {
            try
            {
                var uri = new Uri(imageUrl);
                var publicId = GetPublicIdFromPath(uri.AbsolutePath);
                return publicId;
            }
            catch
            {
                return null;
            }
        }

        private string GetPublicIdFromPath(string path)
        {
            var parts = path.Split('/');
            var versionIndex = FindVersionIndex(parts);
            
            if (versionIndex < 0) return null;
            
            return ExtractPublicIdFromParts(parts, versionIndex);
        }

        private int FindVersionIndex(string[] parts)
        {
            return Array.FindIndex(parts, p => 
                p.StartsWith("v") && p.Length > 1 && char.IsDigit(p[1]));
        }

        private string ExtractPublicIdFromParts(string[] parts, int versionIndex)
        {
            if (versionIndex >= parts.Length - 1) return null;
            
            var publicIdParts = parts.Skip(versionIndex + 1);
            var fullPublicId = string.Join("/", publicIdParts);
            
            return RemoveFileExtension(fullPublicId);
        }

        private string RemoveFileExtension(string fullPublicId)
        {
            var lastDotIndex = fullPublicId.LastIndexOf('.');
            if (lastDotIndex > 0)
            {
                return fullPublicId.Substring(0, lastDotIndex);
            }
            return fullPublicId;
        }

        private async Task<bool> DeleteFromCloudinary(string publicId)
        {
            var deleteParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deleteParams);
            return result.Result == "ok";
        }
    }
}