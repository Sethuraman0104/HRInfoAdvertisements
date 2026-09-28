namespace HRInfoAdvertisements.Application.DTOs.AdvertisementTypeManagement;

public class AdvertisementTypeListResponse
{
    public List<AdvertisementTypeListItemResponse> Items { get; set; } = new();

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalRecords { get; set; }

    public int TotalPages { get; set; }
}