namespace HRInfoAdvertisements.Application.DTOs.AdvertisementTypeManagement;

public class AdvertisementTypeDetailResponse
{
    public int AdvertisementTypeID { get; set; }

    public string TypeName { get; set; } = string.Empty;

    public string? TypeNameAr { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public int AdvertisementCount { get; set; }
}