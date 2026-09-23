using HRInfoAdvertisements.Application.Messaging;
using HRInfoAdvertisements.Application.Messaging.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HRInfoAdvertisements.API.Controllers;

[ApiController]
[Route("api/v1/messages")]
[Authorize]
public class MessageController : ControllerBase
{
    private readonly IMessageService _messageService;

    public MessageController(IMessageService messageService)
    {
        _messageService = messageService;
    }

    [HttpPost]
    public async Task<ActionResult<MessageResponse>> Send(
        [FromBody] SendMessageRequest request)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        try
        {
            var result = await _messageService.SendAsync(
                userId.Value,
                request);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
        catch (UnauthorizedAccessException)
{
    return Forbid();
}
    }

    [HttpGet("{messageId:long}")]
    public async Task<ActionResult<MessageResponse>> GetById(
        long messageId)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        var result = await _messageService.GetByIdAsync(
            userId.Value,
            messageId);

        if (result == null)
            return NotFound(new
            {
                message = "Message not found."
            });

        return Ok(result);
    }

    [HttpGet("conversation")]
    public async Task<ActionResult<List<MessageResponse>>> GetConversation(
        [FromQuery] long otherUserId,
        [FromQuery] long? advertisementId = null,
        [FromQuery] long? enquiryId = null)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        try
        {
            var result = await _messageService.GetConversationAsync(
                userId.Value,
                otherUserId,
                advertisementId,
                enquiryId);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpGet("conversations")]
    public async Task<ActionResult<List<ConversationResponse>>> GetConversations()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        var result = await _messageService.GetConversationsAsync(
            userId.Value);

        return Ok(result);
    }

    [HttpPost("{messageId:long}/read")]
    public async Task<ActionResult<MarkMessageReadResponse>> MarkAsRead(
        long messageId)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        var result = await _messageService.MarkAsReadAsync(
            userId.Value,
            messageId);

        if (result == null)
            return NotFound(new
            {
                message = "Message not found or you are not the recipient."
            });

        return Ok(result);
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<UnreadMessageCountResponse>> GetUnreadCount()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        var result = await _messageService.GetUnreadCountAsync(
            userId.Value);

        return Ok(result);
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