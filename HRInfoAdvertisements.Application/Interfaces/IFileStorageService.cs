namespace HRInfoAdvertisements.Application.Interfaces;

public interface IFileStorageService
{
    Task<StoredFileResult> SaveFileAsync(
        Stream fileStream,
        string originalFileName,
        string folder,
        string contentType);

    Task DeleteFileAsync(string fileUrl);
}

public class StoredFileResult
{
    public string FileName { get; set; } = string.Empty;

    public string OriginalFileName { get; set; } = string.Empty;

    public string FileUrl { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public string ContentType { get; set; } = string.Empty;
}