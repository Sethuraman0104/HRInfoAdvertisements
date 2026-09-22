namespace HRInfoAdvertisements.Domain.Entities;

public class AuditLog
{
    public long AuditLogID { get; set; }

    public long? UserID { get; set; }

    public string Action { get; set; } = string.Empty;

    public string EntityName { get; set; } = string.Empty;

    public string? EntityID { get; set; }

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }

    public string? IPAddress { get; set; }

    public string? UserAgent { get; set; }

    public DateTime CreatedDate { get; set; }

    public User? User { get; set; }
}