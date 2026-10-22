namespace HRInfoAdvertisements.Domain.Entities;

public class AdvertisementFeature
{
    public int AdvertisementFeatureID { get; set; }

    public int CategoryID { get; set; }

    public string FeatureName { get; set; } = string.Empty;

    public string? FeatureNameAr { get; set; }

    public string DataType { get; set; } = "Text";

    public bool IsRequired { get; set; }

    public bool IsActive { get; set; } = true;

    public int DisplayOrder { get; set; }

    public AdvertisementCategory Category { get; set; } = null!;

    public ICollection<AdvertisementFeatureValue> Values { get; set; }
        = new List<AdvertisementFeatureValue>();
}