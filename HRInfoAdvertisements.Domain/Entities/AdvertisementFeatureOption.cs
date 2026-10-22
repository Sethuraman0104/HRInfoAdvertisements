namespace HRInfoAdvertisements.Domain.Entities;

public class AdvertisementFeatureOption
{
    public int AdvertisementFeatureOptionID { get; set; }
    public int AdvertisementFeatureID { get; set; }
    public string OptionValue { get; set; } = string.Empty;
    public string? OptionValueAr { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public AdvertisementFeature Feature { get; set; } = null!;
}