namespace HRInfoAdvertisements.Domain.Entities;

public class City
{
    public int CityID { get; set; }

    public int? StateID { get; set; }

    public string CityName { get; set; } = string.Empty;

    public string? CityNameAr { get; set; }

    public bool IsActive { get; set; } = true;

    public State? State { get; set; }

    public ICollection<Area> Areas { get; set; }
        = new List<Area>();

    public ICollection<Advertisement> Advertisements { get; set; }
        = new List<Advertisement>();
}