using System.Threading.Tasks;

namespace FormBuilder.Core.Interfaces
{
    public interface IDropboxService
    {
        Task<string> UploadJsonAsync(object data, string fileName);
        Task<bool> CreateFolderIfNotExistsAsync(string folderPath);
        Task<string> GetSharedLinkAsync(string filePath);
    }
}