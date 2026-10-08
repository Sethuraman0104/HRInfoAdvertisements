using HRInfoAdvertisements.Application.DTOs.AdvertisementRemoval;
using HRInfoAdvertisements.Application.Interfaces;
using HRInfoAdvertisements.Domain.Entities;
using HRInfoAdvertisements.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;

namespace HRInfoAdvertisements.Infrastructure.Services;

public class AdvertisementRemovalRequestService
    : IAdvertisementRemovalRequestService
{
    private const string PendingMessage =
        "You already have a pending request for this advertisement.";

    private readonly ApplicationDbContext _context;

    public AdvertisementRemovalRequestService(
        ApplicationDbContext context)
    {
        _context = context;
    }

    // ------------------------------------------------------------
    // CREATE
    // ------------------------------------------------------------

    public async Task<AdvertisementRemovalRequestResponse> CreateAsync(
        long userId,
        long advertisementId,
        CreateAdvertisementRemovalRequest request,
        CancellationToken cancellationToken = default)
    {
        var type =
            (request.RequestType ?? string.Empty)
                .Trim()
                .ToUpperInvariant();

        if (type != RemovalRequestTypes.Delete &&
            type != RemovalRequestTypes.Unpublish)
        {
            throw new InvalidOperationException(
                "Please choose a valid request type.");
        }

        var reason =
            string.IsNullOrWhiteSpace(request.Reason)
                ? null
                : request.Reason.Trim();

        if (reason is { Length: > 500 })
        {
            throw new InvalidOperationException(
                "The reason cannot exceed 500 characters.");
        }

        var advertisement =
            await GetOwnerAndStatusAsync(
                advertisementId,
                cancellationToken);

        // Same message for "missing" and "not yours"
        // so advertisement ids cannot be probed.
        if (advertisement is null ||
            advertisement.UserID != userId)
        {
            throw new InvalidOperationException(
                "The advertisement could not be found.");
        }

        if (!string.Equals(
                advertisement.StatusCode,
                "PUBLISHED",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Only published advertisements can have a removal request.");
        }

        var hasPending =
            await _context.AdvertisementRemovalRequests
                .AnyAsync(
                    x =>
                        x.AdvertisementID == advertisementId &&
                        x.RequestStatus ==
                            RemovalRequestStatuses.Pending,
                    cancellationToken);

        if (hasPending)
        {
            throw new InvalidOperationException(
                PendingMessage);
        }

        var entity =
            new AdvertisementRemovalRequest
            {
                AdvertisementID = advertisementId,
                UserID = userId,
                RequestType = type,
                Reason = reason,
                RequestStatus =
                    RemovalRequestStatuses.Pending,
                CreatedDate = DateTime.UtcNow
            };

        _context.AdvertisementRemovalRequests.Add(entity);

        try
        {
            await _context.SaveChangesAsync(
                cancellationToken);
        }
        catch (DbUpdateException)
        {
            // The filtered unique index caught
            // a possible double submit.
            throw new InvalidOperationException(
                PendingMessage);
        }

        return Map(entity);
    }

    // ------------------------------------------------------------
    // LATEST
    // ------------------------------------------------------------

    public async Task<AdvertisementRemovalRequestResponse?>
        GetLatestAsync(
            long userId,
            long advertisementId,
            CancellationToken cancellationToken = default)
    {
        var entity =
            await _context.AdvertisementRemovalRequests
                .AsNoTracking()
                .Where(
                    x =>
                        x.AdvertisementID == advertisementId &&
                        x.UserID == userId)
                .OrderByDescending(
                    x => x.CreatedDate)
                .FirstOrDefaultAsync(
                    cancellationToken);

        return entity is null
            ? null
            : Map(entity);
    }

    // ------------------------------------------------------------
    // CANCEL
    // ------------------------------------------------------------

    public async Task<bool> CancelPendingAsync(
        long userId,
        long advertisementId,
        CancellationToken cancellationToken = default)
    {
        var entity =
            await _context.AdvertisementRemovalRequests
                .FirstOrDefaultAsync(
                    x =>
                        x.AdvertisementID == advertisementId &&
                        x.UserID == userId &&
                        x.RequestStatus ==
                            RemovalRequestStatuses.Pending,
                    cancellationToken);

        if (entity is null)
        {
            return false;
        }

        entity.RequestStatus =
            RemovalRequestStatuses.Cancelled;

        entity.ReviewedDate =
            DateTime.UtcNow;

        entity.ReviewComments =
            "Cancelled by the advertiser.";

        await _context.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    // ------------------------------------------------------------
    // ADMIN - GET PENDING REMOVAL REQUESTS
    // ------------------------------------------------------------

    public async Task<
        List<AdminAdvertisementRemovalRequestResponse>>
        GetPendingForAdminAsync(
            CancellationToken cancellationToken = default)
    {
        var requests =
            await _context.AdvertisementRemovalRequests
                .AsNoTracking()
                .Where(
                    x =>
                        x.RequestStatus ==
                            RemovalRequestStatuses.Pending)
                .OrderBy(x => x.CreatedDate)
                .ToListAsync(cancellationToken);

        if (requests.Count == 0)
        {
            return new List<
                AdminAdvertisementRemovalRequestResponse>();
        }

        var advertisementIds =
            requests
                .Select(x => x.AdvertisementID)
                .Distinct()
                .ToList();

        var advertisements =
            await _context.Advertisements
                .AsNoTracking()
                .Include(x => x.Status)
                .Include(x => x.User)
                    .ThenInclude(x => x.UserProfile)
                .Where(
                    x =>
                        advertisementIds.Contains(
                            x.AdvertisementID))
                .ToListAsync(cancellationToken);

        var advertisementLookup =
            advertisements.ToDictionary(
                x => x.AdvertisementID);

        var result =
            new List<
                AdminAdvertisementRemovalRequestResponse>();

        foreach (var request in requests)
        {
            if (!advertisementLookup.TryGetValue(
                    request.AdvertisementID,
                    out var advertisement))
            {
                continue;
            }

            var user =
                advertisement.User;

            var userName =
                user.UserProfile is not null
                    ? BuildUserName(
                        user.UserProfile.FirstName,
                        user.UserProfile.LastName,
                        user.UserName)
                    : user.UserName;

            result.Add(
                new AdminAdvertisementRemovalRequestResponse
                {
                    RemovalRequestID =
                        request.RemovalRequestID,

                    AdvertisementID =
                        request.AdvertisementID,

                    AdvertisementNumber =
                        advertisement.AdvertisementNumber,

                    AdvertisementTitle =
                        advertisement.Title,

                    UserID =
                        request.UserID,

                    UserName =
                        userName,

                    Email =
                        user.Email,

                    RequestType =
                        request.RequestType,

                    Reason =
                        request.Reason,

                    RequestStatus =
                        request.RequestStatus,

                    AdvertisementStatusCode =
                        advertisement.Status.StatusCode,

                    AdvertisementStatusName =
                        advertisement.Status.StatusName,

                    CreatedDate =
                        request.CreatedDate,

                    ReviewedBy =
                        request.ReviewedBy,

                    ReviewedDate =
                        request.ReviewedDate,

                    ReviewComments =
                        request.ReviewComments
                });
        }

        return result;
    }

    // ------------------------------------------------------------
    // ADMIN - APPROVE REMOVAL REQUEST
    // ------------------------------------------------------------

    public async Task<bool> ApproveAsync(
        long adminUserId,
        long removalRequestId,
        string? comments,
        CancellationToken cancellationToken = default)
    {
        var removalRequest =
            await _context.AdvertisementRemovalRequests
                .FirstOrDefaultAsync(
                    x =>
                        x.RemovalRequestID ==
                            removalRequestId,
                    cancellationToken);

        if (removalRequest is null)
        {
            return false;
        }

        if (!string.Equals(
                removalRequest.RequestStatus,
                RemovalRequestStatuses.Pending,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "This removal request has already been reviewed.");
        }

        if (!string.Equals(
                removalRequest.RequestType,
                RemovalRequestTypes.Unpublish,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Only an unpublish request can be approved through the public listing removal workflow.");
        }

        var advertisement =
            await _context.Advertisements
                .Include(x => x.Status)
                .FirstOrDefaultAsync(
                    x =>
                        x.AdvertisementID ==
                            removalRequest.AdvertisementID,
                    cancellationToken);

        if (advertisement is null)
        {
            throw new InvalidOperationException(
                "The advertisement could not be found.");
        }

        if (!string.Equals(
                advertisement.Status.StatusCode,
                "PUBLISHED",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Only a published advertisement can be removed from the public listing.");
        }

        var unpublishedStatus =
            await _context.AdvertisementStatuses
                .FirstOrDefaultAsync(
                    x =>
                        x.StatusCode == "UNPUBLISHED" &&
                        x.IsActive,
                    cancellationToken);

        if (unpublishedStatus is null)
        {
            throw new InvalidOperationException(
                "The UNPUBLISHED advertisement status has not been configured.");
        }

        var now =
            DateTime.UtcNow;

        // --------------------------------------------------------
        // Remove from public listings.
        // The advertisement itself is NOT deleted.
        // --------------------------------------------------------

        advertisement.StatusID =
            unpublishedStatus.StatusID;

        advertisement.ModifiedDate =
            now;

        advertisement.ModifiedBy =
            adminUserId;

        // --------------------------------------------------------
        // Keep the request as an audit record.
        // --------------------------------------------------------

        removalRequest.RequestStatus =
            RemovalRequestStatuses.Approved;

        removalRequest.ReviewedBy =
            adminUserId;

        removalRequest.ReviewedDate =
            now;

        removalRequest.ReviewComments =
            string.IsNullOrWhiteSpace(comments)
                ? "Removal request approved by administrator."
                : comments.Trim();

        await _context.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    // ------------------------------------------------------------
    // ADMIN - REJECT REMOVAL REQUEST
    // ------------------------------------------------------------

    public async Task<bool> RejectAsync(
        long adminUserId,
        long removalRequestId,
        string? comments,
        CancellationToken cancellationToken = default)
    {
        var removalRequest =
            await _context.AdvertisementRemovalRequests
                .FirstOrDefaultAsync(
                    x =>
                        x.RemovalRequestID ==
                            removalRequestId,
                    cancellationToken);

        if (removalRequest is null)
        {
            return false;
        }

        if (!string.Equals(
                removalRequest.RequestStatus,
                RemovalRequestStatuses.Pending,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "This removal request has already been reviewed.");
        }

        if (!string.Equals(
                removalRequest.RequestType,
                RemovalRequestTypes.Unpublish,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Only an unpublish request can be rejected through the public listing removal workflow.");
        }

        var advertisement =
            await _context.Advertisements
                .Include(x => x.Status)
                .FirstOrDefaultAsync(
                    x =>
                        x.AdvertisementID ==
                            removalRequest.AdvertisementID,
                    cancellationToken);

        if (advertisement is null)
        {
            throw new InvalidOperationException(
                "The advertisement could not be found.");
        }

        if (!string.Equals(
                advertisement.Status.StatusCode,
                "PUBLISHED",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Only a published advertisement can have a removal request rejected.");
        }

        var now =
            DateTime.UtcNow;

        // Advertisement remains PUBLISHED.
        removalRequest.RequestStatus =
            RemovalRequestStatuses.Rejected;

        removalRequest.ReviewedBy =
            adminUserId;

        removalRequest.ReviewedDate =
            now;

        removalRequest.ReviewComments =
            string.IsNullOrWhiteSpace(comments)
                ? "Removal request rejected by administrator."
                : comments.Trim();

        await _context.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    // ------------------------------------------------------------
    // ADVERTISEMENT OWNER / STATUS
    // ------------------------------------------------------------

    private async Task<OwnerAndStatus?>
        GetOwnerAndStatusAsync(
            long advertisementId,
            CancellationToken cancellationToken)
    {
        return await _context.Advertisements
            .AsNoTracking()
            .Where(
                x =>
                    x.AdvertisementID ==
                        advertisementId)
            .Select(
                x => new OwnerAndStatus
                {
                    UserID =
                        x.UserID,

                    StatusCode =
                        x.Status.StatusCode
                })
            .FirstOrDefaultAsync(
                cancellationToken);
    }

    // ------------------------------------------------------------
    // HELPERS
    // ------------------------------------------------------------

    private static string BuildUserName(
        string? firstName,
        string? lastName,
        string fallback)
    {
        var fullName =
            string.Join(
                " ",
                new[]
                {
                    firstName?.Trim(),
                    lastName?.Trim()
                }
                .Where(
                    x =>
                        !string.IsNullOrWhiteSpace(x)));

        return string.IsNullOrWhiteSpace(fullName)
            ? fallback
            : fullName;
    }

    private sealed class OwnerAndStatus
    {
        public long UserID { get; set; }

        public string StatusCode { get; set; } =
            string.Empty;
    }

    private static AdvertisementRemovalRequestResponse Map(
        AdvertisementRemovalRequest x) =>
        new()
        {
            RemovalRequestID =
                x.RemovalRequestID,

            AdvertisementID =
                x.AdvertisementID,

            RequestType =
                x.RequestType,

            Reason =
                x.Reason,

            RequestStatus =
                x.RequestStatus,

            CreatedDate =
                x.CreatedDate,

            ReviewedDate =
                x.ReviewedDate,

            ReviewComments =
                x.ReviewComments
        };
}