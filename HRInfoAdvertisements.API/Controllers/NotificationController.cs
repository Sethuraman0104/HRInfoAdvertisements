using HRInfoAdvertisements.Application.Notifications;
using HRInfoAdvertisements.Application.Notifications.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HRInfoAdvertisements.API.Controllers;

[ApiController]
[Route("api/v1/notifications")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(
        INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<ActionResult<List<NotificationResponse>>> GetAll()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        var result = await _notificationService.GetAllAsync(
            userId.Value);

        return Ok(result);
    }

    [HttpGet("unread")]
    public async Task<ActionResult<List<NotificationResponse>>> GetUnread()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        var result = await _notificationService.GetUnreadAsync(
            userId.Value);

        return Ok(result);
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<UnreadNotificationCountResponse>>
        GetUnreadCount()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        var result = await _notificationService.GetUnreadCountAsync(
            userId.Value);

        return Ok(result);
    }

    [HttpGet("{notificationId:long}")]
    public async Task<ActionResult<NotificationResponse>> GetById(
        long notificationId)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        var result = await _notificationService.GetByIdAsync(
            userId.Value,
            notificationId);

        if (result == null)
        {
            return NotFound(new
            {
                message = "Notification not found."
            });
        }

        return Ok(result);
    }

    [HttpPost("{notificationId:long}/read")]
    public async Task<ActionResult<MarkNotificationReadResponse>>
        MarkAsRead(long notificationId)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        var result = await _notificationService.MarkAsReadAsync(
            userId.Value,
            notificationId);

        if (result == null)
        {
            return NotFound(new
            {
                message = "Notification not found."
            });
        }

        return Ok(result);
    }

    [HttpPost("read-all")]
    public async Task<ActionResult> MarkAllAsRead()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        var count = await _notificationService.MarkAllAsReadAsync(
            userId.Value);

        return Ok(new
        {
            message = "Notifications marked as read.",
            count
        });
    }

    private long? GetCurrentUserId()
    {
        var userIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("userId")?.Value
            ?? User.FindFirst("UserID")?.Value;

        if (long.TryParse(userIdClaim, out var userId))
            return userId;

        return null;
    }
}