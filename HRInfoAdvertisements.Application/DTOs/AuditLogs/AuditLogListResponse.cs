namespace HRInfoAdvertisements.Application.DTOs.AuditLogs;

public class AuditLogListResponse
{
    public List<AuditLogListItemResponse> Items { get; set; } = new();

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalRecords { get; set; }

    public int TotalPages { get; set; }
}