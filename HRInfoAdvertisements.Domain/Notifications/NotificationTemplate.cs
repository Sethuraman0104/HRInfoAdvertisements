namespace HRInfoAdvertisements.Domain.Entities;

public class NotificationTemplate
{
    public int NotificationTemplateID { get; set; }

    public string TemplateCode { get; set; } = string.Empty;

    public string TemplateName { get; set; } = string.Empty;

    public string? Subject { get; set; }

    public string? Body { get; set; }

    public string? SubjectAr { get; set; }

    public string? BodyAr { get; set; }

    public bool IsEmailEnabled { get; set; }

    public bool IsSmsEnabled { get; set; }

    public bool IsPushEnabled { get; set; }

    public bool IsActive { get; set; } = true;
}