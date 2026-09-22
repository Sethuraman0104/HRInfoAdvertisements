namespace HRInfoAdvertisements.Application.DTOs.AdvertisementMedia;

public class AdvertisementImageResponse
{
    public long AdvertisementImageID { get; set; }

    public long AdvertisementID { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string FileURL { get; set; } = string.Empty;

    public string? ContentType { get; set; }

    public long? FileSize { get; set; }

    public bool IsPrimary { get; set; }

    public int DisplayOrder { get; set; }

    public DateTime CreatedDate { get; set; }
}