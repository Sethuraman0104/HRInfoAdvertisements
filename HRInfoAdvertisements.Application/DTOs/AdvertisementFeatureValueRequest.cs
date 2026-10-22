using System.ComponentModel.DataAnnotations;

namespace HRInfoAdvertisements.Application.DTOs.Advertisements;

public sealed class AdvertisementFeatureValueRequest
{
    [Range(1, int.MaxValue)]
    public int AdvertisementFeatureID { get; set; }

    [MaxLength(1000)]
    public string? FeatureValue { get; set; }

    [MaxLength(1000)]
    public string? FeatureValueAr { get; set; }
}
