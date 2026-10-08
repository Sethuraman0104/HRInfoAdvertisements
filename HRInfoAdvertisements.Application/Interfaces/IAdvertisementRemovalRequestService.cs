using HRInfoAdvertisements.Application.DTOs.AdvertisementRemoval;

namespace HRInfoAdvertisements.Application.Interfaces;

public interface IAdvertisementRemovalRequestService
{
    Task<AdvertisementRemovalRequestResponse> CreateAsync(
        long userId,
        long advertisementId,
        CreateAdvertisementRemovalRequest request,
        CancellationToken cancellationToken = default);

    Task<AdvertisementRemovalRequestResponse?> GetLatestAsync(
        long userId,
        long advertisementId,
        CancellationToken cancellationToken = default);

    Task<bool> CancelPendingAsync(
        long userId,
        long advertisementId,
        CancellationToken cancellationToken = default);

    // ------------------------------------------------------------
    // ADMIN
    // ------------------------------------------------------------

    Task<List<AdminAdvertisementRemovalRequestResponse>>
        GetPendingForAdminAsync(
            CancellationToken cancellationToken = default);

    Task<bool> ApproveAsync(
        long adminUserId,
        long removalRequestId,
        string? comments,
        CancellationToken cancellationToken = default);

    Task<bool> RejectAsync(
        long adminUserId,
        long removalRequestId,
        string? comments,
        CancellationToken cancellationToken = default);
}