using HRInfoAdvertisements.Application.DTOs.Admin;
using HRInfoAdvertisements.Application.DTOs.AdvertisementMedia;
using HRInfoAdvertisements.Application.Interfaces;
using HRInfoAdvertisements.Domain.Entities;
using HRInfoAdvertisements.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;

namespace HRInfoAdvertisements.Infrastructure.Services;

public class AdvertisementModerationService
    : IAdvertisementModerationService
{
    private readonly ApplicationDbContext _context;

    public AdvertisementModerationService(
        ApplicationDbContext context)
    {
        _context = context;
    }

    // ============================================================
    // ADMIN ADVERTISEMENT LIST
    // ============================================================

    public async Task<List<AdminAdvertisementListResponse>>
        GetAdvertisementsAsync(
            string? statusCode = null,
            string? search = null,
            int pageNumber = 1,
            int pageSize = 20)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize < 1 ? 20 : Math.Min(pageSize, 100);

        var query = _context.Advertisements
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.AdvertisementType)
            .Include(x => x.Status)
            .Include(x => x.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(statusCode))
        {
            var status = statusCode.Trim();

            query = query.Where(x =>
                x.Status.StatusCode == status);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchText = search.Trim();

            query = query.Where(x =>
                x.Title.Contains(searchText) ||
                x.AdvertisementNumber.Contains(searchText) ||
                x.User.UserName.Contains(searchText) ||
                (x.User.Email != null &&
                 x.User.Email.Contains(searchText)));
        }

        var advertisements = await query
            .OrderByDescending(x => x.CreatedDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return advertisements
            .Select(x => new AdminAdvertisementListResponse
            {
                AdvertisementID =
                    x.AdvertisementID,

                AdvertisementNumber =
                    x.AdvertisementNumber,

                Title =
                    x.Title,

                TitleAr =
                    x.TitleAr,

                Price =
                    x.Price,

                CurrencyCode =
                    x.CurrencyCode,

                CategoryName =
                    x.Category.CategoryName,

                AdvertisementTypeName =
                    x.AdvertisementType.TypeName,

                StatusCode =
                    x.Status.StatusCode,

                StatusName =
                    x.Status.StatusName,

                UserID =
                    x.UserID,

                UserName =
                    x.User.UserName,

                Email =
                    x.User.Email,

                MobileNo =
                    x.User.MobileNo,

                CreatedDate =
                    x.CreatedDate,

                PublishedDate =
                    x.PublishedDate,

                ExpiryDate =
                    x.ExpiryDate
            })
            .ToList();
    }

    // ============================================================
    // GET ADMIN DETAIL
    // ============================================================

    public async Task<AdminAdvertisementDetailResponse?>
        GetAdvertisementAsync(
            long advertisementId)
    {
        var advertisement = await _context.Advertisements
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Category)
            .Include(x => x.AdvertisementType)
            .Include(x => x.Status)
            .Include(x => x.Images)
            .Include(x => x.Videos)
            .Include(x => x.Documents)
            .FirstOrDefaultAsync(x =>
                x.AdvertisementID == advertisementId);

        if (advertisement == null)
        {
            return null;
        }

        var history =
            await GetApprovalHistoryAsync(advertisementId);

        return new AdminAdvertisementDetailResponse
        {
            AdvertisementID =
                advertisement.AdvertisementID,

            AdvertisementNumber =
                advertisement.AdvertisementNumber,

            UserID =
                advertisement.UserID,

            UserName =
                advertisement.User.UserName,

            Email =
                advertisement.User.Email,

            MobileNo =
                advertisement.User.MobileNo,

            Title =
                advertisement.Title,

            TitleAr =
                advertisement.TitleAr,

            Description =
                advertisement.Description,

            DescriptionAr =
                advertisement.DescriptionAr,

            Price =
                advertisement.Price,

            CurrencyCode =
                advertisement.CurrencyCode,

            IsNegotiable =
                advertisement.IsNegotiable,

            CategoryName =
                advertisement.Category.CategoryName,

            AdvertisementTypeName =
                advertisement.AdvertisementType.TypeName,

            StatusCode =
                advertisement.Status.StatusCode,

            StatusName =
                advertisement.Status.StatusName,

            AddressLine =
                advertisement.AddressLine,

            Latitude =
                advertisement.Latitude,

            Longitude =
                advertisement.Longitude,

            LandArea =
                advertisement.LandArea,

            BuiltUpArea =
                advertisement.BuiltUpArea,

            Bedrooms =
                advertisement.Bedrooms,

            Bathrooms =
                advertisement.Bathrooms,

            PropertyAge =
                advertisement.PropertyAge,

            IsFeatured =
                advertisement.IsFeatured,

            FeaturedUntil =
                advertisement.FeaturedUntil,

            PublishedDate =
                advertisement.PublishedDate,

            ExpiryDate =
                advertisement.ExpiryDate,

            CreatedDate =
                advertisement.CreatedDate,

            ModifiedDate =
                advertisement.ModifiedDate,

            Images =
                advertisement.Images
                    .OrderBy(x => x.DisplayOrder)
                    .Select(x => new AdvertisementImageResponse
                    {
                        AdvertisementImageID =
                            x.AdvertisementImageID,

                        AdvertisementID =
                            x.AdvertisementID,

                        FileName =
                            x.FileName,

                        FileURL =
                            x.FileURL,

                        ContentType =
                            x.ContentType,

                        FileSize =
                            x.FileSize,

                        IsPrimary =
                            x.IsPrimary,

                        DisplayOrder =
                            x.DisplayOrder,

                        CreatedDate =
                            x.CreatedDate
                    })
                    .ToList(),

            Videos =
                advertisement.Videos
                    .OrderBy(x => x.DisplayOrder)
                    .Select(x => new AdvertisementVideoResponse
                    {
                        AdvertisementVideoID =
                            x.AdvertisementVideoID,

                        AdvertisementID =
                            x.AdvertisementID,

                        FileName =
                            x.FileName,

                        FileURL =
                            x.FileURL,

                        ContentType =
                            x.ContentType,

                        FileSize =
                            x.FileSize,

                        DisplayOrder =
                            x.DisplayOrder,

                        CreatedDate =
                            x.CreatedDate
                    })
                    .ToList(),

            Documents =
                advertisement.Documents
                    .OrderByDescending(x => x.CreatedDate)
                    .Select(x => new AdvertisementDocumentResponse
                    {
                        AdvertisementDocumentID =
                            x.AdvertisementDocumentID,

                        AdvertisementID =
                            x.AdvertisementID,

                        DocumentName =
                            x.DocumentName,

                        FileURL =
                            x.FileURL,

                        ContentType =
                            x.ContentType,

                        FileSize =
                            x.FileSize,

                        CreatedDate =
                            x.CreatedDate
                    })
                    .ToList(),

            ApprovalHistory =
                history
        };
    }

    // ============================================================
    // APPROVE
    // ============================================================

    public async Task<bool> ApproveAdvertisementAsync(
        long adminUserId,
        long advertisementId,
        ApproveAdvertisementRequest request)
    {
        var advertisement =
            await GetAdvertisementForModerationAsync(
                advertisementId);

        if (advertisement == null)
        {
            return false;
        }

        if (advertisement.Status.StatusCode !=
            "PENDING_REVIEW")
        {
            throw new InvalidOperationException(
                "Only advertisements pending review can be approved.");
        }

        var approvalRequest =
            await GetOrCreateApprovalRequestAsync(
                advertisement,
                adminUserId);

        var publishedStatus =
            await GetStatusAsync("PUBLISHED");

        var now = DateTime.UtcNow;

        advertisement.StatusID =
            publishedStatus.StatusID;

        advertisement.PublishedDate =
            now;

        advertisement.ModifiedDate =
            now;

        advertisement.ModifiedBy =
            adminUserId;

        approvalRequest.Status =
            "Approved";

        approvalRequest.AssignedToUserID =
            adminUserId;

        approvalRequest.Comments =
            request.Comments;

        approvalRequest.CompletedDate =
            now;

        approvalRequest.ApprovalHistory.Add(
            new ApprovalHistory
            {
                Action =
                    "APPROVED",

                Comments =
                    request.Comments,

                ActionedByUserID =
                    adminUserId,

                ActionDate =
                    now
            });

        await _context.SaveChangesAsync();

        return true;
    }

    // ============================================================
    // REJECT
    // ============================================================

    public async Task<bool> RejectAdvertisementAsync(
        long adminUserId,
        long advertisementId,
        RejectAdvertisementRequest request)
    {
        var advertisement =
            await GetAdvertisementForModerationAsync(
                advertisementId);

        if (advertisement == null)
        {
            return false;
        }

        if (advertisement.Status.StatusCode !=
            "PENDING_REVIEW")
        {
            throw new InvalidOperationException(
                "Only advertisements pending review can be rejected.");
        }

        string? rejectionReasonText = null;

        if (request.RejectionReasonID.HasValue)
        {
            var reason =
                await _context.RejectionReasons
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.RejectionReasonID ==
                        request.RejectionReasonID.Value &&
                        x.IsActive);

            if (reason == null)
            {
                throw new InvalidOperationException(
                    "Invalid rejection reason.");
            }

            rejectionReasonText =
                reason.ReasonText;
        }

        var rejectedStatus =
            await GetStatusAsync("REJECTED");

        var now = DateTime.UtcNow;

        advertisement.StatusID =
            rejectedStatus.StatusID;

        advertisement.ModifiedDate =
            now;

        advertisement.ModifiedBy =
            adminUserId;

        var approvalRequest =
            await GetOrCreateApprovalRequestAsync(
                advertisement,
                adminUserId);

        var historyComments =
            BuildRejectionComments(
                rejectionReasonText,
                request.Comments);

        approvalRequest.Status =
            "Rejected";

        approvalRequest.AssignedToUserID =
            adminUserId;

        approvalRequest.Comments =
            historyComments;

        approvalRequest.CompletedDate =
            now;

        approvalRequest.ApprovalHistory.Add(
            new ApprovalHistory
            {
                Action =
                    "REJECTED",

                Comments =
                    historyComments,

                ActionedByUserID =
                    adminUserId,

                ActionDate =
                    now
            });

        await _context.SaveChangesAsync();

        return true;
    }

    // ============================================================
    // SUSPEND
    // ============================================================

    public async Task<bool> SuspendAdvertisementAsync(
        long adminUserId,
        long advertisementId,
        SuspendAdvertisementRequest request)
    {
        var advertisement =
            await GetAdvertisementForModerationAsync(
                advertisementId);

        if (advertisement == null)
        {
            return false;
        }

        if (advertisement.Status.StatusCode !=
            "PUBLISHED")
        {
            throw new InvalidOperationException(
                "Only published advertisements can be suspended.");
        }

        var suspendedStatus =
            await GetStatusAsync("SUSPENDED");

        var now = DateTime.UtcNow;

        advertisement.StatusID =
            suspendedStatus.StatusID;

        advertisement.ModifiedDate =
            now;

        advertisement.ModifiedBy =
            adminUserId;

        /*
         * Suspension is also recorded against the latest
         * approval request because ApprovalHistory belongs
         * to ApprovalRequest in the current database model.
         */
        var approvalRequest =
            await GetOrCreateApprovalRequestAsync(
                advertisement,
                adminUserId);

        approvalRequest.AssignedToUserID =
            adminUserId;

        approvalRequest.ApprovalHistory.Add(
            new ApprovalHistory
            {
                Action =
                    "SUSPENDED",

                Comments =
                    request.Comments,

                ActionedByUserID =
                    adminUserId,

                ActionDate =
                    now
            });

        await _context.SaveChangesAsync();

        return true;
    }

    // ============================================================
    // REACTIVATE
    // ============================================================

    public async Task<bool> ReactivateAdvertisementAsync(
        long adminUserId,
        long advertisementId,
        string? comments)
    {
        var advertisement =
            await GetAdvertisementForModerationAsync(
                advertisementId);

        if (advertisement == null)
        {
            return false;
        }

        if (advertisement.Status.StatusCode !=
            "SUSPENDED")
        {
            throw new InvalidOperationException(
                "Only suspended advertisements can be reactivated.");
        }

        var publishedStatus =
            await GetStatusAsync("PUBLISHED");

        var now = DateTime.UtcNow;

        advertisement.StatusID =
            publishedStatus.StatusID;

        advertisement.ModifiedDate =
            now;

        advertisement.ModifiedBy =
            adminUserId;

        var approvalRequest =
            await GetOrCreateApprovalRequestAsync(
                advertisement,
                adminUserId);

        approvalRequest.AssignedToUserID =
            adminUserId;

        approvalRequest.ApprovalHistory.Add(
            new ApprovalHistory
            {
                Action =
                    "REACTIVATED",

                Comments =
                    comments,

                ActionedByUserID =
                    adminUserId,

                ActionDate =
                    now
            });

        await _context.SaveChangesAsync();

        return true;
    }

    // ============================================================
    // APPROVAL HISTORY
    // ============================================================

    public async Task<List<ApprovalHistoryResponse>>
        GetApprovalHistoryAsync(
            long advertisementId)
    {
        var history =
            await _context.ApprovalHistories
                .AsNoTracking()
                .Include(x => x.ActionedByUser)
                .Include(x => x.ApprovalRequest)
                .Where(x =>
                    EF.Property<long>(
                        x.ApprovalRequest,
                        "AdvertisementID") ==
                    advertisementId)
                .OrderByDescending(x =>
                    x.ActionDate)
                .ToListAsync();

        return history
            .Select(x => new ApprovalHistoryResponse
            {
                ApprovalHistoryID =
                    x.ApprovalHistoryID,

                AdvertisementID =
                    advertisementId,

                Action =
                    x.Action,

                Comments =
                    x.Comments,

                ActionByUserID =
                    x.ActionedByUserID,

                ActionByUserName =
                    x.ActionedByUser != null
                        ? x.ActionedByUser.UserName
                        : null,

                ActionDate =
                    x.ActionDate,

                RejectionReasonID =
                    null,

                RejectionReason =
                    ExtractRejectionReason(
                        x.Comments)
            })
            .ToList();
    }

    // ============================================================
    // REJECTION REASONS
    // ============================================================

    public async Task<List<RejectionReasonResponse>>
        GetRejectionReasonsAsync()
    {
        return await _context.RejectionReasons
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.ReasonText)
            .Select(x => new RejectionReasonResponse
            {
                RejectionReasonID =
                    x.RejectionReasonID,

                ReasonCode =
                    x.ReasonCode,

                ReasonName =
                    x.ReasonText,

                ReasonNameAr =
                    x.ReasonTextAr,

                IsActive =
                    x.IsActive
            })
            .ToListAsync();
    }

    // ============================================================
    // GET ADVERTISEMENT FOR MODERATION
    // ============================================================

    private async Task<Advertisement?>
        GetAdvertisementForModerationAsync(
            long advertisementId)
    {
        return await _context.Advertisements
            .Include(x => x.Status)
            .FirstOrDefaultAsync(x =>
                x.AdvertisementID ==
                advertisementId);
    }

    // ============================================================
    // GET OR CREATE APPROVAL REQUEST
    // ============================================================

    private async Task<ApprovalRequest>
        GetOrCreateApprovalRequestAsync(
            Advertisement advertisement,
            long actionedByUserId)
    {
        var approvalRequest =
            await _context.ApprovalRequests
                .Include(x => x.ApprovalHistory)
                .Where(x =>
                    EF.Property<long>(
                        x,
                        "AdvertisementID") ==
                    advertisement.AdvertisementID)
                .OrderByDescending(x =>
                    x.SubmittedDate)
                .FirstOrDefaultAsync();

        if (approvalRequest != null)
        {
            return approvalRequest;
        }

        approvalRequest = new ApprovalRequest
        {
            SubmittedByUserID =
                advertisement.UserID,

            AssignedToUserID =
                actionedByUserId,

            Status =
                "Pending",

            SubmittedDate =
                advertisement.ModifiedDate
                ?? advertisement.CreatedDate
        };

        /*
         * AdvertisementID is currently a shadow property
         * because it exists in ApprovalRequestConfiguration
         * but not in ApprovalRequest.cs.
         */
        _context.Entry(approvalRequest)
            .Property<long>("AdvertisementID")
            .CurrentValue =
                advertisement.AdvertisementID;

        _context.ApprovalRequests.Add(
            approvalRequest);

        return approvalRequest;
    }

    // ============================================================
    // GET STATUS
    // ============================================================

    private async Task<AdvertisementStatus>
        GetStatusAsync(
            string statusCode)
    {
        var status =
            await _context.AdvertisementStatuses
                .FirstOrDefaultAsync(x =>
                    x.StatusCode == statusCode &&
                    x.IsActive);

        if (status == null)
        {
            throw new InvalidOperationException(
                $"Advertisement status '{statusCode}' was not found.");
        }

        return status;
    }

    // ============================================================
    // BUILD REJECTION COMMENTS
    // ============================================================

    private static string?
        BuildRejectionComments(
            string? reasonText,
            string? comments)
    {
        if (string.IsNullOrWhiteSpace(reasonText) &&
            string.IsNullOrWhiteSpace(comments))
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(reasonText))
        {
            return comments?.Trim();
        }

        if (string.IsNullOrWhiteSpace(comments))
        {
            return $"Rejection reason: {reasonText.Trim()}";
        }

        return
            $"Rejection reason: {reasonText.Trim()}\n" +
            $"Admin comments: {comments.Trim()}";
    }

    // ============================================================
    // EXTRACT REJECTION REASON
    // ============================================================

    private static string?
        ExtractRejectionReason(
            string? comments)
    {
        if (string.IsNullOrWhiteSpace(comments))
        {
            return null;
        }

        const string prefix =
            "Rejection reason:";

        var index =
            comments.IndexOf(
                prefix,
                StringComparison.OrdinalIgnoreCase);

        if (index < 0)
        {
            return null;
        }

        var start =
            index + prefix.Length;

        var end =
            comments.IndexOf(
                "\n",
                start,
                StringComparison.Ordinal);

        if (end < 0)
        {
            end = comments.Length;
        }

        var reason =
            comments[start..end].Trim();

        return string.IsNullOrWhiteSpace(reason)
            ? null
            : reason;
    }
}