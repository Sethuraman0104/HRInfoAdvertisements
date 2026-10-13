using HRInfoAdvertisements.Application.Billing.DTOs;

namespace HRInfoAdvertisements.Application.Billing;

public interface IEnquiryBillingService
{
    Task<IReadOnlyList<EnquiryPackageResponse>> GetAvailablePackagesAsync(
        CancellationToken cancellationToken = default);

    Task<PackagePurchaseRequestResponse> CreatePurchaseRequestAsync(
        long userId,
        int enquiryPackageId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PackagePurchaseRequestResponse>> GetMyPurchaseRequestsAsync(
        long userId,
        CancellationToken cancellationToken = default);
}
