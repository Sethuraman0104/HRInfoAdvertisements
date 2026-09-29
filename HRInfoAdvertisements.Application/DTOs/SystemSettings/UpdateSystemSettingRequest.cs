using System.ComponentModel.DataAnnotations;

namespace HRInfoAdvertisements.Application.DTOs.SystemSettings;

public class UpdateSystemSettingRequest
{
    [Required]
    [MaxLength(150)]
    public string SettingKey { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? SettingValue { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsEncrypted { get; set; }

    public bool IsActive { get; set; } = true;
}