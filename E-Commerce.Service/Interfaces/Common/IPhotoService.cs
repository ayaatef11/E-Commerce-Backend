namespace E_Commerce.Application.Interfaces.Common;
public interface IPhotoService
{
    Task<string> UploadImageAsync(IFormFile file);
    Task<FileStreamResult> DownloadUserImageAsync(string userId);
    Task<FileStreamResult> DownloadFileByPathAsync(string filePath);
    bool DeleteImage(string PhotoPath);
    //Task<byte[]> GetImageAsync(string fileName);
    string GetContentType(string path);
    Task<string> SaveCompressedImageAsync(IFormFile file, string userID);
    Task<byte[]> GetCompressedImageAsync(string fileName, string userId);
}

