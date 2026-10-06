using System.ComponentModel.DataAnnotations;

namespace HRInfoAdvertisements.Application.DTOs.AdminManagement;

public class UpdateAdminProfileRequest
{
    [Required]
    [MaxLength(100)]
    public string? FirstName { get; set; }

    [MaxLength(100)]
    public string? LastName { get; set; }

    [MaxLength(100)]
    public string? Nationality { get; set; }

    [MaxLength(10)]
    public string? PreferredLanguage { get; set; }

    [MaxLength(1000)]
    [Url]
    public string? ProfilePhotoURL { get; set; }
}