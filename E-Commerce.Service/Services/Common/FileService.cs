using E_Commerce.Application.Common.DTOS.Responses;
using E_Commerce.Application.Interfaces.Common;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace E_Commerce.Application.Services.Common;
    internal class FileService:IFileService
    {
    private const string UploadDirectory = "FilesUploaded";
    private readonly List<string> _allowedExtensions = [".zip", ".bin", ".png", ".jpg"];

    public async Task<FileUploadSummary> UploadFileAsync(Stream fileStream, string contentType)
    {
        var fileCount = 0;
        long totalSizeInBytes = 0;

        var boundary=GetBoundary(MediaTypeHeaderValue.Parse(contentType));
        var multiPartReader = new MultipartReader(boundary, fileStream);
        var section=await multiPartReader.ReadNextSectionAsync();
        var filePaths=new List<string>();
        var notUploadedFiles=new List<string>();

        while(section is not null)
        {
var fileSection=section.AsFileSection();
            if(fileSection is not null)
            {
                var result = await SaveFileAsync(fileSection, filePaths, notUploadedFiles);
                if (result > 0)
                {
                    totalSizeInBytes += result;
                    fileCount++;
                }
            }
section=await multiPartReader.ReadNextSectionAsync();

        }
        return new FileUploadSummary { 
        TotalFilesUploaded=fileCount,
        TotalSizeUploaded=ConvertSizeToString(totalSizeInBytes),
        FilePaths=filePaths,
        NotUploadedFiles=notUploadedFiles
        };
    }

    private string GetBoundary(MediaTypeHeaderValue mediatypeHeaderValue)
    {
        var boundary = HeaderUtilities.RemoveQuotes(mediatypeHeaderValue.Boundary).Value;

        if (string.IsNullOrWhiteSpace(boundary))
        {
            throw new InvalidDataException("Missing content-type boundary.");
        }
        return boundary;
    }

    private async Task<long> SaveFileAsync(FileMultipartSection fileSection, List<string> filePaths, List<string> notUploadedFiles)
    {
        var extension = Path.GetExtension(fileSection.FileName);
        if (!_allowedExtensions.Contains(extension))
        {
            notUploadedFiles.Add(fileSection.FileName);
            return 0;
        }
        Directory.CreateDirectory(UploadDirectory);
        var filePath=Path.Combine(UploadDirectory, fileSection.FileName);
        await using var stream =new FileStream(filePath,FileMode.Create,FileAccess.Write,FileShare.None);
        await fileSection.FileStream.CopyToAsync(stream);
        filePaths.Add(GetFullFilePath(fileSection));
        return fileSection.FileStream.Length;
    }
    private string GetFullFilePath(FileMultipartSection fileSection)
    {
        return !string.IsNullOrEmpty(fileSection.FileName)
            ? Path.Combine(Directory.GetCurrentDirectory(), UploadDirectory,fileSection.FileName) :
            string.Empty;
    }
        private string ConvertSizeToString(long bytes)
    {
        var fileSize = new decimal(bytes);
        var kilobytes = new decimal(1024);
        var megabytes=new decimal(1024 * 1024);
        var gigabytes=new decimal(1024 * 1024 * 1024);

        return fileSize switch
        {
            _ when fileSize < kilobytes => "Less than 1KB",
            _ when fileSize < megabytes =>
              $"{Math.Round(fileSize / kilobytes, fileSize < 10 * kilobytes ? 2 : 1, MidpointRounding.AwayFromZero):##,###.##}KB",
            _ when fileSize < gigabytes =>
              $"{Math.Round(fileSize / megabytes, fileSize < 10 * megabytes ? 2 : 1, MidpointRounding.AwayFromZero):##,###.##}MB",
            _ when fileSize > gigabytes =>
              $"{Math.Round(fileSize / gigabytes, fileSize < 10 * gigabytes ? 2 : 1, MidpointRounding.AwayFromZero):##,###.##}GB",
            _ => "n/a"
        };
    }
    }

