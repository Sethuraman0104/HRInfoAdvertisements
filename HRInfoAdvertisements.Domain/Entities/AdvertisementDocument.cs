namespace HRInfoAdvertisements.Domain.Entities;

public class AdvertisementDocument
{
    public long AdvertisementDocumentID { get; set; }

    public long AdvertisementID { get; set; }

    public string DocumentName { get; set; } = string.Empty;

    public string FileURL { get; set; } = string.Empty;

    public string? StorageKey { get; set; }

    public string? ContentType { get; set; }

    public long? FileSize { get; set; }

    public DateTime CreatedDate { get; set; }

    public Advertisement Advertisement { get; set; } = null!;
}