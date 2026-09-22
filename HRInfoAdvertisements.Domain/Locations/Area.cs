namespace HRInfoAdvertisements.Domain.Entities;

public class Area
{
    public int AreaID { get; set; }

    public int CountryID { get; set; }

    public int? StateID { get; set; }

    public int? CityID { get; set; }

    public string AreaName { get; set; } = string.Empty;

    public string? AreaNameAr { get; set; }

    public bool IsActive { get; set; } = true;

    public Country Country { get; set; } = null!;

    public State? State { get; set; }

    public City? City { get; set; }

    public ICollection<Advertisement> Advertisements { get; set; }
        = new List<Advertisement>();
}