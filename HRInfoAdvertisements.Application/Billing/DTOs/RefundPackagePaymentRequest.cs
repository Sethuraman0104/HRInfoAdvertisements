using System.ComponentModel.DataAnnotations;

namespace HRInfoAdvertisements.Application.Billing.DTOs;

public sealed class RefundPackagePaymentRequest
{
    [Range(typeof(bool), "true", "true",
        ErrorMessage = "Confirm that the external refund has already been processed.")]
    public bool RefundProcessedExternally { get; set; }

    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string ExternalRefundReference { get; set; } = string.Empty;

    [Required]
    [StringLength(1000, MinimumLength = 5)]
    public string Reason { get; set; } = string.Empty;
}
