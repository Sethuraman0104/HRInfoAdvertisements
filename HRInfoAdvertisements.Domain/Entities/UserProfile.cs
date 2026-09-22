namespace HRInfoAdvertisements.Domain.Entities;

public class UserProfile
{
    public long UserProfileID { get; set; }

    public long UserID { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string? LastName { get; set; }

    public string? ProfilePhotoURL { get; set; }

    public string? Nationality { get; set; }

    public string PreferredLanguage { get; set; } = "en";

    public bool IsBusinessAccount { get; set; }

    public string? CompanyName { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public User User { get; set; } = null!;
}