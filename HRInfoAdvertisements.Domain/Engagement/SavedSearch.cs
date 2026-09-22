namespace HRInfoAdvertisements.Domain.Entities;

public class SavedSearch
{
    public long SavedSearchID { get; set; }

    public long UserID { get; set; }

    public string SearchName { get; set; } = string.Empty;

    public string? SearchCriteriaJson { get; set; }

    public bool EnableNotifications { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public User User { get; set; } = null!;
}