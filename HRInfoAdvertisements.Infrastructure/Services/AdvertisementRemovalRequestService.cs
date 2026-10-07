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

    public AdvertisementRemovalRequestService(ApplicationDbContext context)
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

        // Same message for "missing" and "not yours" so ids cannot be probed.
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
                .AnyAsync(x =>
                    x.AdvertisementID == advertisementId &&
                    x.RequestStatus == RemovalRequestStatuses.Pending,
                    cancellationToken);

        if (hasPending)
        {
            throw new InvalidOperationException(PendingMessage);
        }

        var entity =
            new AdvertisementRemovalRequest
            {
                AdvertisementID = advertisementId,
                UserID = userId,
                RequestType = type,
                Reason = reason,
                RequestStatus = RemovalRequestStatuses.Pending,
                CreatedDate = DateTime.UtcNow
            };

        _context.AdvertisementRemovalRequests.Add(entity);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // The filtered unique index caught a double submit.
            throw new InvalidOperationException(PendingMessage);
        }

        return Map(entity);
    }

    // ------------------------------------------------------------
    // LATEST
    // ------------------------------------------------------------

    public async Task<AdvertisementRemovalRequestResponse?> GetLatestAsync(
        long userId,
        long advertisementId,
        CancellationToken cancellationToken = default)
    {
        var entity =
            await _context.AdvertisementRemovalRequests
                .AsNoTracking()
                .Where(x =>
                    x.AdvertisementID == advertisementId &&
                    x.UserID == userId)
                .OrderByDescending(x => x.CreatedDate)
                .FirstOrDefaultAsync(cancellationToken);

        return entity is null ? null : Map(entity);
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
                .FirstOrDefaultAsync(x =>
                    x.AdvertisementID == advertisementId &&
                    x.UserID == userId &&
                    x.RequestStatus == RemovalRequestStatuses.Pending,
                    cancellationToken);

        if (entity is null)
        {
            return false;
        }

        entity.RequestStatus = RemovalRequestStatuses.Cancelled;
        entity.ReviewedDate = DateTime.UtcNow;
        entity.ReviewComments = "Cancelled by the advertiser.";

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ------------------------------------------------------------
    // >>> ADJUST HERE (the only place that touches your Advertisement entity)
    // Return the owner's UserID and the status code of the advertisement.
    // Change the property names below to match your entity.
    // ------------------------------------------------------------

    private async Task<OwnerAndStatus?> GetOwnerAndStatusAsync(
        long advertisementId,
        CancellationToken cancellationToken)
    {
        return await _context.Advertisements
            .AsNoTracking()
            .Where(x => x.AdvertisementID == advertisementId)
            .Select(x => new OwnerAndStatus
            {
                UserID = x.UserID,
                StatusCode = x.Status.StatusCode      // <-- adjust if needed
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    private sealed class OwnerAndStatus
    {
        public long UserID { get; set; }
        public string StatusCode { get; set; } = string.Empty;
    }

    private static AdvertisementRemovalRequestResponse Map(
        AdvertisementRemovalRequest x) =>
        new()
        {
            RemovalRequestID = x.RemovalRequestID,
            AdvertisementID = x.AdvertisementID,
            RequestType = x.RequestType,
            Reason = x.Reason,
            RequestStatus = x.RequestStatus,
            CreatedDate = x.CreatedDate,
            ReviewedDate = x.ReviewedDate,
            ReviewComments = x.ReviewComments
        };
}