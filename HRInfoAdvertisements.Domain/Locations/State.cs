namespace HRInfoAdvertisements.Domain.Entities;

public class State
{
    public int StateID { get; set; }

    public int CountryID { get; set; }

    public string StateName { get; set; } = string.Empty;

    public string? StateNameAr { get; set; }

    public bool IsActive { get; set; } = true;

    public Country Country { get; set; } = null!;

    public ICollection<City> Cities { get; set; }
        = new List<City>();

    public ICollection<Area> Areas { get; set; }
        = new List<Area>();

    public ICollection<Advertisement> Advertisements { get; set; }
        = new List<Advertisement>();
}