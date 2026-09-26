namespace HRInfoAdvertisements.Domain.Entities;

public class AdvertisementVideo
{
    public long AdvertisementVideoID { get; set; }

    public long AdvertisementID { get; set; }

    public string? FileName { get; set; }

    public string? S3Key { get; set; }

    public string? VideoURL { get; set; }

    public string? ThumbnailURL { get; set; }

    public int? DurationSeconds { get; set; }

    public int DisplayOrder { get; set; }

    public DateTime CreatedDate { get; set; }

    public Advertisement Advertisement { get; set; } = null!;
}