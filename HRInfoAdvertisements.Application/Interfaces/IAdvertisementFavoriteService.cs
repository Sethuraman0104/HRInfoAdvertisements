using HRInfoAdvertisements.Application.DTOs.Favorite;

namespace HRInfoAdvertisements.Application.Interfaces;

public interface IAdvertisementFavoriteService
{
    Task<bool> AddFavoriteAsync(
        long userId,
        long advertisementId);

    Task<bool> RemoveFavoriteAsync(
        long userId,
        long advertisementId);

    Task<FavoriteStatusResponse>
        GetFavoriteStatusAsync(
            long userId,
            long advertisementId);

    Task<List<FavoriteResponse>>
        GetMyFavoritesAsync(
            long userId);
}