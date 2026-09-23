using HRInfoAdvertisements.Application.Enquiries.DTOs;

namespace HRInfoAdvertisements.Application.Enquiries;

public interface IAdvertisementEnquiryService
{
    Task<AdvertisementEnquiryResponse> CreateAsync(
        long senderUserId,
        CreateAdvertisementEnquiryRequest request);

    Task<AdvertisementEnquiryDetailResponse?> GetByIdAsync(
        long userId,
        long enquiryId);

    Task<List<AdvertisementEnquiryResponse>> GetSentAsync(
        long userId);

    Task<List<AdvertisementEnquiryResponse>> GetReceivedAsync(
        long userId);

    Task<AdvertisementEnquiryMessageResponse?> ReplyAsync(
        long userId,
        long enquiryId,
        ReplyAdvertisementEnquiryRequest request);

    Task<bool> CloseAsync(
        long userId,
        long enquiryId);

    Task<bool> MarkMessagesAsReadAsync(
        long userId,
        long enquiryId);
}