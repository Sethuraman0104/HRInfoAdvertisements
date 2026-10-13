using HRInfoAdvertisements.Application.Billing.DTOs;

namespace HRInfoAdvertisements.Application.Billing;

public interface IEnquiryPackageAdminService
{
    Task<IReadOnlyList<EnquiryPackageResponse>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<EnquiryPackageResponse?> UpdateAsync(
        int enquiryPackageId,
        UpdateEnquiryPackageRequest request,
        CancellationToken cancellationToken = default);
}
