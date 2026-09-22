namespace HRInfoAdvertisements.Domain.Entities;

public class AdvertisementType
{
    public int AdvertisementTypeID { get; set; }

    public string TypeName { get; set; } = string.Empty;

    public string? TypeNameAr { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Advertisement> Advertisements { get; set; } = new List<Advertisement>();
}