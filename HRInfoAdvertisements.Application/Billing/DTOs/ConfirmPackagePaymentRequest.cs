namespace HRInfoAdvertisements.Application.Billing.DTOs;

public sealed class ConfirmPackagePaymentRequest
{
    public string PaymentMethod { get; init; } = string.Empty;

    public string ProviderTransactionReference { get; init; } = string.Empty;
}
