namespace HRInfoAdvertisements.Domain.Entities;

public class AdvertisementCategory
{
    public int CategoryID { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public string? CategoryNameAr { get; set; }

    public string? Description { get; set; }

    public int? ParentCategoryID { get; set; }

    public AdvertisementCategory? ParentCategory { get; set; }

    public ICollection<AdvertisementCategory> SubCategories { get; set; }
        = new List<AdvertisementCategory>();

    public bool IsActive { get; set; } = true;

    public int DisplayOrder { get; set; }

    public ICollection<Advertisement> Advertisements { get; set; } = new List<Advertisement>();
}