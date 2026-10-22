namespace HRInfoAdvertisements.Application.DTOs.Advertisements;

public sealed class AdvertisementFeatureOptionResponse
{
    public int AdvertisementFeatureOptionID { get; set; }
    public string OptionValue { get; set; } = string.Empty;
    public string? OptionValueAr { get; set; }
    public int DisplayOrder { get; set; }
}

public sealed class AdvertisementFeatureValueResponse
{
    public int AdvertisementFeatureID { get; set; }
    public string FeatureName { get; set; } = string.Empty;
    public string? FeatureNameAr { get; set; }
    public string DataType { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public string? FeatureValue { get; set; }
    public string? FeatureValueAr { get; set; }
    public List<AdvertisementFeatureOptionResponse> Options { get; set; } = new();
}