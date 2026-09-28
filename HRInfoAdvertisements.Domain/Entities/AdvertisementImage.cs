namespace HRInfoAdvertisements.Domain.Entities;

public class AdvertisementImage
{
    public long AdvertisementImageID { get; set; }

    public long AdvertisementID { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string FileURL { get; set; } = string.Empty;

    public string? StorageKey { get; set; }

    public string? ContentType { get; set; }

    public long? FileSize { get; set; }

    public bool IsPrimary { get; set; }

    public int DisplayOrder { get; set; }

    public DateTime CreatedDate { get; set; }

    public Advertisement Advertisement { get; set; } = null!;
}