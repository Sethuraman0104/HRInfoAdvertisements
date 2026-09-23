using HRInfoAdvertisements.Application.Enquiries;
using HRInfoAdvertisements.Application.Enquiries.DTOs;
using HRInfoAdvertisements.Domain.Entities;
using HRInfoAdvertisements.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRInfoAdvertisements.Infrastructure.Enquiries;

public class AdvertisementEnquiryService : IAdvertisementEnquiryService
{
    private readonly ApplicationDbContext _context;

    public AdvertisementEnquiryService(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdvertisementEnquiryResponse> CreateAsync(
        long senderUserId,
        CreateAdvertisementEnquiryRequest request)
    {
        if (request.AdvertisementID <= 0)
            throw new ArgumentException("AdvertisementID is required.");

        if (string.IsNullOrWhiteSpace(request.Subject))
            throw new ArgumentException("Subject is required.");

        if (string.IsNullOrWhiteSpace(request.Message))
            throw new ArgumentException("Message is required.");

        var advertisement = await _context.Advertisements
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.AdvertisementID == request.AdvertisementID);

        if (advertisement == null)
            throw new KeyNotFoundException(
                "Advertisement not found.");

        var status = await _context.AdvertisementStatuses
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.StatusID == advertisement.StatusID);

        if (status == null ||
            !string.Equals(
                status.StatusCode,
                "PUBLISHED",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Enquiries can only be sent for published advertisements.");
        }

        if (advertisement.UserID == senderUserId)
        {
            throw new InvalidOperationException(
                "You cannot send an enquiry for your own advertisement.");
        }

        var sender = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.UserID == senderUserId);

        if (sender == null)
            throw new KeyNotFoundException(
                "Sender user not found.");

        var enquiry = new AdvertisementEnquiry
        {
            AdvertisementID = advertisement.AdvertisementID,

            // Always derive recipient from the advertisement owner.
            RecipientUserID = advertisement.UserID,

            SenderUserID = senderUserId,

            Subject = request.Subject.Trim(),

            Message = request.Message.Trim(),

            ContactMobile =
                string.IsNullOrWhiteSpace(request.ContactMobile)
                    ? sender.MobileNo
                    : request.ContactMobile.Trim(),

            ContactEmail =
                string.IsNullOrWhiteSpace(request.ContactEmail)
                    ? sender.Email
                    : request.ContactEmail.Trim(),

            Status = "OPEN",

            CreatedDate = DateTime.UtcNow
        };

        _context.AdvertisementEnquiries.Add(enquiry);

        await _context.SaveChangesAsync();

        return await BuildResponseAsync(
            enquiry.AdvertisementEnquiryID)
            ?? throw new InvalidOperationException(
                "Unable to retrieve the created enquiry.");
    }

    public async Task<AdvertisementEnquiryDetailResponse?> GetByIdAsync(
        long userId,
        long enquiryId)
    {
        var enquiry = await _context.AdvertisementEnquiries
            .AsNoTracking()
            .Include(x => x.Advertisement)
            .Include(x => x.SenderUser)
            .Include(x => x.RecipientUser)
            .Include(x => x.Messages)
                .ThenInclude(x => x.SenderUser)
            .FirstOrDefaultAsync(x =>
                x.AdvertisementEnquiryID == enquiryId &&
                (x.SenderUserID == userId ||
                 x.RecipientUserID == userId));

        if (enquiry == null)
            return null;

        return MapDetail(enquiry);
    }

    public async Task<List<AdvertisementEnquiryResponse>> GetSentAsync(
        long userId)
    {
        var enquiries = await _context.AdvertisementEnquiries
            .AsNoTracking()
            .Include(x => x.Advertisement)
            .Include(x => x.SenderUser)
            .Include(x => x.RecipientUser)
            .Include(x => x.Messages)
            .Where(x => x.SenderUserID == userId)
            .OrderByDescending(x => x.CreatedDate)
            .ToListAsync();

        return enquiries
            .Select(MapResponse)
            .ToList();
    }

    public async Task<List<AdvertisementEnquiryResponse>> GetReceivedAsync(
        long userId)
    {
        var enquiries = await _context.AdvertisementEnquiries
            .AsNoTracking()
            .Include(x => x.Advertisement)
            .Include(x => x.SenderUser)
            .Include(x => x.RecipientUser)
            .Include(x => x.Messages)
            .Where(x => x.RecipientUserID == userId)
            .OrderByDescending(x => x.CreatedDate)
            .ToListAsync();

        return enquiries
            .Select(MapResponse)
            .ToList();
    }

    public async Task<AdvertisementEnquiryMessageResponse?> ReplyAsync(
        long userId,
        long enquiryId,
        ReplyAdvertisementEnquiryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            throw new ArgumentException("Message is required.");

        var enquiry = await _context.AdvertisementEnquiries
            .FirstOrDefaultAsync(x =>
                x.AdvertisementEnquiryID == enquiryId &&
                (x.SenderUserID == userId ||
                 x.RecipientUserID == userId));

        if (enquiry == null)
            return null;

        if (string.Equals(
                enquiry.Status,
                "CLOSED",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "This enquiry is closed and cannot receive new replies.");
        }

        var message = new AdvertisementEnquiryMessage
        {
            AdvertisementEnquiryID =
                enquiry.AdvertisementEnquiryID,

            SenderUserID = userId,

            Message = request.Message.Trim(),

            CreatedDate = DateTime.UtcNow,

            IsRead = false
        };

        _context.AdvertisementEnquiryMessages.Add(message);

        enquiry.LastRepliedDate = message.CreatedDate;
        enquiry.Status = "OPEN";

        await _context.SaveChangesAsync();

        var sender = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.UserID == userId);

        return new AdvertisementEnquiryMessageResponse
        {
            AdvertisementEnquiryMessageID =
                message.AdvertisementEnquiryMessageID,

            AdvertisementEnquiryID =
                message.AdvertisementEnquiryID,

            SenderUserID =
                message.SenderUserID,

            SenderName =
                GetUserDisplayName(sender),

            Message =
                message.Message,

            CreatedDate =
                message.CreatedDate,

            IsRead =
                message.IsRead,

            ReadDate =
                message.ReadDate
        };
    }

    public async Task<bool> CloseAsync(
        long userId,
        long enquiryId)
    {
        var enquiry = await _context.AdvertisementEnquiries
            .FirstOrDefaultAsync(x =>
                x.AdvertisementEnquiryID == enquiryId &&
                (x.SenderUserID == userId ||
                 x.RecipientUserID == userId));

        if (enquiry == null)
            return false;

        if (string.Equals(
                enquiry.Status,
                "CLOSED",
                StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        enquiry.Status = "CLOSED";
        enquiry.ClosedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> MarkMessagesAsReadAsync(
        long userId,
        long enquiryId)
    {
        var enquiry = await _context.AdvertisementEnquiries
            .FirstOrDefaultAsync(x =>
                x.AdvertisementEnquiryID == enquiryId &&
                (x.SenderUserID == userId ||
                 x.RecipientUserID == userId));

        if (enquiry == null)
            return false;

        var messages = await _context.AdvertisementEnquiryMessages
            .Where(x =>
                x.AdvertisementEnquiryID == enquiryId &&
                x.SenderUserID != userId &&
                !x.IsRead)
            .ToListAsync();

        if (messages.Count == 0)
            return true;

        var readDate = DateTime.UtcNow;

        foreach (var message in messages)
        {
            message.IsRead = true;
            message.ReadDate = readDate;
        }

        await _context.SaveChangesAsync();

        return true;
    }

    private async Task<AdvertisementEnquiryResponse?> BuildResponseAsync(
        long enquiryId)
    {
        var enquiry = await _context.AdvertisementEnquiries
            .AsNoTracking()
            .Include(x => x.Advertisement)
            .Include(x => x.SenderUser)
            .Include(x => x.RecipientUser)
            .Include(x => x.Messages)
            .FirstOrDefaultAsync(x =>
                x.AdvertisementEnquiryID == enquiryId);

        return enquiry == null
            ? null
            : MapResponse(enquiry);
    }

    private static AdvertisementEnquiryResponse MapResponse(
        AdvertisementEnquiry enquiry)
    {
        return new AdvertisementEnquiryResponse
        {
            AdvertisementEnquiryID =
                enquiry.AdvertisementEnquiryID,

            AdvertisementID =
                enquiry.AdvertisementID,

            AdvertisementNumber =
                enquiry.Advertisement?.AdvertisementNumber
                ?? string.Empty,

            AdvertisementTitle =
                enquiry.Advertisement?.Title
                ?? string.Empty,

            SenderUserID =
                enquiry.SenderUserID,

            SenderName =
                GetUserDisplayName(enquiry.SenderUser),

            RecipientUserID =
                enquiry.RecipientUserID,

            RecipientName =
                GetUserDisplayName(enquiry.RecipientUser),

            Subject =
                enquiry.Subject,

            Message =
                enquiry.Message,

            Status =
                enquiry.Status,

            CreatedDate =
                enquiry.CreatedDate,

            LastRepliedDate =
                enquiry.LastRepliedDate,

            ClosedDate =
                enquiry.ClosedDate,

            MessageCount =
                enquiry.Messages?.Count ?? 0
        };
    }

    private static AdvertisementEnquiryDetailResponse MapDetail(
        AdvertisementEnquiry enquiry)
    {
        return new AdvertisementEnquiryDetailResponse
        {
            AdvertisementEnquiryID =
                enquiry.AdvertisementEnquiryID,

            AdvertisementID =
                enquiry.AdvertisementID,

            AdvertisementNumber =
                enquiry.Advertisement?.AdvertisementNumber
                ?? string.Empty,

            AdvertisementTitle =
                enquiry.Advertisement?.Title
                ?? string.Empty,

            SenderUserID =
                enquiry.SenderUserID,

            SenderName =
                GetUserDisplayName(enquiry.SenderUser),

            RecipientUserID =
                enquiry.RecipientUserID,

            RecipientName =
                GetUserDisplayName(enquiry.RecipientUser),

            Subject =
                enquiry.Subject,

            Message =
                enquiry.Message,

            Status =
                enquiry.Status,

            CreatedDate =
                enquiry.CreatedDate,

            LastRepliedDate =
                enquiry.LastRepliedDate,

            ClosedDate =
                enquiry.ClosedDate,

            ContactMobile =
                enquiry.ContactMobile,

            ContactEmail =
                enquiry.ContactEmail,

            Messages = enquiry.Messages
                .OrderBy(x => x.CreatedDate)
                .Select(x => new AdvertisementEnquiryMessageResponse
                {
                    AdvertisementEnquiryMessageID =
                        x.AdvertisementEnquiryMessageID,

                    AdvertisementEnquiryID =
                        x.AdvertisementEnquiryID,

                    SenderUserID =
                        x.SenderUserID,

                    SenderName =
                        GetUserDisplayName(x.SenderUser),

                    Message =
                        x.Message,

                    CreatedDate =
                        x.CreatedDate,

                    IsRead =
                        x.IsRead,

                    ReadDate =
                        x.ReadDate
                })
                .ToList()
        };
    }

    private static string GetUserDisplayName(User? user)
    {
        if (user == null)
            return string.Empty;

        if (!string.IsNullOrWhiteSpace(user.UserName))
            return user.UserName;

        return user.Email;
    }
}