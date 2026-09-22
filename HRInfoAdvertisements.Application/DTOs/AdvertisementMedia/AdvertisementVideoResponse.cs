namespace HRInfoAdvertisements.Application.DTOs.AdvertisementMedia;

public class AdvertisementVideoResponse
{
    public long AdvertisementVideoID { get; set; }

    public long AdvertisementID { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string FileURL { get; set; } = string.Empty;

    public string? ContentType { get; set; }

    public long? FileSize { get; set; }

    public int DisplayOrder { get; set; }

    public DateTime CreatedDate { get; set; }
}