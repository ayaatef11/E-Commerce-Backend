using E_Commerce.Application.Common.Helpers.Compression;
using E_Commerce.Application.Interfaces.Common;

namespace E_Commerce.Application.Services.Common;
public class PhotoService(IHostEnvironment _environment, UserManager<AppUser> _userManager) : IPhotoService
{
    private async Task<AppUser> CheckUserFound(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || string.IsNullOrEmpty(user.PhotoPath))
        {
            throw new FileNotFoundException("User or photo not found");

        }
        return user;
    }
    public async Task<FileStreamResult> DownloadUserImageAsync(string userId)
    {
        var user = await CheckUserFound(userId);

        var filePath = Path.Combine(_environment.ContentRootPath, user.PhotoPath.TrimStart('/'));

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Photo file not found on server");
        }

        var contentType = GetContentType(filePath);

        var fileStream = File.OpenRead(filePath);

        return new FileStreamResult(fileStream, contentType)
        {
            FileDownloadName = Path.GetFileName(filePath)
        };
    }

    public async Task<FileStreamResult> DownloadFileByPathAsync(string filePath)
    {
        var safePath = Path.Combine(_environment.ContentRootPath, filePath.TrimStart('/'));

        if (!File.Exists(safePath))
        {
            throw new FileNotFoundException("File not found");
        }

        var contentType = GetContentType(safePath);
        var fileStream = File.OpenRead(safePath);

        return new FileStreamResult(fileStream, contentType)
        {
            FileDownloadName = Path.GetFileName(safePath)
        };
    }
    //public async Task<byte[]> GetImageAsync(string fileName)
    //{
    //    string filePath = "";
    //    if (_environment.IsDevelopment())
    //        filePath = Path.Combine(_environment.ContentRootPath, $"wwwroot{fileName}");
    //    else
    //    {
    //        //filePath = Path.Combine(_environment.ContentRootPath, $"wwwroot/wwwroot{fileName}");
    //        //filePath = $"/wwwroot/wwwroot{fileName}";
    //        //filePath = Path.Combine(_environment.ContentRootPath, $"{fileName}");
    //        filePath = Path.Combine(_environment.ContentRootPath, $"wwwroot{fileName}");

    //    }
    //    if (!File.Exists(filePath))
    //        return null;

    //    return await File.ReadAllBytesAsync(filePath);
    //}

    public string GetContentType(string path)
    {
        var extension = Path.GetExtension(path).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            _ => "application/octet-stream"
        };
    }

    public async Task<string> UploadImageAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("No file uploaded");

        //var uploadsFolder = Path.Combine(_environment.ContentRootPath, "wwwroot/uploads", "users");
        var uploadsFolder = Path.Combine(_environment.ContentRootPath, "wwwroot");

        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"{uniqueFileName}";
    }

    public bool DeleteImage(string photoPath)
    {
        if (string.IsNullOrEmpty(photoPath)) return false;

        var filePath = Path.Combine(_environment.ContentRootPath, photoPath.TrimStart('/'));

        if (!File.Exists(filePath)) return false;

        File.Delete(filePath);
        return true;
    }

    public async Task<string> SaveCompressedImageAsync(IFormFile file, string userId)
    {
        var user = await CheckUserFound(userId);

        byte[] compressedImage = await ImageCompression.CompressImageAsync(file);

        string fileName = $"{Guid.NewGuid()}.jpg";
        var filePath = Path.Combine(_environment.ContentRootPath, user.PhotoPath.TrimStart('/'));

        await File.WriteAllBytesAsync(filePath, compressedImage);
        return fileName;
    }

    public async Task<byte[]> GetCompressedImageAsync(string fileName, string userId)
    {
        var user = await CheckUserFound(userId);
        var filePath = Path.Combine(_environment.ContentRootPath, user.PhotoPath.TrimStart('/'));
        return await File.ReadAllBytesAsync(filePath);
    }

}


