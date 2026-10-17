using HRInfoAdvertisements.Application.Enquiries;
using HRInfoAdvertisements.Application.Enquiries.DTOs;
using System.Text.Json;
using HRInfoAdvertisements.Domain.Billing;
using HRInfoAdvertisements.Domain.Entities;
using HRInfoAdvertisements.Infrastructure.Data;
using HRInfoAdvertisements.Infrastructure.Billing;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Microsoft.EntityFrameworkCore.Storage;

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
        throw new KeyNotFoundException("Advertisement not found.");

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
        .FirstOrDefaultAsync(x => x.UserID == senderUserId);

    if (sender == null)
        throw new KeyNotFoundException("Sender user not found.");

    var now = DateTime.UtcNow;
    var monthStart = new DateTime(
        now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
    var nextMonthStart = monthStart.AddMonths(1);
    var monthKey = monthStart.ToString("yyyyMM");

    var senderLock =
        $"HRInfoAdvertisements:EnquiryLimit:Sender:{senderUserId}:{monthKey}";

    var advertisementLock =
        $"HRInfoAdvertisements:EnquiryLimit:Advertisement:{advertisement.AdvertisementID}:{monthKey}";

    string? rejectionType = null;
    int rejectionCount = 0;
    int rejectionLimit = 0;
    string? rejectionPackageCode = null;
    long? createdEnquiryId = null;

    await using (var transaction =
        await _context.Database.BeginTransactionAsync())
    {
        bool transactionCompleted = false;
        try
        {
            // Acquire locks in a consistent order for every request.
            foreach (var resource in new[] { senderLock, advertisementLock }
                .OrderBy(x => x, StringComparer.Ordinal))
            {
                await AcquireEnquiryLimitLockAsync(resource);
            }

            // Activate any paid packages whose scheduled start has arrived.
            foreach (var packageUserId in new[] { senderUserId, advertisement.UserID }
                .Distinct()
                .OrderBy(x => x))
            {
                await PackageEntitlementActivation.ActivateDueAsync(
                    _context, packageUserId, now);
            }
            // Resolve package entitlements while the limit locks are held.
            var senderPackage =
                await ResolveEnquiryPackageAsync(senderUserId, now);

            var ownerPackage =
                await ResolveEnquiryPackageAsync(advertisement.UserID, now);

            var senderCount =
                await _context.AdvertisementEnquiries.CountAsync(x =>
                    x.SenderUserID == senderUserId &&
                    x.CreatedDate >= monthStart &&
                    x.CreatedDate < nextMonthStart);

            if (senderCount >= senderPackage.MonthlySenderEnquiryLimit)
            {
                rejectionType = "SENDER_MONTHLY_LIMIT";
                rejectionCount = senderCount;
                rejectionLimit = senderPackage.MonthlySenderEnquiryLimit;
                rejectionPackageCode = senderPackage.PackageCode;
            }
            else
            {
                var advertisementCount =
                    await _context.AdvertisementEnquiries.CountAsync(x =>
                        x.AdvertisementID == advertisement.AdvertisementID &&
                        x.CreatedDate >= monthStart &&
                        x.CreatedDate < nextMonthStart);

                if (advertisementCount >=
                    ownerPackage.MonthlyAdvertisementEnquiryLimit)
                {
                    rejectionType = "ADVERTISEMENT_MONTHLY_LIMIT";
                    rejectionCount = advertisementCount;
                    rejectionLimit =
                        ownerPackage.MonthlyAdvertisementEnquiryLimit;
                    rejectionPackageCode = ownerPackage.PackageCode;
                }
            }

            if (rejectionType == null)
            {
                var enquiry = new AdvertisementEnquiry
                {
                    AdvertisementID = advertisement.AdvertisementID,
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

                _context.AuditLogs.Add(new AuditLog
                {
                    UserID = senderUserId,
                    Action = "ENQUIRY_CREATED",
                    EntityName = "AdvertisementEnquiry",
                    EntityID = enquiry.AdvertisementEnquiryID.ToString(),
                    NewValues = JsonSerializer.Serialize(new
                    {
                        AdvertisementEnquiryID =
                            enquiry.AdvertisementEnquiryID,
                        AdvertisementID = advertisement.AdvertisementID,
                        SenderUserID = senderUserId,
                        RecipientUserID = advertisement.UserID,
                        Subject = enquiry.Subject,
                        CreatedDate = enquiry.CreatedDate
                    }),
                    CreatedDate = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                transactionCompleted = true;
createdEnquiryId = enquiry.AdvertisementEnquiryID;

                return await BuildResponseAsync(
                    enquiry.AdvertisementEnquiryID)
                    ?? throw new InvalidOperationException(
                        "Unable to retrieve the created enquiry.");
            }

            // Release the limit locks before persisting the rejection audit.
            await transaction.RollbackAsync();
            transactionCompleted = true;
        }
        catch
        {
            if (!transactionCompleted)
    {
        try
        {
            await transaction.RollbackAsync();
        }
        catch
        {
            // Preserve the original exception.
        }
    }

            throw;
        }
    }

    if (createdEnquiryId.HasValue)
{
    return await BuildResponseAsync(createdEnquiryId.Value)
        ?? throw new InvalidOperationException(
            "The enquiry was created, but its response could not be retrieved.");
}

    // Keep rejected-limit audit records outside the rolled-back transaction.
    await WriteEnquiryLimitAuditAsync(
        senderUserId,
        advertisement.AdvertisementID,
        rejectionType!,
        rejectionCount,
        rejectionLimit,
        rejectionPackageCode!,
        monthStart);

    if (rejectionType == "SENDER_MONTHLY_LIMIT")
    {
        throw new InvalidOperationException(
            $"Your monthly enquiry limit of {rejectionLimit} has been reached.");
    }

    throw new InvalidOperationException(
        "This advertisement has reached its monthly enquiry limit.");
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

    public async Task<AdvertisementEnquiryDetailResponse?> GetByIdForAdminAsync(
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
            x.AdvertisementEnquiryID == enquiryId);

    if (enquiry == null)
        return null;

    return MapDetail(enquiry);
}

    public async Task<List<AdvertisementEnquiryResponse>>
    GetAllForAdminAsync()
{
    var enquiries = await _context.AdvertisementEnquiries
        .AsNoTracking()
        .Include(x => x.Advertisement)
        .Include(x => x.SenderUser)
        .Include(x => x.RecipientUser)
        .Include(x => x.Messages)
        .OrderByDescending(x => x.CreatedDate)
        .ToListAsync();

    return enquiries
        .Select(MapResponse)
        .ToList();
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

    var now = DateTime.UtcNow;
    var monthStart = new DateTime(
        now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
    var nextMonthStart = monthStart.AddMonths(1);
    var monthKey = monthStart.ToString("yyyyMM");

    var replyLock =
        $"HRInfoAdvertisements:ReplyLimit:User:{userId}:{monthKey}";

    string? packageCode = null;
    var currentCount = 0;
    var configuredLimit = 0;
    var limitReached = false;
    AdvertisementEnquiryMessage? savedMessage = null;

    await using (var transaction =
        await _context.Database.BeginTransactionAsync())
    {
        var transactionCompleted = false;

        try
        {
            await AcquireEnquiryLimitLockAsync(replyLock);
            await PackageEntitlementActivation.ActivateDueAsync(_context, userId, now);

            var package = await ResolveEnquiryPackageAsync(userId, now);
            packageCode = package.PackageCode;
            configuredLimit = package.MonthlyReplyLimit;

            currentCount = await _context.AdvertisementEnquiryMessages
                .CountAsync(x =>
                    x.SenderUserID == userId &&
                    x.CreatedDate >= monthStart &&
                    x.CreatedDate < nextMonthStart);

            if (currentCount >= configuredLimit)
            {
                limitReached = true;

                await transaction.RollbackAsync();
                transactionCompleted = true;
            }
            else
            {
                var message = new AdvertisementEnquiryMessage
                {
                    AdvertisementEnquiryID =
                        enquiry.AdvertisementEnquiryID,
                    SenderUserID = userId,
                    Message = request.Message.Trim(),
                    CreatedDate = now,
                    IsRead = false
                };

                _context.AdvertisementEnquiryMessages.Add(message);

                enquiry.LastRepliedDate = now;
                enquiry.Status = "OPEN";

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                transactionCompleted = true;
                savedMessage = message;
            }
        }
        catch
        {
            if (!transactionCompleted)
            {
                try
                {
                    await transaction.RollbackAsync();
                }
                catch
                {
                    // Preserve the original exception.
                }
            }

            throw;
        }
    }

    if (limitReached)
    {
        // Persist the audit record after releasing the limit lock.
        _context.AuditLogs.Add(new AuditLog
        {
            UserID = userId,
            Action = "REPLY_LIMIT_REJECTED",
            EntityName = "AdvertisementEnquiry",
            EntityID = enquiryId.ToString(),
            NewValues = JsonSerializer.Serialize(new
            {
                UserID = userId,
                AdvertisementEnquiryID = enquiryId,
                LimitType = "USER_MONTHLY_REPLY_LIMIT",
                CurrentCount = currentCount,
                ConfiguredLimit = configuredLimit,
                PackageCode = packageCode,
                MonthStartUtc = monthStart
            }),
            CreatedDate = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        throw new InvalidOperationException(
            $"Your monthly reply limit of {configuredLimit} has been reached.");
    }

    var sender = await _context.Users
        .AsNoTracking()
        .FirstOrDefaultAsync(x => x.UserID == userId);

    return new AdvertisementEnquiryMessageResponse
    {
        AdvertisementEnquiryMessageID =
            savedMessage!.AdvertisementEnquiryMessageID,
        AdvertisementEnquiryID =
            savedMessage.AdvertisementEnquiryID,
        SenderUserID = savedMessage.SenderUserID,
        SenderName = GetUserDisplayName(sender),
        Message = savedMessage.Message,
        CreatedDate = savedMessage.CreatedDate,
        IsRead = savedMessage.IsRead,
        ReadDate = savedMessage.ReadDate
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

    
private async Task<ResolvedEnquiryPackage> ResolveEnquiryPackageAsync(
    long userId,
    DateTime now)
{
    var paidPackage = await _context.UserPackageEntitlements
        .AsNoTracking()
        .Where(x =>
            x.UserID == userId &&
            x.Status == UserPackageEntitlementStatuses.Active &&
            x.StartDate <= now &&
            (x.EndDate == null || x.EndDate > now) &&
            x.EnquiryPackage.PackageCode != "FREE" &&
            x.Payments.Any(p =>
                p.Status == PaymentTransactionStatuses.Confirmed &&
                p.ConfirmedDate != null &&
                p.RefundedDate == null))
        .OrderByDescending(x => x.StartDate)
        .Select(x => new ResolvedEnquiryPackage(
            x.EnquiryPackage.PackageCode,
            x.MonthlySenderEnquiryLimit,
            x.MonthlyAdvertisementEnquiryLimit,
            x.MonthlyReplyLimit))
        .FirstOrDefaultAsync();

    if (paidPackage != null)
        return paidPackage;

    var freePackage = await _context.EnquiryPackages
        .AsNoTracking()
        .Where(x =>
            x.PackageCode == "FREE" &&
            x.IsActive)
        .Select(x => new ResolvedEnquiryPackage(
            x.PackageCode,
            x.MonthlySenderEnquiryLimit,
            x.MonthlyAdvertisementEnquiryLimit,
            x.MonthlyReplyLimit))
        .FirstOrDefaultAsync();

    return freePackage ?? throw new InvalidOperationException(
        "The FREE enquiry package is unavailable. Please contact support.");
}

private sealed record ResolvedEnquiryPackage(
    string PackageCode,
    int MonthlySenderEnquiryLimit,
    int MonthlyAdvertisementEnquiryLimit,
    int MonthlyReplyLimit);


private async Task AcquireEnquiryLimitLockAsync(
    string resource)
{
    var transaction = _context.Database.CurrentTransaction
        ?? throw new InvalidOperationException(
            "An active database transaction is required for enquiry limit checks.");

    await using var command =
        _context.Database.GetDbConnection().CreateCommand();

    command.Transaction = transaction.GetDbTransaction();
    command.CommandType = CommandType.Text;
    command.CommandText = """
        DECLARE @result int;

        EXEC @result = sys.sp_getapplock
            @Resource = @resource,
            @LockMode = 'Exclusive',
            @LockOwner = 'Transaction',
            @LockTimeout = 10000,
            @DbPrincipal = 'public';

        SELECT @result;
        """;

    var parameter = command.CreateParameter();
    parameter.ParameterName = "@resource";
    parameter.DbType = DbType.String;
    parameter.Size = 255;
    parameter.Value = resource;
    command.Parameters.Add(parameter);

    var result = await command.ExecuteScalarAsync();
    var lockResult = Convert.ToInt32(result);

    // SQL Server returns a negative value if the lock
    // cannot be acquired. Never continue without the lock.
    if (lockResult < 0)
    {
        throw new InvalidOperationException(
            "The enquiry limit could not be checked because of concurrent activity. Please try again.");
    }
}

    private async Task WriteEnquiryLimitAuditAsync(
        long senderUserId,
        long advertisementId,
        string limitType,
        int currentCount,
        int configuredLimit,
        string packageCode,
        DateTime monthStart)
    {
        _context.AuditLogs.Add(new AuditLog
        {
            UserID = senderUserId,
            Action = "ENQUIRY_LIMIT_REJECTED",
            EntityName = "AdvertisementEnquiry",
            EntityID = advertisementId.ToString(),
            NewValues = JsonSerializer.Serialize(new
            {
                SenderUserID = senderUserId,
                AdvertisementID = advertisementId,
                LimitType = limitType,
                CurrentCount = currentCount,
                ConfiguredLimit = configuredLimit,
                PackageCode = packageCode,
                MonthStartUtc = monthStart
            }),
            CreatedDate = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
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
    1 + (enquiry.Messages?.Count ?? 0),

LastMessageSenderUserID =
    enquiry.Messages?
        .OrderByDescending(x => x.CreatedDate)
        .ThenByDescending(x => x.AdvertisementEnquiryMessageID)
        .Select(x => (long?)x.SenderUserID)
        .FirstOrDefault()
    ?? enquiry.SenderUserID
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
