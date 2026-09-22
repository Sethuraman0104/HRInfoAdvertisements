namespace HRInfoAdvertisements.Domain.Entities;

public class Location
{
    public long LocationID { get; set; }

    public string LocationName { get; set; } = string.Empty;

    public string? LocationNameAr { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public string? AddressLine { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedDate { get; set; }
}