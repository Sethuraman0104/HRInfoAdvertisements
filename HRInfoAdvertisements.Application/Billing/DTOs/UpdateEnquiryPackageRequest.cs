using System.ComponentModel.DataAnnotations;

namespace HRInfoAdvertisements.Application.Billing.DTOs;

public sealed class UpdateEnquiryPackageRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; init; }

    [Range(0, int.MaxValue)]
    public int MonthlySenderEnquiryLimit { get; init; }

    [Range(0, int.MaxValue)]
    public int MonthlyAdvertisementEnquiryLimit { get; init; }

    [Range(1, 3650)]
    public int? ValidityDays { get; init; }

    [Range(typeof(decimal), "0", "999999999999999.999")]
    public decimal Price { get; init; }

    [Required]
    [RegularExpression("^[A-Za-z]{3}$")]
    public string CurrencyCode { get; init; } = "BHD";

    public bool IsActive { get; init; }
}
