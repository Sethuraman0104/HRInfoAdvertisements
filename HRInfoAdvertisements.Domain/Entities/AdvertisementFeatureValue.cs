namespace HRInfoAdvertisements.Domain.Entities;

public class AdvertisementFeatureValue
{
    public long AdvertisementFeatureValueID { get; set; }

    public long AdvertisementID { get; set; }

    public int AdvertisementFeatureID { get; set; }

    public string? FeatureValue { get; set; }

    public string? FeatureValueAr { get; set; }

    public Advertisement Advertisement { get; set; } = null!;

    public AdvertisementFeature Feature { get; set; } = null!;
}