using HRInfoAdvertisements.Application.Messaging;
using HRInfoAdvertisements.Application.Messaging.DTOs;
using HRInfoAdvertisements.Domain.Entities;
using HRInfoAdvertisements.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRInfoAdvertisements.Infrastructure.Messaging;

public class MessageService : IMessageService
{
    private readonly ApplicationDbContext _context;

    public MessageService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MessageResponse> SendAsync(
        long senderUserId,
        SendMessageRequest request)
    {
        if (request.ReceiverUserID <= 0)
            throw new ArgumentException("ReceiverUserID is required.");

        if (request.ReceiverUserID == senderUserId)
            throw new ArgumentException("You cannot send a message to yourself.");

        if (string.IsNullOrWhiteSpace(request.MessageText))
            throw new ArgumentException("Message text is required.");

        var messageText = request.MessageText.Trim();

        if (messageText.Length > 4000)
            throw new ArgumentException(
                "Message text cannot exceed 4000 characters.");

        var receiverExists = await _context.Users
            .AsNoTracking()
            .AnyAsync(x => x.UserID == request.ReceiverUserID);

        if (!receiverExists)
            throw new KeyNotFoundException("Receiver user was not found.");

        if (request.AdvertisementID.HasValue)
        {
            var advertisementExists = await _context.Advertisements
                .AsNoTracking()
                .AnyAsync(x =>
                    x.AdvertisementID == request.AdvertisementID.Value);

            if (!advertisementExists)
                throw new KeyNotFoundException(
                    "Advertisement was not found.");
        }

        if (request.EnquiryID.HasValue)
        {
            var enquiry = await _context.AdvertisementEnquiries
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.AdvertisementEnquiryID == request.EnquiryID.Value);

            if (enquiry == null)
                throw new KeyNotFoundException(
                    "Advertisement enquiry was not found.");

            // Only users participating in the enquiry may use it
            // as the context of a message.
            if (enquiry.SenderUserID != senderUserId &&
                enquiry.RecipientUserID != senderUserId)
            {
                throw new UnauthorizedAccessException(
                    "You are not authorized to use this enquiry.");
            }

            if (enquiry.SenderUserID != request.ReceiverUserID &&
                enquiry.RecipientUserID != request.ReceiverUserID)
            {
                throw new ArgumentException(
                    "The receiver is not a participant in this enquiry.");
            }
        }

        var message = new Message
        {
            SenderUserID = senderUserId,
            ReceiverUserID = request.ReceiverUserID,
            AdvertisementID = request.AdvertisementID,
            EnquiryID = request.EnquiryID,
            MessageText = messageText,
            IsRead = false,
            ReadDate = null,
            CreatedDate = DateTime.UtcNow
        };

        _context.Messages.Add(message);

        await _context.SaveChangesAsync();

        return await BuildMessageResponseAsync(message.MessageID);
    }

    public async Task<MessageResponse?> GetByIdAsync(
        long userId,
        long messageId)
    {
        var message = await _context.Messages
            .AsNoTracking()
            .Include(x => x.SenderUser)
            .Include(x => x.ReceiverUser)
            .FirstOrDefaultAsync(x =>
                x.MessageID == messageId &&
                (x.SenderUserID == userId ||
                 x.ReceiverUserID == userId));

        if (message == null)
            return null;

        return MapMessage(message);
    }

    public async Task<List<MessageResponse>> GetConversationAsync(
        long userId,
        long otherUserId,
        long? advertisementId = null,
        long? enquiryId = null)
    {
        if (otherUserId <= 0)
            throw new ArgumentException("Other user ID is required.");

        if (otherUserId == userId)
            throw new ArgumentException(
                "You cannot retrieve a conversation with yourself.");

        var query = _context.Messages
            .AsNoTracking()
            .Include(x => x.SenderUser)
            .Include(x => x.ReceiverUser)
            .Where(x =>
                ((x.SenderUserID == userId &&
                  x.ReceiverUserID == otherUserId) ||
                 (x.SenderUserID == otherUserId &&
                  x.ReceiverUserID == userId)));

        if (advertisementId.HasValue)
        {
            query = query.Where(x =>
                x.AdvertisementID == advertisementId.Value);
        }

        if (enquiryId.HasValue)
        {
            query = query.Where(x =>
                x.EnquiryID == enquiryId.Value);
        }

        var messages = await query
            .OrderBy(x => x.CreatedDate)
            .ThenBy(x => x.MessageID)
            .ToListAsync();

        return messages
            .Select(MapMessage)
            .ToList();
    }

    public async Task<List<ConversationResponse>> GetConversationsAsync(
        long userId)
    {
        var messages = await _context.Messages
            .AsNoTracking()
            .Include(x => x.SenderUser)
            .Include(x => x.ReceiverUser)
            .Where(x =>
                x.SenderUserID == userId ||
                x.ReceiverUserID == userId)
            .OrderByDescending(x => x.CreatedDate)
            .ThenByDescending(x => x.MessageID)
            .ToListAsync();

        var conversations = messages
            .GroupBy(x => new
            {
                OtherUserID = x.SenderUserID == userId
                    ? x.ReceiverUserID
                    : x.SenderUserID,

                x.AdvertisementID,
                x.EnquiryID
            })
            .Select(group =>
            {
                var lastMessage = group
                    .OrderByDescending(x => x.CreatedDate)
                    .ThenByDescending(x => x.MessageID)
                    .First();

                var otherUser = lastMessage.SenderUserID == userId
                    ? lastMessage.ReceiverUser
                    : lastMessage.SenderUser;

                return new ConversationResponse
                {
                    OtherUserID = group.Key.OtherUserID,
                    OtherUserName = GetUserDisplayName(otherUser),
                    AdvertisementID = group.Key.AdvertisementID,
                    EnquiryID = group.Key.EnquiryID,
                    LastMessage = lastMessage.MessageText,
                    LastMessageDate = lastMessage.CreatedDate,
                    UnreadCount = group.Count(x =>
                        x.ReceiverUserID == userId &&
                        !x.IsRead)
                };
            })
            .OrderByDescending(x => x.LastMessageDate)
            .ToList();

        return conversations;
    }

    public async Task<MarkMessageReadResponse?> MarkAsReadAsync(
        long userId,
        long messageId)
    {
        var message = await _context.Messages
            .FirstOrDefaultAsync(x =>
                x.MessageID == messageId &&
                x.ReceiverUserID == userId);

        if (message == null)
            return null;

        if (!message.IsRead)
        {
            message.IsRead = true;
            message.ReadDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        return new MarkMessageReadResponse
        {
            MessageID = message.MessageID,
            IsRead = message.IsRead,
            ReadDate = message.ReadDate
        };
    }

    public async Task<UnreadMessageCountResponse> GetUnreadCountAsync(
        long userId)
    {
        var count = await _context.Messages
            .AsNoTracking()
            .CountAsync(x =>
                x.ReceiverUserID == userId &&
                !x.IsRead);

        return new UnreadMessageCountResponse
        {
            UnreadCount = count
        };
    }

    private async Task<MessageResponse> BuildMessageResponseAsync(
        long messageId)
    {
        var message = await _context.Messages
            .AsNoTracking()
            .Include(x => x.SenderUser)
            .Include(x => x.ReceiverUser)
            .FirstAsync(x => x.MessageID == messageId);

        return MapMessage(message);
    }

    private static MessageResponse MapMessage(Message message)
    {
        return new MessageResponse
        {
            MessageID = message.MessageID,

            SenderUserID = message.SenderUserID,
            SenderUserName = GetUserDisplayName(message.SenderUser),

            ReceiverUserID = message.ReceiverUserID,
            ReceiverUserName = GetUserDisplayName(message.ReceiverUser),

            AdvertisementID = message.AdvertisementID,
            EnquiryID = message.EnquiryID,

            MessageText = message.MessageText,

            IsRead = message.IsRead,
            ReadDate = message.ReadDate,
            CreatedDate = message.CreatedDate
        };
    }

    private static string GetUserDisplayName(User user)
    {
        if (!string.IsNullOrWhiteSpace(user.UserName))
            return user.UserName;

        return user.Email;
    }
}