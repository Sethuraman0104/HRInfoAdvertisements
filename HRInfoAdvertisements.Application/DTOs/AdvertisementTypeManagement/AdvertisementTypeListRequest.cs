namespace HRInfoAdvertisements.Application.DTOs.AdvertisementTypeManagement;

public class AdvertisementTypeListRequest
{
    public string? Search { get; set; }

    public bool? IsActive { get; set; }

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}