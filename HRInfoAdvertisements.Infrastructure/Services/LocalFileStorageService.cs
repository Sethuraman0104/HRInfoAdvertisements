using HRInfoAdvertisements.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
namespace HRInfoAdvertisements.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LocalFileStorageService(
        IWebHostEnvironment environment,
        IHttpContextAccessor httpContextAccessor)
    {
        _environment = environment;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<StoredFileResult> SaveFileAsync(
        Stream fileStream,
        string originalFileName,
        string folder,
        string contentType)
    {
        var uploadsRoot = Path.Combine(
            _environment.WebRootPath ?? Path.Combine(
                _environment.ContentRootPath,
                "wwwroot"),
            "uploads",
            "advertisements",
            folder);

        Directory.CreateDirectory(uploadsRoot);

        var extension = Path.GetExtension(originalFileName);

        var generatedFileName =
            $"{Guid.NewGuid():N}{extension}";

        var physicalPath =
            Path.Combine(
                uploadsRoot,
                generatedFileName);

        await using var outputStream =
            new FileStream(
                physicalPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None);

        await fileStream.CopyToAsync(outputStream);

        var fileSize = outputStream.Length;

        var request =
            _httpContextAccessor.HttpContext?.Request;

        var baseUrl =
            $"{request?.Scheme}://{request?.Host}";

        var fileUrl =
            $"{baseUrl}/uploads/advertisements/{folder}/{generatedFileName}";

        return new StoredFileResult
        {
            FileName = generatedFileName,
            OriginalFileName = originalFileName,
            FileUrl = fileUrl,
            FileSize = fileSize,
            ContentType = contentType
        };
    }

    public Task DeleteFileAsync(string fileUrl)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
            return Task.CompletedTask;

        var uri = new Uri(fileUrl);

        var relativePath =
            uri.AbsolutePath.TrimStart('/')
                .Replace(
                    '/',
                    Path.DirectorySeparatorChar);

        var wwwRoot =
            _environment.WebRootPath ??
            Path.Combine(
                _environment.ContentRootPath,
                "wwwroot");

        var physicalPath =
            Path.Combine(
                wwwRoot,
                relativePath.Replace(
                    "uploads" +
                    Path.DirectorySeparatorChar,
                    "uploads" +
                    Path.DirectorySeparatorChar));

        if (File.Exists(physicalPath))
        {
            File.Delete(physicalPath);
        }

        return Task.CompletedTask;
    }
}