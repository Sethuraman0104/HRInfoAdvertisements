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

    Task<IReadOnlyList<AdminPackagePurchaseRequestResponse>>
        GetPendingPurchaseRequestsForAdminAsync(
            CancellationToken cancellationToken = default);

    Task<AdminPackagePurchaseRequestResponse?> ReviewPurchaseRequestAsync(
        long adminUserId,
        long purchaseRequestId,
        bool approve,
        CancellationToken cancellationToken = default);

    Task<AdminPackagePurchaseRequestResponse?> ConfirmPackagePaymentAsync(
        long adminUserId,
        long purchaseRequestId,
        ConfirmPackagePaymentRequest request,
        CancellationToken cancellationToken = default);
}

