namespace HRInfoAdvertisements.Application.DTOs.SystemSettings;

public class SystemSettingListResponse
{
    public List<SystemSettingListItemResponse> Items { get; set; } = new();

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalRecords { get; set; }

    public int TotalPages { get; set; }
}