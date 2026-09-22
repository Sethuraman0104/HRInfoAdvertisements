namespace HRInfoAdvertisements.Application.DTOs.Advertisements;

public class AdvertisementListResponse
{
    public List<AdvertisementResponse> Items { get; set; } = new();

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalRecords { get; set; }

    public int TotalPages { get; set; }
}