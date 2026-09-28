namespace HRInfoAdvertisements.Application.DTOs.CategoryManagement;

public class CategoryListItemResponse
{
    public int CategoryID { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public string? CategoryNameAr { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public int DisplayOrder { get; set; }

    public int AdvertisementCount { get; set; }
}
