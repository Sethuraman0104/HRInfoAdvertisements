using HRInfoAdvertisements.Application.Messaging.DTOs;

namespace HRInfoAdvertisements.Application.Messaging;

public interface IMessageService
{
    Task<MessageResponse> SendAsync(
        long senderUserId,
        SendMessageRequest request);

    Task<MessageResponse?> GetByIdAsync(
        long userId,
        long messageId);

    Task<List<MessageResponse>> GetConversationAsync(
        long userId,
        long otherUserId,
        long? advertisementId = null,
        long? enquiryId = null);

    Task<List<ConversationResponse>> GetConversationsAsync(
        long userId);

    Task<MarkMessageReadResponse?> MarkAsReadAsync(
        long userId,
        long messageId);

    Task<UnreadMessageCountResponse> GetUnreadCountAsync(
        long userId);
}