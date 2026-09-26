namespace HRInfoAdvertisements.Domain.Entities;

public class AdvertisementImage
{
    public long AdvertisementImageID { get; set; }

    public long AdvertisementID { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string S3Key { get; set; } = string.Empty;

    public string? FileURL { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsPrimary { get; set; }

    public long? FileSize { get; set; }

    public int? Width { get; set; }

    public int? Height { get; set; }

    public DateTime CreatedDate { get; set; }

    public Advertisement Advertisement { get; set; } = null!;
}