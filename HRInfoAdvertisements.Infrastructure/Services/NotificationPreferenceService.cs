using HRInfoAdvertisements.Application.NotificationPreferences;
using HRInfoAdvertisements.Application.NotificationPreferences.DTOs;
using HRInfoAdvertisements.Domain.Entities;
using HRInfoAdvertisements.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRInfoAdvertisements.Infrastructure.Services;

public class NotificationPreferenceService
    : INotificationPreferenceService
{
    private readonly ApplicationDbContext _db;

    public NotificationPreferenceService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<NotificationPreferenceResponse> GetAsync(
        long userId)
    {
        var preference = await _db.NotificationPreferences
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserID == userId);

        // Existing users may not have a preference row yet.
        // Treat missing preferences as all enabled.
        if (preference == null)
        {
            return new NotificationPreferenceResponse
            {
                UserID = userId,
                EmailNotificationsEnabled = true,
                AdvertisementUpdatesEnabled = true,
                FavoriteAdvertisementUpdatesEnabled = true
            };
        }

        return Map(preference);
    }

    public async Task<NotificationPreferenceResponse> UpdateAsync(
        long userId,
        UpdateNotificationPreferenceRequest request)
    {
        var userExists = await _db.Users
            .AsNoTracking()
            .AnyAsync(x => x.UserID == userId);

        if (!userExists)
        {
            throw new KeyNotFoundException(
                $"User with ID {userId} was not found.");
        }

        var preference = await _db.NotificationPreferences
            .FirstOrDefaultAsync(x => x.UserID == userId);

        if (preference == null)
        {
            preference = new NotificationPreference
            {
                UserID = userId,
                EmailNotificationsEnabled =
                    request.EmailNotificationsEnabled,
                AdvertisementUpdatesEnabled =
                    request.AdvertisementUpdatesEnabled,
                FavoriteAdvertisementUpdatesEnabled =
                    request.FavoriteAdvertisementUpdatesEnabled,
                CreatedDate = DateTime.UtcNow
            };

            _db.NotificationPreferences.Add(preference);
        }
        else
        {
            preference.EmailNotificationsEnabled =
                request.EmailNotificationsEnabled;

            preference.AdvertisementUpdatesEnabled =
                request.AdvertisementUpdatesEnabled;

            preference.FavoriteAdvertisementUpdatesEnabled =
                request.FavoriteAdvertisementUpdatesEnabled;

            preference.ModifiedDate = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();

        return Map(preference);
    }

    private static NotificationPreferenceResponse Map(
        NotificationPreference preference)
    {
        return new NotificationPreferenceResponse
        {
            UserID = preference.UserID,
            EmailNotificationsEnabled =
                preference.EmailNotificationsEnabled,
            AdvertisementUpdatesEnabled =
                preference.AdvertisementUpdatesEnabled,
            FavoriteAdvertisementUpdatesEnabled =
                preference.FavoriteAdvertisementUpdatesEnabled
        };
    }
}