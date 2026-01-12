using E_Commerce.Application.Common.Helpers.Compression;
using E_Commerce.Application.Interfaces.Common;
using E_Commerce.Core.Shared.Utilties.Identity;
using Microsoft.AspNetCore.StaticFiles;

namespace E_Commerce.Controllers;
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = $"{Roles.User},{Roles.Admin}")]
[EnableRateLimiting("FixedWindowPolicy")]
public class PhotosController(IPhotoService _photoService, IWebHostEnvironment _env) : ControllerBase
{

    [HttpPost("upload")]
    public async Task<ActionResult<string>> UploadImageAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded or file is empty");

        try
        {
            var uploadsFolder = _env.WebRootPath;
            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return a relative path (or adjust based on how you serve static files)
            var relativePath = Path.Combine("uploads", uniqueFileName).Replace("\\", "/");

            //var photoPath = await _photoService.UploadImageAsync(file);
            return Ok(new { path = uniqueFileName });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "An error occurred while uploading the image");
        }
    }

    [HttpGet("download-by-path")]
    public async Task<IActionResult> DownloadImageByPath(string filePath)
    {
        try
        {
            var result = await _photoService.DownloadFileByPathAsync(filePath);
            return result;
        }
        catch (FileNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    [HttpDelete("delete")]
    public ActionResult<bool> DeleteImage(string photoPath)
    {
        if (string.IsNullOrEmpty(photoPath))
            return BadRequest("Photo path is required");

        var deleted = _photoService.DeleteImage(photoPath);
        if (!deleted)
            return NotFound("Photo not found");

        return Ok(true);
    }

    [HttpPut("update")]
    public async Task<ActionResult<string>> UpdatePhoto( [FromQuery] string oldPhotoPath,IFormFile newFile)
   {
        if (string.IsNullOrEmpty(oldPhotoPath))
            return BadRequest("Old photo path is required");

        if (newFile == null || newFile.Length == 0)
            return BadRequest("New file is required");

        try
        {
            var deleted = _photoService.DeleteImage(oldPhotoPath);
            if (!deleted)
                return NotFound("Old photo not found");

            // 3. رفع الصورة الجديدة
            var newPhotoPath = await _photoService.UploadImageAsync(newFile);

            return Ok(new
            {
                message = "Photo updated successfully",
                newPath = newPhotoPath
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred while updating photo: {ex.Message}");
        }
    }

    //[HttpGet("view/{fileName}")]
    //public async Task<IActionResult> ViewImage(string fileName)
    //{
    //    try
    //    {
    //        var decodedFileName = Uri.UnescapeDataString(fileName);
    //        var imageData = await _photoService.GetImageAsync(decodedFileName);
    //        //var imageData = await _photoService.GetImageAsync(fileName);

    //        if (imageData == null || imageData.Length==0) return NotFound("Image not found");
    //        var contentType = _photoService.GetContentType(decodedFileName) ?? "image/jpeg" ;

    //        return File(imageData, contentType);
    //    }
    //    catch (Exception ex)
    //    {
    //        return StatusCode(500, $"Error retrieving image: {ex.Message}");
    //    }
    //}

    [HttpGet("view/{*imagePath}")]
    public IActionResult ViewImage(string imagePath)
    {
        try
        {
            var fullPath = Path.Combine(_env.WebRootPath, imagePath);

            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound("is not found");
            }
            var contentType = GetContentType(fullPath);

            return PhysicalFile(fullPath, contentType);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    private string GetContentType(string path)
    {
        var provider = new FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(path, out var contentType))
        {
            contentType = "application/octet-stream";
        }
        return contentType;
    }

    [HttpPost("uploadCompressed")]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Invalid file");
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return BadRequest("user isn't found");
        try
        {
            string savedFileName = await _photoService.SaveCompressedImageAsync(file, userId);
            return Ok(new { FileName = savedFileName });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("download-zip")]
    public IActionResult DownloadZip([FromQuery] List<string> filePaths)
    {
        if (filePaths == null || !filePaths.Any())
            return BadRequest("No files specified");

        try
        {
            string tempZipPath = Path.GetTempFileName();
            ImageCompression.ZipPhotos(filePaths, tempZipPath);
            var fileStream = new FileStream(tempZipPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.DeleteOnClose);
            return File(fileStream, "application/zip", "photos.zip");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error generating ZIP: {ex.Message}");
        }
    }

    [HttpGet("{fileName}")]
    public async Task<IActionResult> GetImage(string fileName)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return BadRequest("user isn't found");
        try
        {
            byte[] imageData = await _photoService.GetCompressedImageAsync(fileName, userId);
            return File(imageData, "image/jpeg");
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}

