namespace HRInfoAdvertisements.Application.DTOs.Reports;

public class ReportListResponse
{
    public List<ReportListItemResponse> Items { get; set; } = new();

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalRecords { get; set; }

    public int TotalPages { get; set; }
}