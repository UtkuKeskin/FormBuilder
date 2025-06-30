using System.IO;
using System.Threading.Tasks;

namespace FormBuilder.Core.Interfaces
{
    public interface ICloudinaryService
    {
        Task<string> UploadImageAsync(Stream fileStream, string fileName);
        Task<bool> DeleteImageAsync(string publicId);
        string GetImageUrl(string publicId, int width, int height);
    }
}