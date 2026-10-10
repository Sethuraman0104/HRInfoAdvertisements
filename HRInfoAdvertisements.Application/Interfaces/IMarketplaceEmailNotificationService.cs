namespace HRInfoAdvertisements.Application.Interfaces;

public interface IMarketplaceEmailNotificationService
{
    Task SendAdvertisementPublishedAsync(long advertisementId);

    Task SendFavoriteAdvertisementUpdatedAsync(long advertisementId);
}