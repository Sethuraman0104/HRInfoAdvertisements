
using System.ComponentModel.DataAnnotations;

namespace HRInfoAdvertisements.Application.Billing.DTOs;

public sealed class ConfirmPackagePaymentRequest
{
    [Required]
    [StringLength(20, MinimumLength = 1)]
    public string PaymentMethod { get; init; } = string.Empty;

    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string ProviderTransactionReference { get; init; } = string.Empty;
}
