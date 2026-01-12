using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;

using System.IO.Compression;

namespace E_Commerce.Application.Common.Helpers.Compression;
public class ImageCompression
{

    public static async Task<byte[]> CompressImageAsync(IFormFile file)
    {
        using var image = await Image.LoadAsync(file.OpenReadStream());

        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Mode = ResizeMode.Max,
            Size = new Size(800, 800)
        }));

        using var ms = new MemoryStream();
        await image.SaveAsJpegAsync(ms, new JpegEncoder { Quality = 70 });

        return ms.ToArray();
    }


    public static void ZipPhotos(List<string> filePaths, string zipPath)//to a folder
    {
        if (File.Exists(zipPath))
            File.Delete(zipPath);

        using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
        {
            foreach (var file in filePaths)
            {
                zip.CreateEntryFromFile(file, Path.GetFileName(file));
            }
        }
    }
}

