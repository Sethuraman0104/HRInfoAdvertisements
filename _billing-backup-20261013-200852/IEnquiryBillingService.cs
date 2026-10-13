using HRInfoAdvertisements.Application.Billing.DTOs;

namespace HRInfoAdvertisements.Application.Billing;

public interface IEnquiryBillingService
{
    Task<IReadOnlyList<EnquiryPackageResponse>> GetAvailablePackagesAsync(
        CancellationToken cancellationToken = default);
}
