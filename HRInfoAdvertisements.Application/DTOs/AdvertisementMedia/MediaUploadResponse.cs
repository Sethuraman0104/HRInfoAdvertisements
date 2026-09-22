namespace HRInfoAdvertisements.Application.DTOs.AdvertisementMedia;

public class MediaUploadResponse
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string OriginalFileName { get; set; } = string.Empty;

    public string FileUrl { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public string ContentType { get; set; } = string.Empty;
}