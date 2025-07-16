using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Dropbox.Api;
using Dropbox.Api.Files;
using FormBuilder.Core.Interfaces;
using FormBuilder.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace FormBuilder.Infrastructure.Services
{
    public class DropboxService : IDropboxService
    {
        private readonly ILogger<DropboxService> _logger;
        private readonly DropboxConfig _config;
        private DropboxClient _client;

        public DropboxService(
            IOptions<DropboxConfig> config,
            ILogger<DropboxService> logger)
        {
            _logger = logger;
            _config = config.Value;
            InitializeClient();
        }

        private void InitializeClient()
        {
            try
            {
                // Use the access token directly if refresh token is not working
                if (!string.IsNullOrEmpty(_config.RefreshToken))
                {
                    // For now, use RefreshToken as AccessToken
                    // This is temporary - in production you'd implement proper OAuth2 flow
                    _client = new DropboxClient(_config.RefreshToken);
                }
                else
                {
                    throw new InvalidOperationException("Dropbox access token is not configured");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Dropbox client");
                throw;
            }
        }

        public async Task<string> UploadJsonAsync(object data, string fileName)
        {
            try
            {
                var json = JsonConvert.SerializeObject(data, Formatting.Indented);
                var bytes = Encoding.UTF8.GetBytes(json);
                
                // Ensure path starts with /
                if (!fileName.StartsWith("/"))
                    fileName = "/" + fileName;
                
                using (var stream = new MemoryStream(bytes))
                {
                    var result = await _client.Files.UploadAsync(
                        fileName,
                        WriteMode.Add.Instance,
                        body: stream);
                    
                    _logger.LogInformation("File uploaded to Dropbox: {Path}", result.PathDisplay);
                    return result.PathDisplay;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file to Dropbox: {FileName}", fileName);
                throw;
            }
        }

        public async Task<bool> CreateFolderIfNotExistsAsync(string folderPath)
        {
            try
            {
                // Ensure path starts with /
                if (!folderPath.StartsWith("/"))
                    folderPath = "/" + folderPath;
                
                try
                {
                    // Check if folder exists
                    await _client.Files.GetMetadataAsync(folderPath);
                    return true;
                }
                catch (ApiException<GetMetadataError> ex) when (ex.ErrorResponse.IsPath && ex.ErrorResponse.AsPath.Value.IsNotFound)
                {
                    // Folder doesn't exist, create it
                    await _client.Files.CreateFolderV2Async(folderPath);
                    _logger.LogInformation("Created folder: {Path}", folderPath);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating folder in Dropbox: {FolderPath}", folderPath);
                return false;
            }
        }

        public async Task<string> GetSharedLinkAsync(string filePath)
        {
            try
            {
                var sharedLink = await _client.Sharing.CreateSharedLinkWithSettingsAsync(filePath);
                return sharedLink.Url;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating shared link");
                return null;
            }
        }
    }
}