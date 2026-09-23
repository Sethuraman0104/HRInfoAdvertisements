using HRInfoAdvertisements.Application.DTOs.Admin;

namespace HRInfoAdvertisements.Application.Interfaces;

public interface IAdvertisementModerationService
{
    Task<List<AdminAdvertisementListResponse>> GetAdvertisementsAsync(
        string? statusCode = null,
        string? search = null,
        int pageNumber = 1,
        int pageSize = 20);

    Task<AdminAdvertisementDetailResponse?> GetAdvertisementAsync(
        long advertisementId);

    Task<bool> ApproveAdvertisementAsync(
        long adminUserId,
        long advertisementId,
        ApproveAdvertisementRequest request);

    Task<bool> RejectAdvertisementAsync(
        long adminUserId,
        long advertisementId,
        RejectAdvertisementRequest request);

    Task<bool> SuspendAdvertisementAsync(
        long adminUserId,
        long advertisementId,
        SuspendAdvertisementRequest request);

    Task<bool> ReactivateAdvertisementAsync(
        long adminUserId,
        long advertisementId,
        string? comments);

    Task<List<ApprovalHistoryResponse>> GetApprovalHistoryAsync(
        long advertisementId);

    Task<List<RejectionReasonResponse>> GetRejectionReasonsAsync();
}