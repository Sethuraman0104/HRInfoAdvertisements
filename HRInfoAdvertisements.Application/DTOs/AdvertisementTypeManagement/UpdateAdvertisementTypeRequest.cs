using System.ComponentModel.DataAnnotations;

namespace HRInfoAdvertisements.Application.DTOs.AdvertisementTypeManagement;

public class UpdateAdvertisementTypeRequest
{
    [Required]
    [MaxLength(100)]
    public string TypeName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? TypeNameAr { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; }
}