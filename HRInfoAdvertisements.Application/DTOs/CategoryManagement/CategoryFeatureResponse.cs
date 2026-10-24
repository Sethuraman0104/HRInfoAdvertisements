namespace HRInfoAdvertisements.Application.DTOs.CategoryManagement;

public sealed class CategoryFeatureResponse
{
    public int AdvertisementFeatureID { get; set; }
    public int CategoryID { get; set; }
    public string FeatureName { get; set; } = string.Empty;
    public string? FeatureNameAr { get; set; }
    public string DataType { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public bool IsActive { get; set; } = true;
    public string? HelpText { get; set; }
    public string? HelpTextAr { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public int? MinLength { get; set; }
    public int? MaxLength { get; set; }
    public string? ValidationPattern { get; set; }
    public bool IsSearchable { get; set; }
    public bool IsFilterable { get; set; }
    public int DisplayOrder { get; set; }

    public List<CategoryFeatureOptionResponse> Options { get; set; } = new();
}

public sealed class CategoryFeatureOptionResponse
{
    public int AdvertisementFeatureOptionID { get; set; }
    public string OptionValue { get; set; } = string.Empty;
    public string? OptionValueAr { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}