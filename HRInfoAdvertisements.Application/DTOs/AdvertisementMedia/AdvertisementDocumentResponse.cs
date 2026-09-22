namespace HRInfoAdvertisements.Application.DTOs.AdvertisementMedia;

public class AdvertisementDocumentResponse
{
    public long AdvertisementDocumentID { get; set; }

    public long AdvertisementID { get; set; }

    public string DocumentName { get; set; } = string.Empty;

    public string FileURL { get; set; } = string.Empty;

    public string? ContentType { get; set; }

    public long? FileSize { get; set; }

    public DateTime CreatedDate { get; set; }
}