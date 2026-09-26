namespace HRInfoAdvertisements.Domain.Entities;

public class Area
{
    public int AreaID { get; set; }

    public int CityID { get; set; }

    public string AreaName { get; set; } = string.Empty;

    public string? AreaNameAr { get; set; }

    public bool IsActive { get; set; } = true;

    public City City { get; set; } = null!;

    public ICollection<Advertisement> Advertisements { get; set; }
        = new List<Advertisement>();
}