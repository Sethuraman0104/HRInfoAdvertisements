namespace HRInfoAdvertisements.Application.DTOs.UserManagement;

public class UserProfileResponse
{
    public long UserProfileID { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string? LastName { get; set; }

    public string? ProfilePhotoURL { get; set; }

    public string? Nationality { get; set; }

    public string PreferredLanguage { get; set; } = "en";

    public bool IsBusinessAccount { get; set; }

    public string? CompanyName { get; set; }
}