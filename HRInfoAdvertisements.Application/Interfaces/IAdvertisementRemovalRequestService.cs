using HRInfoAdvertisements.Application.DTOs.AdvertisementRemoval;

namespace HRInfoAdvertisements.Application.Interfaces;

public interface IAdvertisementRemovalRequestService
{
    /// <summary>
    /// Creates a request. Throws InvalidOperationException with a
    /// user-friendly message when a rule is broken.
    /// </summary>
    Task<AdvertisementRemovalRequestResponse> CreateAsync(
        long userId,
        long advertisementId,
        CreateAdvertisementRemovalRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Latest request for this advertisement, or null.</summary>
    Task<AdvertisementRemovalRequestResponse?> GetLatestAsync(
        long userId,
        long advertisementId,
        CancellationToken cancellationToken = default);

    /// <summary>Cancels the pending request. False if none is pending.</summary>
    Task<bool> CancelPendingAsync(
        long userId,
        long advertisementId,
        CancellationToken cancellationToken = default);
}