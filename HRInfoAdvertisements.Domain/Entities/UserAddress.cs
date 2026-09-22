namespace HRInfoAdvertisements.Domain.Entities;

public class UserAddress
{
    public long UserAddressID { get; set; }

    public long UserID { get; set; }

    public int? CountryID { get; set; }

    public int? StateID { get; set; }

    public int? CityID { get; set; }

    public int? AreaID { get; set; }

    public string? AddressLine1 { get; set; }

    public string? AddressLine2 { get; set; }

    public string? BuildingNo { get; set; }

    public string? RoadNo { get; set; }

    public string? BlockNo { get; set; }

    public string? PostalCode { get; set; }

    public bool IsPrimary { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public User User { get; set; } = null!;

    public Country? Country { get; set; }

    public State? State { get; set; }

    public City? City { get; set; }

    public Area? Area { get; set; }
}