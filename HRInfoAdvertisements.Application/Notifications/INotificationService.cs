using HRInfoAdvertisements.Application.Notifications.DTOs;

namespace HRInfoAdvertisements.Application.Notifications;

public interface INotificationService
{
    Task<NotificationResponse> CreateAsync(
        CreateNotificationRequest request);

    Task<NotificationResponse?> GetByIdAsync(
        long userId,
        long notificationId);

    Task<List<NotificationResponse>> GetAllAsync(
        long userId);

    Task<List<NotificationResponse>> GetUnreadAsync(
        long userId);

    Task<UnreadNotificationCountResponse> GetUnreadCountAsync(
        long userId);

    Task<MarkNotificationReadResponse?> MarkAsReadAsync(
        long userId,
        long notificationId);

    Task<int> MarkAllAsReadAsync(
        long userId);
}