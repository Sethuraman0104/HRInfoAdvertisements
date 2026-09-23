namespace HRInfoAdvertisements.Application.Profile.DTOs;

public class UserAddressResponse
{
    public long UserAddressID { get; set; }
    public long UserID { get; set; }

    public int? CountryID { get; set; }
    public string? CountryName { get; set; }

    public int? StateID { get; set; }
    public string? StateName { get; set; }

    public int? CityID { get; set; }
    public string? CityName { get; set; }

    public int? AreaID { get; set; }
    public string? AreaName { get; set; }

    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }

    public string? BuildingNo { get; set; }
    public string? RoadNo { get; set; }
    public string? BlockNo { get; set; }
    public string? PostalCode { get; set; }

    public bool IsPrimary { get; set; }

    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
}