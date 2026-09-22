namespace HRInfoAdvertisements.Domain.Entities;

public class Country
{
    public int CountryID { get; set; }

    public string CountryCode { get; set; } = string.Empty;

    public string CountryName { get; set; } = string.Empty;

    public string? CountryNameAr { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<State> States { get; set; } = new List<State>();

    public ICollection<City> Cities { get; set; } = new List<City>();

    public ICollection<Area> Areas { get; set; } = new List<Area>();

    public ICollection<Advertisement> Advertisements { get; set; }
        = new List<Advertisement>();
}