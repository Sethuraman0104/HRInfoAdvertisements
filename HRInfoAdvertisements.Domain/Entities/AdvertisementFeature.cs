namespace HRInfoAdvertisements.Domain.Entities;

public class AdvertisementFeature
{
    public int AdvertisementFeatureID { get; set; }

    public int CategoryID { get; set; }

    public string FeatureName { get; set; } = string.Empty;

    public string? FeatureNameAr { get; set; }

    public string DataType { get; set; } = "Text";

    public bool IsRequired { get; set; }

    public string? HelpText { get; set; }
    public string? HelpTextAr { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public int? MinLength { get; set; }
    public int? MaxLength { get; set; }
    public string? ValidationPattern { get; set; }
    public bool IsSearchable { get; set; }
    public bool IsFilterable { get; set; }

    public bool IsActive { get; set; } = true;

    public int DisplayOrder { get; set; }

    public AdvertisementCategory Category { get; set; } = null!;

    public ICollection<AdvertisementFeatureValue> Values { get; set; }
        = new List<AdvertisementFeatureValue>();

    public ICollection<AdvertisementFeatureOption> Options { get; set; }
        = new List<AdvertisementFeatureOption>();
}