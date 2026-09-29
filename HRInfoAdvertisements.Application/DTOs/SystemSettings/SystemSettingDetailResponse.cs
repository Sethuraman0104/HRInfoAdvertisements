namespace HRInfoAdvertisements.Application.DTOs.SystemSettings;

public class SystemSettingDetailResponse
{
    public int SystemSettingID { get; set; }

    public string SettingKey { get; set; } = string.Empty;

    public string? SettingValue { get; set; }

    public string? Description { get; set; }

    public bool IsEncrypted { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }
}