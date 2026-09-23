namespace HRInfoAdvertisements.Application.Profile.DTOs;

public class CreateUserAddressRequest
{
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
}