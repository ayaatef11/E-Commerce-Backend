using E_Commerce.Application.Common.DTOS.Responses;
using Microsoft.AspNetCore.WebUtilities;

namespace E_Commerce.Application.Interfaces.Common;
    internal interface IFileService
    {
    Task<FileUploadSummary> UploadFileAsync(Stream fileStream, string contentType);
    }

