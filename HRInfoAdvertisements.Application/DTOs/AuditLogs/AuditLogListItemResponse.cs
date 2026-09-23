namespace HRInfoAdvertisements.Application.DTOs.AuditLogs;

public class AuditLogListItemResponse
{
    public long AuditLogID { get; set; }

    public long? UserID { get; set; }

    public string? UserName { get; set; }

    public string? UserEmail { get; set; }

    public string Action { get; set; } = string.Empty;

    public string EntityName { get; set; } = string.Empty;

    public string? EntityID { get; set; }

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }

    public string? IPAddress { get; set; }

    public string? UserAgent { get; set; }

    public DateTime CreatedDate { get; set; }
}