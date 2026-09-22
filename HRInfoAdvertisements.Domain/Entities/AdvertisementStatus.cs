namespace HRInfoAdvertisements.Domain.Entities;

public class AdvertisementStatus
{
    public int StatusID { get; set; }

    public string StatusCode { get; set; } = string.Empty;

    public string StatusName { get; set; } = string.Empty;

    public string? StatusNameAr { get; set; }

    public string? Description { get; set; }

    public bool IsPublicStatus { get; set; }

    public bool IsActive { get; set; } = true;

    public int DisplayOrder { get; set; }

    public ICollection<Advertisement> Advertisements { get; set; } = new List<Advertisement>();
}