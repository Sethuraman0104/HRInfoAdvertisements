namespace HRInfoAdvertisements.Application.DTOs.AuditLogs;

public class AuditLogListRequest
{
    public long? UserID { get; set; }

    public string? Action { get; set; }

    public string? EntityName { get; set; }

    public string? EntityID { get; set; }

    public string? Search { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}