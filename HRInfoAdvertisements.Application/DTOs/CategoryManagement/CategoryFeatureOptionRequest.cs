namespace HRInfoAdvertisements.Application.DTOs.CategoryManagement;

public sealed class CategoryFeatureOptionRequest
{
    public string OptionValue { get; set; } = string.Empty;
    public string? OptionValueAr { get; set; }
    public int DisplayOrder { get; set; }
}