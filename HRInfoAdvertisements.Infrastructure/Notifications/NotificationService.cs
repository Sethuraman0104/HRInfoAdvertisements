using HRInfoAdvertisements.Application.Notifications;
using HRInfoAdvertisements.Application.Notifications.DTOs;
using HRInfoAdvertisements.Domain.Entities;
using HRInfoAdvertisements.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRInfoAdvertisements.Infrastructure.Notifications;

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;

    public NotificationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationResponse> CreateAsync(
        CreateNotificationRequest request)
    {
        if (request.UserID <= 0)
            throw new ArgumentException("UserID is required.");

        if (string.IsNullOrWhiteSpace(request.NotificationType))
            throw new ArgumentException(
                "Notification type is required.");

        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ArgumentException(
                "Notification title is required.");

        var title = request.Title.Trim();

        if (title.Length > 250)
            throw new ArgumentException(
                "Notification title cannot exceed 250 characters.");

        string? message = request.Message?.Trim();

        if (message?.Length > 4000)
            throw new ArgumentException(
                "Notification message cannot exceed 4000 characters.");

        var userExists = await _context.Users
            .AsNoTracking()
            .AnyAsync(x => x.UserID == request.UserID);

        if (!userExists)
            throw new KeyNotFoundException(
                "Notification user was not found.");

        if (request.NotificationTemplateID.HasValue)
        {
            var templateExists = await _context.NotificationTemplates
                .AsNoTracking()
                .AnyAsync(x =>
                    x.NotificationTemplateID ==
                    request.NotificationTemplateID.Value &&
                    x.IsActive);

            if (!templateExists)
                throw new KeyNotFoundException(
                    "Notification template was not found or is inactive.");
        }

        var notification = new Notification
        {
            UserID = request.UserID,
            NotificationTemplateID = request.NotificationTemplateID,
            NotificationType = request.NotificationType.Trim(),
            Title = title,
            Message = message,
            ReferenceType = string.IsNullOrWhiteSpace(
                request.ReferenceType)
                ? null
                : request.ReferenceType.Trim(),
            ReferenceID = request.ReferenceID,
            IsRead = false,
            ReadDate = null,
            CreatedDate = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);

        await _context.SaveChangesAsync();

        return await BuildNotificationResponseAsync(
            notification.NotificationID);
    }

    public async Task<NotificationResponse?> GetByIdAsync(
        long userId,
        long notificationId)
    {
        var notification = await _context.Notifications
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.NotificationID == notificationId &&
                x.UserID == userId);

        if (notification == null)
            return null;

        return MapNotification(notification);
    }

    public async Task<List<NotificationResponse>> GetAllAsync(
        long userId)
    {
        var notifications = await _context.Notifications
            .AsNoTracking()
            .Where(x => x.UserID == userId)
            .OrderByDescending(x => x.CreatedDate)
            .ThenByDescending(x => x.NotificationID)
            .ToListAsync();

        return notifications
            .Select(MapNotification)
            .ToList();
    }

    public async Task<List<NotificationResponse>> GetUnreadAsync(
        long userId)
    {
        var notifications = await _context.Notifications
            .AsNoTracking()
            .Where(x =>
                x.UserID == userId &&
                !x.IsRead)
            .OrderByDescending(x => x.CreatedDate)
            .ThenByDescending(x => x.NotificationID)
            .ToListAsync();

        return notifications
            .Select(MapNotification)
            .ToList();
    }

    public async Task<UnreadNotificationCountResponse>
        GetUnreadCountAsync(long userId)
    {
        var count = await _context.Notifications
            .AsNoTracking()
            .CountAsync(x =>
                x.UserID == userId &&
                !x.IsRead);

        return new UnreadNotificationCountResponse
        {
            UnreadCount = count
        };
    }

    public async Task<MarkNotificationReadResponse?> MarkAsReadAsync(
        long userId,
        long notificationId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(x =>
                x.NotificationID == notificationId &&
                x.UserID == userId);

        if (notification == null)
            return null;

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        return new MarkNotificationReadResponse
        {
            NotificationID = notification.NotificationID,
            IsRead = notification.IsRead,
            ReadDate = notification.ReadDate
        };
    }

    public async Task<int> MarkAllAsReadAsync(
        long userId)
    {
        var notifications = await _context.Notifications
            .Where(x =>
                x.UserID == userId &&
                !x.IsRead)
            .ToListAsync();

        if (notifications.Count == 0)
            return 0;

        var readDate = DateTime.UtcNow;

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadDate = readDate;
        }

        await _context.SaveChangesAsync();

        return notifications.Count;
    }

    private async Task<NotificationResponse>
        BuildNotificationResponseAsync(long notificationId)
    {
        var notification = await _context.Notifications
            .AsNoTracking()
            .FirstAsync(x =>
                x.NotificationID == notificationId);

        return MapNotification(notification);
    }

    private static NotificationResponse MapNotification(
        Notification notification)
    {
        return new NotificationResponse
        {
            NotificationID = notification.NotificationID,
            UserID = notification.UserID,
            NotificationTemplateID =
                notification.NotificationTemplateID,
            NotificationType = notification.NotificationType,
            Title = notification.Title,
            Message = notification.Message,
            ReferenceType = notification.ReferenceType,
            ReferenceID = notification.ReferenceID,
            IsRead = notification.IsRead,
            ReadDate = notification.ReadDate,
            CreatedDate = notification.CreatedDate
        };
    }
}