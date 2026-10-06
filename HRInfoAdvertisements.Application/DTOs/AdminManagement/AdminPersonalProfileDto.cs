namespace HRInfoAdvertisements.Application.DTOs.AdminManagement;

public class AdminPersonalProfileDto
{
    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? ProfilePhotoURL { get; set; }

    public string? Nationality { get; set; }

    public string? PreferredLanguage { get; set; }

    public bool IsBusinessAccount { get; set; }

    public string? CompanyName { get; set; }
}