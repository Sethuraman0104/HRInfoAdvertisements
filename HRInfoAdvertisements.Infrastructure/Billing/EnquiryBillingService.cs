using HRInfoAdvertisements.Application.Billing;
using HRInfoAdvertisements.Application.Billing.DTOs;
using HRInfoAdvertisements.Application.Services;
using HRInfoAdvertisements.Domain.Billing;
using HRInfoAdvertisements.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using HRInfoAdvertisements.Domain.Entities;

namespace HRInfoAdvertisements.Infrastructure.Billing;

public sealed class EnquiryBillingService : IEnquiryBillingService
{
    private readonly ApplicationDbContext _context;
    private readonly IApplicationSettingsService _settings;

    public EnquiryBillingService(
        ApplicationDbContext context,
        IApplicationSettingsService settings)
    {
        _context = context;
        _settings = settings;
    }

    public async Task<MyPackageBalanceResponse> GetMyPackageBalanceAsync(
        long userId,
        CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(userId));
        }

        var now = DateTime.UtcNow;
        var periodStart = new DateTime(
            now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var periodEnd = periodStart.AddMonths(1);

        var response = new MyPackageBalanceResponse
        {
            UsagePeriodStartUtc = periodStart,
            UsagePeriodEndUtc = periodEnd
        };

        var paidEntitlement = await _context.UserPackageEntitlements
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
            .Select(x => new
            {
                x.UserPackageEntitlementID,
                x.EnquiryPackage.PackageCode,
                x.EnquiryPackage.Name,
                x.Status,
                x.StartDate,
                x.EndDate,
                x.MonthlyAdvertisementPostLimit,
                x.MonthlySenderEnquiryLimit,
                x.MonthlyAdvertisementEnquiryLimit,
                x.MonthlyReplyLimit
            })
            .FirstOrDefaultAsync(cancellationToken);

        int postLimit;
        int senderLimit;
        int receivedLimit;
        int replyLimit;

        if (paidEntitlement != null)
        {
            var contributions = await _context.PackageEntitlementContributions
                .AsNoTracking()
                .Where(x =>
                    x.UserPackageEntitlementID ==
                        paidEntitlement.UserPackageEntitlementID &&
                    (
                        x.IsLegacyBaseline ||
                        (x.PaymentTransaction != null &&
                         x.PaymentTransaction.Status ==
                            PaymentTransactionStatuses.Confirmed &&
                         x.PaymentTransaction.ConfirmedDate != null &&
                         x.PaymentTransaction.RefundedDate == null)
                    ))
                .OrderBy(x => x.IsLegacyBaseline ? 0 : 1)
                .ThenBy(x => x.CreatedDate)
                .Select(x => new
                {
                    x.PackageCode,
                    x.PackageName
                })
                .ToListAsync(cancellationToken);

            response.PackageCode = contributions.Count > 0
                ? string.Join(" + ", contributions
                    .Select(x => x.PackageCode)
                    .Distinct(StringComparer.OrdinalIgnoreCase))
                : paidEntitlement.PackageCode;

            response.PackageName = contributions.Count > 0
                ? string.Join(" + ", contributions
                    .Select(x => x.PackageName)
                    .Distinct(StringComparer.OrdinalIgnoreCase))
                : paidEntitlement.Name;

            response.PackageCombination = response.PackageName;
            response.EntitlementStatus = paidEntitlement.Status;
            response.StartDate = paidEntitlement.StartDate;
            response.EndDate = paidEntitlement.EndDate;
            response.IsPaidPackage = true;

            postLimit = paidEntitlement.MonthlyAdvertisementPostLimit;
            senderLimit = paidEntitlement.MonthlySenderEnquiryLimit;
            receivedLimit = paidEntitlement.MonthlyAdvertisementEnquiryLimit;
            replyLimit = paidEntitlement.MonthlyReplyLimit;
        }
        else
        {
            var freePackage = await _context.EnquiryPackages
                .AsNoTracking()
                .Where(x => x.PackageCode == "FREE" && x.IsActive)
                .Select(x => new
                {
                    x.PackageCode,
                    x.Name,
                    x.MonthlyAdvertisementPostLimit,
                    x.MonthlySenderEnquiryLimit,
                    x.MonthlyAdvertisementEnquiryLimit,
                    x.MonthlyReplyLimit
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (freePackage == null)
            {
                throw new InvalidOperationException(
                    "The FREE package is unavailable. Please contact support.");
            }

            response.PackageCode = freePackage.PackageCode;
            response.PackageName = freePackage.Name;
            response.PackageCombination = freePackage.Name;
            response.EntitlementStatus = "FREE";
            response.IsPaidPackage = false;

            postLimit = freePackage.MonthlyAdvertisementPostLimit;
            senderLimit = freePackage.MonthlySenderEnquiryLimit;
            receivedLimit = freePackage.MonthlyAdvertisementEnquiryLimit;
            replyLimit = freePackage.MonthlyReplyLimit;
        }
        var postsUsed = await _context.AuditLogs
            .AsNoTracking()
            .Where(x =>
                x.UserID == userId &&
                x.Action == "ADVERTISEMENT_SUBMITTED" &&
                x.EntityName == "Advertisement" &&
                x.EntityID != null &&
                x.CreatedDate >= periodStart &&
                x.CreatedDate < periodEnd)
            .Select(x => x.EntityID)
            .Distinct()
            .CountAsync(cancellationToken);

        var enquiriesSent = await _context.AdvertisementEnquiries
            .AsNoTracking()
            .CountAsync(x =>
                x.SenderUserID == userId &&
                x.CreatedDate >= periodStart &&
                x.CreatedDate < periodEnd,
                cancellationToken);

        var repliesSent = await _context.AdvertisementEnquiryMessages
            .AsNoTracking()
            .CountAsync(x =>
                x.SenderUserID == userId &&
                x.CreatedDate >= periodStart &&
                x.CreatedDate < periodEnd,
                cancellationToken);

        response.AdvertisementPosts = new PackageUsageBalance
        {
            Limit = postLimit,
            Used = postsUsed
        };

        response.EnquiriesSent = new PackageUsageBalance
        {
            Limit = senderLimit,
            Used = enquiriesSent
        };

        response.RepliesSent = new PackageUsageBalance
        {
            Limit = replyLimit,
            Used = repliesSent
        };

        var ownedAdvertisements = await _context.Advertisements
            .AsNoTracking()
            .Where(x => x.UserID == userId)
            .Select(x => new
            {
                x.AdvertisementID,
                x.Title
            })
            .ToListAsync(cancellationToken);

        var ownedAdvertisementIds = ownedAdvertisements
            .Select(x => x.AdvertisementID)
            .ToList();

        var receivedCounts = ownedAdvertisementIds.Count == 0
            ? new List<(long AdvertisementID, int Used)>()
            : (await _context.AdvertisementEnquiries
                .AsNoTracking()
                .Where(x =>
                    ownedAdvertisementIds.Contains(x.AdvertisementID) &&
                    x.CreatedDate >= periodStart &&
                    x.CreatedDate < periodEnd)
                .GroupBy(x => x.AdvertisementID)
                .Select(g => new
                {
                    AdvertisementID = g.Key,
                    Used = g.Count()
                })
                .ToListAsync(cancellationToken))
                .Select(x => (x.AdvertisementID, x.Used))
                .ToList();

        var receivedCountByAdvertisement = receivedCounts.ToDictionary(
            x => x.AdvertisementID,
            x => x.Used);

        response.AdvertisementEnquiriesReceived = ownedAdvertisements
            .Select(advertisement => new AdvertisementEnquiryUsageBalance
            {
                AdvertisementID = advertisement.AdvertisementID,
                AdvertisementTitle = advertisement.Title,
                Limit = receivedLimit,
                Used = receivedCountByAdvertisement.TryGetValue(
                    advertisement.AdvertisementID,
                    out var used)
                    ? used
                    : 0
            })
            .OrderBy(x => x.AdvertisementTitle)
            .ToList();

        return response;
    }
    public async Task<IReadOnlyList<EnquiryPackageResponse>>
        GetAvailablePackagesAsync(
            CancellationToken cancellationToken = default)
    {
        var paidPackagesEnabled =
            await _settings.GetBoolAsync("PAID_PACKAGES_ENABLED", cancellationToken) ?? false;

        return await _context.EnquiryPackages
            .AsNoTracking()
            .Where(package => package.IsActive && (paidPackagesEnabled || package.PackageCode == "FREE"))
            .OrderBy(package => package.Price)
            .ThenBy(package => package.EnquiryPackageID)
            .Select(package => new EnquiryPackageResponse
            {
                EnquiryPackageID = package.EnquiryPackageID,
                PackageCode = package.PackageCode,
                Name = package.Name,
                Description = package.Description,
                MonthlySenderEnquiryLimit =
                    package.MonthlySenderEnquiryLimit,
                MonthlyAdvertisementEnquiryLimit =
                    package.MonthlyAdvertisementEnquiryLimit,
                MonthlyReplyLimit = package.MonthlyReplyLimit,
MonthlyAdvertisementPostLimit =
    package.MonthlyAdvertisementPostLimit,
                ValidityDays = package.ValidityDays,
                Price = package.Price,
                CurrencyCode = package.CurrencyCode,
                IsActive = package.IsActive
            })
            .ToListAsync(cancellationToken);
    }

    
public async Task<PackagePurchaseRequestResponse>
    CreatePurchaseRequestAsync(
        long userId,
        int enquiryPackageId,
        CancellationToken cancellationToken = default)
{
    if (userId <= 0)
    {
        throw new InvalidOperationException(
            "The authenticated user ID is invalid.");
    }

    var paidPackagesEnabled =
        await _settings.GetBoolAsync(
            "PAID_PACKAGES_ENABLED",
            cancellationToken) ?? false;

    if (!paidPackagesEnabled)
    {
        throw new InvalidOperationException(
            "Paid package purchases are currently disabled.");
    }

    var package = await _context.EnquiryPackages
        .AsNoTracking()
        .SingleOrDefaultAsync(
            x => x.EnquiryPackageID == enquiryPackageId &&
                 x.IsActive,
            cancellationToken);

    if (package == null)
    {
        throw new InvalidOperationException(
            "The requested package does not exist or is inactive.");
    }

    if (string.Equals(
        package.PackageCode,
        "FREE",
        StringComparison.OrdinalIgnoreCase) ||
    package.Price <= 0m ||
    (package.MonthlySenderEnquiryLimit <= 0 &&
     package.MonthlyAdvertisementEnquiryLimit <= 0 &&
     package.MonthlyReplyLimit <= 0 &&
     package.MonthlyAdvertisementPostLimit <= 0))
{
    throw new InvalidOperationException(
        "This package is not configured for paid purchase.");
}

    await using var transaction =
        await _context.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable,
            cancellationToken);

    var hasOutstandingRequest =
        await _context.PackagePurchaseRequests
            .AnyAsync(
                x => x.UserID == userId &&
                     (x.Status == PackagePurchaseRequestStatuses.Pending ||
                      x.Status == PackagePurchaseRequestStatuses.Approved),
                cancellationToken);

    if (hasOutstandingRequest)
    {
        throw new InvalidOperationException(
            "You already have an outstanding package purchase request. " +
            "Complete or resolve it before requesting another package.");
    }

    var request = new PackagePurchaseRequest
    {
        UserID = userId,
        EnquiryPackageID = package.EnquiryPackageID,
        Amount = package.Price,
        CurrencyCode = package.CurrencyCode,
        Status = PackagePurchaseRequestStatuses.Pending,
        CreatedDate = DateTime.UtcNow
    };

    _context.PackagePurchaseRequests.Add(request);

    await _context.SaveChangesAsync(cancellationToken);
    await transaction.CommitAsync(cancellationToken);

    request.EnquiryPackage = package;

    return MapPurchaseRequest(request);
}


    public async Task<IReadOnlyList<PackagePurchaseRequestResponse>>
        GetMyPurchaseRequestsAsync(
            long userId,
            CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
        {
            throw new InvalidOperationException(
                "The authenticated user ID is invalid.");
        }

        var requests = await _context.PackagePurchaseRequests
            .AsNoTracking()
            .Include(x => x.EnquiryPackage)
            .Where(x => x.UserID == userId)
            .OrderByDescending(x => x.CreatedDate)
            .ThenByDescending(x => x.PackagePurchaseRequestID)
            .ToListAsync(cancellationToken);

        return requests
            .Select(MapPurchaseRequest)
            .ToList();
    }

    public async Task<IReadOnlyList<AdminPackagePurchaseRequestResponse>>
        GetPendingPurchaseRequestsForAdminAsync(
            CancellationToken cancellationToken = default)
    {
        var requests = await _context.PackagePurchaseRequests
            .AsNoTracking()
            .Include(x => x.EnquiryPackage)
            .Where(x => x.Status == PackagePurchaseRequestStatuses.Pending)
            .OrderBy(x => x.CreatedDate)
            .ThenBy(x => x.PackagePurchaseRequestID)
            .ToListAsync(cancellationToken);

        return requests
            .Select(MapAdminPurchaseRequest)
            .ToList();
    }

    public async Task<IReadOnlyList<AdminPackagePurchaseRequestResponse>>
    GetActionablePurchaseRequestsForAdminAsync(
        CancellationToken cancellationToken = default)
{
    var requests = await _context.PackagePurchaseRequests
        .AsNoTracking()
        .Include(x => x.EnquiryPackage)
        .Where(x =>
            x.Status == PackagePurchaseRequestStatuses.Pending ||
            x.Status == PackagePurchaseRequestStatuses.Approved)
        .OrderBy(x => x.CreatedDate)
        .ThenBy(x => x.PackagePurchaseRequestID)
        .ToListAsync(cancellationToken);

    return requests
        .Select(MapAdminPurchaseRequest)
        .ToList();
}

    public async Task<IReadOnlyList<AdminPackagePurchaseRequestResponse>>
        GetPurchaseRequestHistoryForAdminAsync(
            CancellationToken cancellationToken = default)
    {
        var requests = await _context.PackagePurchaseRequests
            .AsNoTracking()
            .Include(x => x.EnquiryPackage)
            .OrderByDescending(x => x.CreatedDate)
            .ThenByDescending(x => x.PackagePurchaseRequestID)
            .ToListAsync(cancellationToken);

        return requests
            .Select(MapAdminPurchaseRequest)
            .ToList();
    }

    public async Task<AdminPackagePurchaseRequestResponse?>
        CancelPurchaseRequestAsync(
            long adminUserId,
            long purchaseRequestId,
            CancellationToken cancellationToken = default)
    {
        if (adminUserId <= 0)
        {
            throw new InvalidOperationException(
                "The authenticated administrator ID is invalid.");
        }

        if (purchaseRequestId <= 0)
        {
            throw new InvalidOperationException(
                "The purchase request ID is invalid.");
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable,
                cancellationToken);

        var request = await _context.PackagePurchaseRequests
            .Include(x => x.EnquiryPackage)
            .SingleOrDefaultAsync(
                x => x.PackagePurchaseRequestID == purchaseRequestId,
                cancellationToken);

        if (request is null)
        {
            return null;
        }

        if (request.Status != PackagePurchaseRequestStatuses.Pending &&
            request.Status != PackagePurchaseRequestStatuses.Approved)
        {
            throw new InvalidOperationException(
                "Only pending or approved unpaid purchase requests can be cancelled.");
        }

        var now = DateTime.UtcNow;
        var oldStatus = request.Status;

        request.Status = PackagePurchaseRequestStatuses.Cancelled;
        request.ModifiedDate = now;

        _context.AuditLogs.Add(new AuditLog
        {
            UserID = adminUserId,
            Action = "PACKAGE_PURCHASE_CANCELLED",
            EntityName = "PackagePurchaseRequest",
            EntityID = request.PackagePurchaseRequestID.ToString(),
            OldValues = JsonSerializer.Serialize(new
            {
                Status = oldStatus
            }),
            NewValues = JsonSerializer.Serialize(new
            {
                Status = request.Status,
                ModifiedDate = request.ModifiedDate
            }),
            CreatedDate = now
        });

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return MapAdminPurchaseRequest(request);
    }
    public async Task<AdminPackagePurchaseRequestResponse?>
ReviewPurchaseRequestAsync(
long adminUserId,
long purchaseRequestId,
bool approve,
CancellationToken cancellationToken = default)
{
if (adminUserId <= 0)
{
throw new InvalidOperationException(
"The authenticated administrator ID is invalid.");
}

if (purchaseRequestId <= 0)
{
    throw new InvalidOperationException(
        "The purchase request ID is invalid.");
}

await using var transaction =
    await _context.Database.BeginTransactionAsync(
        System.Data.IsolationLevel.Serializable,
        cancellationToken);

var request = await _context.PackagePurchaseRequests
    .Include(x => x.EnquiryPackage)
    .SingleOrDefaultAsync(
        x => x.PackagePurchaseRequestID == purchaseRequestId,
        cancellationToken);

if (request == null)
{
    return null;
}

if (request.Status != PackagePurchaseRequestStatuses.Pending)
{
    throw new InvalidOperationException(
        "Only pending purchase requests can be reviewed.");
}

var now = DateTime.UtcNow;
var oldStatus = request.Status;

var newStatus = approve
    ? PackagePurchaseRequestStatuses.Approved
    : PackagePurchaseRequestStatuses.Rejected;

var oldValues = new
{
    Status = oldStatus,
    request.ModifiedDate
};

request.Status = newStatus;
request.ModifiedDate = now;

var newValues = new
{
    Status = request.Status,
    request.ModifiedDate
};

_context.AuditLogs.Add(new AuditLog
{
    UserID = adminUserId,
    Action = approve
        ? "PACKAGE_PURCHASE_APPROVED"
        : "PACKAGE_PURCHASE_REJECTED",
    EntityName = "PackagePurchaseRequest",
    EntityID = request.PackagePurchaseRequestID.ToString(),
    OldValues = JsonSerializer.Serialize(oldValues),
    NewValues = JsonSerializer.Serialize(newValues),
    CreatedDate = now
});

await _context.SaveChangesAsync(cancellationToken);
await transaction.CommitAsync(cancellationToken);

// Approval permits the request to proceed to payment.
// It does not confirm payment or activate an entitlement.
return MapAdminPurchaseRequest(request);

}

    public async Task<AdminPackagePurchaseRequestResponse?>
ConfirmPackagePaymentAsync(
long adminUserId,
long purchaseRequestId,
ConfirmPackagePaymentRequest request,
CancellationToken cancellationToken = default)
{
if (adminUserId <= 0)
{
throw new InvalidOperationException(
"The authenticated administrator ID is invalid.");
}

if (purchaseRequestId <= 0)
{
    throw new InvalidOperationException(
        "The purchase request ID is invalid.");
}

ArgumentNullException.ThrowIfNull(request);

if (string.IsNullOrWhiteSpace(request.PaymentMethod))
{
    throw new InvalidOperationException(
        "Payment method is required.");
}

var paymentMethod =
    request.PaymentMethod.Trim().ToUpperInvariant();

if (paymentMethod != "BANK_TRANSFER" &&
    paymentMethod != "BENEFITPAY")
{
    throw new InvalidOperationException(
        "Payment method must be BANK_TRANSFER or BENEFITPAY.");
}

var settingKey = paymentMethod == "BANK_TRANSFER"
    ? "BANK_TRANSFER_ENABLED"
    : "BENEFITPAY_ENABLED";

var paymentMethodEnabled =
    await _settings.GetBoolAsync(
        settingKey,
        cancellationToken) ?? false;

if (!paymentMethodEnabled)
{
    throw new InvalidOperationException(
        "The selected payment method is currently disabled.");
}

var reference =
    request.ProviderTransactionReference?.Trim();

if (string.IsNullOrWhiteSpace(reference) ||
    reference.Length > 200)
{
    throw new InvalidOperationException(
        "A payment reference is required and must not exceed 200 characters.");
}

await using var transaction =
    await _context.Database.BeginTransactionAsync(
        System.Data.IsolationLevel.Serializable,
        cancellationToken);

var purchaseRequest = await _context.PackagePurchaseRequests
    .Include(x => x.EnquiryPackage)
    .SingleOrDefaultAsync(
        x => x.PackagePurchaseRequestID == purchaseRequestId,
        cancellationToken);

if (purchaseRequest == null)
{
    return null;
}

if (purchaseRequest.Status !=
    PackagePurchaseRequestStatuses.Approved)
{
    throw new InvalidOperationException(
        "Only approved purchase requests can have payment confirmed.");
}

var package = purchaseRequest.EnquiryPackage;

if (!package.IsActive ||
    string.Equals(
        package.PackageCode,
        "FREE",
        StringComparison.OrdinalIgnoreCase) ||
    purchaseRequest.Amount <= 0m)
{
    throw new InvalidOperationException(
        "The purchase request is not eligible for paid activation.");
}

// Validate paid-package validity before creating an entitlement.
if (!package.ValidityDays.HasValue ||
    package.ValidityDays.Value <= 0 ||
    package.ValidityDays.Value > 3650)
{
    throw new InvalidOperationException(
        "The paid package must have a validity period between 1 and 3650 days before payment can be confirmed.");
}

var now = DateTime.UtcNow;

var newEntitlement = await _context.UserPackageEntitlements
    .Where(x =>
        x.UserID == purchaseRequest.UserID &&
        x.Status == UserPackageEntitlementStatuses.Active &&
        x.StartDate <= now &&
        x.EndDate.HasValue &&
        x.EndDate.Value > now)
    .OrderByDescending(x => x.StartDate)
    .FirstOrDefaultAsync(cancellationToken);

if (newEntitlement is null)
{
    newEntitlement = new UserPackageEntitlement
    {
        UserID = purchaseRequest.UserID,
        EnquiryPackageID = package.EnquiryPackageID,
        MonthlyAdvertisementPostLimit = 0,
        MonthlySenderEnquiryLimit = 0,
        MonthlyAdvertisementEnquiryLimit = 0,
        MonthlyReplyLimit = 0,
        StartDate = now,
        EndDate = now,
        Status = UserPackageEntitlementStatuses.Active,
        CreatedDate = now
    };

    _context.UserPackageEntitlements.Add(newEntitlement);
}

if (newEntitlement.UserPackageEntitlementID > 0)
{
    var hasContributions = await _context.PackageEntitlementContributions
        .AnyAsync(
            x => x.UserPackageEntitlementID ==
                newEntitlement.UserPackageEntitlementID,
            cancellationToken);

    var hasExistingAllowances =
        newEntitlement.MonthlyAdvertisementPostLimit > 0 ||
        newEntitlement.MonthlySenderEnquiryLimit > 0 ||
        newEntitlement.MonthlyAdvertisementEnquiryLimit > 0 ||
        newEntitlement.MonthlyReplyLimit > 0;

    if (!hasContributions && hasExistingAllowances)
    {
        var baselineStart = newEntitlement.StartDate < now
            ? newEntitlement.StartDate
            : now;

        var baselineEnd = newEntitlement.EndDate!.Value;

        if (baselineEnd <= baselineStart)
        {
            baselineStart = baselineEnd.AddDays(-1);
        }

        _context.PackageEntitlementContributions.Add(
            new PackageEntitlementContribution
            {
                UserPackageEntitlementID =
                    newEntitlement.UserPackageEntitlementID,
                EnquiryPackageID = newEntitlement.EnquiryPackageID,
                PackageCode = "LEGACY",
                PackageName = "Legacy package allowance",
                MonthlyAdvertisementPostLimit =
                    newEntitlement.MonthlyAdvertisementPostLimit,
                MonthlySenderEnquiryLimit =
                    newEntitlement.MonthlySenderEnquiryLimit,
                MonthlyAdvertisementEnquiryLimit =
                    newEntitlement.MonthlyAdvertisementEnquiryLimit,
                MonthlyReplyLimit =
                    newEntitlement.MonthlyReplyLimit,
                ValidityDays = Math.Max(
                    1,
                    (int)Math.Ceiling(
                        (baselineEnd - baselineStart).TotalDays)),
                ContributionStartDate = baselineStart,
                ContributionEndDate = baselineEnd,
                CreatedDate = now,
                IsLegacyBaseline = true
            });
    }
}
newEntitlement.MonthlyAdvertisementPostLimit =
    checked(newEntitlement.MonthlyAdvertisementPostLimit +
            package.MonthlyAdvertisementPostLimit);

newEntitlement.MonthlySenderEnquiryLimit =
    checked(newEntitlement.MonthlySenderEnquiryLimit +
            package.MonthlySenderEnquiryLimit);

newEntitlement.MonthlyAdvertisementEnquiryLimit =
    checked(newEntitlement.MonthlyAdvertisementEnquiryLimit +
            package.MonthlyAdvertisementEnquiryLimit);

newEntitlement.MonthlyReplyLimit =
    checked(newEntitlement.MonthlyReplyLimit +
            package.MonthlyReplyLimit);

newEntitlement.EndDate =
    (newEntitlement.EndDate.HasValue &&
     newEntitlement.EndDate.Value > now
        ? newEntitlement.EndDate.Value
        : now).AddDays(package.ValidityDays.Value);

newEntitlement.ModifiedDate = now;
newEntitlement.Status = UserPackageEntitlementStatuses.Active;
newEntitlement.EnquiryPackageID = package.EnquiryPackageID;

var payment = new PaymentTransaction
{
    UserPackageEntitlement = newEntitlement,
    Amount = purchaseRequest.Amount,
    CurrencyCode = purchaseRequest.CurrencyCode,
    PaymentMethod = paymentMethod,
    PaymentProvider = paymentMethod == "BANK_TRANSFER"
        ? "MANUAL"
        : "BENEFITPAY",
    ProviderTransactionReference = reference,
    Status = PaymentTransactionStatuses.Confirmed,
    CreatedDate = now,
    ConfirmedDate = now
};

_context.PaymentTransactions.Add(payment);

var contribution = new PackageEntitlementContribution
{
    UserPackageEntitlement = newEntitlement,
    PaymentTransaction = payment,
    EnquiryPackageID = package.EnquiryPackageID,
    PackageCode = package.PackageCode,
    PackageName = package.Name,
    MonthlyAdvertisementPostLimit = package.MonthlyAdvertisementPostLimit,
    MonthlySenderEnquiryLimit = package.MonthlySenderEnquiryLimit,
    MonthlyAdvertisementEnquiryLimit = package.MonthlyAdvertisementEnquiryLimit,
    MonthlyReplyLimit = package.MonthlyReplyLimit,
    ValidityDays = package.ValidityDays.Value,
    ContributionStartDate = now,
    ContributionEndDate = newEntitlement.EndDate.Value,
    CreatedDate = now
};

_context.PackageEntitlementContributions.Add(contribution);
var oldStatus = purchaseRequest.Status;

purchaseRequest.Status =
    PackagePurchaseRequestStatuses.Completed;

purchaseRequest.ModifiedDate = now;

// Save the entitlement and payment first so generated IDs are available.
// This save remains inside the transaction.
await _context.SaveChangesAsync(cancellationToken);

_context.AuditLogs.Add(new AuditLog
{
    UserID = adminUserId,
    Action = "PACKAGE_PAYMENT_CONFIRMED",
    EntityName = "PackagePurchaseRequest",
    EntityID = purchaseRequest.PackagePurchaseRequestID.ToString(),
    OldValues = JsonSerializer.Serialize(new
    {
        Status = oldStatus
    }),
    NewValues = JsonSerializer.Serialize(new
    {
        Status = purchaseRequest.Status,
        PaymentTransactionID = payment.PaymentTransactionID,
        PaymentMethod = payment.PaymentMethod,
        PaymentProvider = payment.PaymentProvider,
        ProviderTransactionReference =
            payment.ProviderTransactionReference,
        Amount = payment.Amount,
        CurrencyCode = payment.CurrencyCode,
        UserPackageEntitlementID =
            newEntitlement.UserPackageEntitlementID,
        ConfirmedDate = payment.ConfirmedDate,
        ModifiedDate = purchaseRequest.ModifiedDate
    }),
    CreatedDate = now
});

// Persist the audit record before committing the same transaction.
await _context.SaveChangesAsync(cancellationToken);
await transaction.CommitAsync(cancellationToken);

return MapAdminPurchaseRequest(purchaseRequest);

}

    public async Task<bool?> RefundPackagePaymentAsync(
        long adminUserId,
        long paymentTransactionId,
        RefundPackagePaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (adminUserId <= 0)
        {
            throw new InvalidOperationException(
                "A valid administrator identity is required.");
        }

        if (paymentTransactionId <= 0)
        {
            throw new InvalidOperationException(
                "A valid payment transaction ID is required.");
        }

        if (request is null || !request.RefundProcessedExternally)
        {
            throw new InvalidOperationException(
                "Confirm that the external refund has already been processed.");
        }

        var refundReference = request.ExternalRefundReference?.Trim();
        var reason = request.Reason?.Trim();

        if (string.IsNullOrWhiteSpace(refundReference) ||
            refundReference.Length > 200)
        {
            throw new InvalidOperationException(
                "A valid external refund reference is required (maximum 200 characters).");
        }

        if (string.IsNullOrWhiteSpace(reason) ||
            reason.Length < 5 ||
            reason.Length > 1000)
        {
            throw new InvalidOperationException(
                "The refund reason must be between 5 and 1000 characters.");
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable,
                cancellationToken);

        var payment = await _context.PaymentTransactions
            .Include(x => x.UserPackageEntitlement)
                .ThenInclude(x => x.Payments)
            .SingleOrDefaultAsync(
                x => x.PaymentTransactionID == paymentTransactionId,
                cancellationToken);

        if (payment is null)
        {
            return null;
        }

        var refundReferenceInUse = await _context.PaymentTransactions
            .AnyAsync(x =>
                x.PaymentTransactionID != paymentTransactionId &&
                x.ExternalRefundReference == refundReference,
                cancellationToken);

        if (refundReferenceInUse)
        {
            throw new InvalidOperationException(
                "This external refund reference has already been recorded.");
        }

        if (payment.Status != PaymentTransactionStatuses.Confirmed ||
            !payment.ConfirmedDate.HasValue ||
            payment.RefundedDate.HasValue)
        {
            throw new InvalidOperationException(
                "Only a confirmed payment that has not already been refunded can be refunded.");
        }

        var entitlement = payment.UserPackageEntitlement;
        var now = DateTime.UtcNow;

        var oldPaymentStatus = payment.Status;
        var oldRefundedDate = payment.RefundedDate;
        var oldEntitlementStatus = entitlement.Status;

        payment.Status = PaymentTransactionStatuses.Refunded;
        payment.RefundedDate = now;
        payment.ExternalRefundReference = refundReference;

        var hasContributionRecords = await _context.PackageEntitlementContributions
            .AnyAsync(
                x => x.UserPackageEntitlementID ==
                    entitlement.UserPackageEntitlementID,
                cancellationToken);

        if (hasContributionRecords)
        {
            var remainingContributions = await _context.PackageEntitlementContributions
                .AsNoTracking()
                .Where(x =>
                    x.UserPackageEntitlementID ==
                        entitlement.UserPackageEntitlementID &&
                    (x.IsLegacyBaseline ||
                     (x.PaymentTransaction != null &&
                      x.PaymentTransaction.Status ==
                          PaymentTransactionStatuses.Confirmed &&
                      x.PaymentTransaction.ConfirmedDate != null &&
                      x.PaymentTransaction.RefundedDate == null)))
                .Select(x => new
                {
                    x.MonthlyAdvertisementPostLimit,
                    x.MonthlySenderEnquiryLimit,
                    x.MonthlyAdvertisementEnquiryLimit,
                    x.MonthlyReplyLimit
                })
                .ToListAsync(cancellationToken);

            entitlement.MonthlyAdvertisementPostLimit =
                remainingContributions.Sum(x => x.MonthlyAdvertisementPostLimit);

            entitlement.MonthlySenderEnquiryLimit =
                remainingContributions.Sum(x => x.MonthlySenderEnquiryLimit);

            entitlement.MonthlyAdvertisementEnquiryLimit =
                remainingContributions.Sum(x => x.MonthlyAdvertisementEnquiryLimit);

            entitlement.MonthlyReplyLimit =
                remainingContributions.Sum(x => x.MonthlyReplyLimit);
        }
        // Keep the combined EndDate unchanged when a contribution is refunded.
        var hasOtherValidPayment = entitlement.Payments.Any(other =>
            other.PaymentTransactionID != payment.PaymentTransactionID &&
            other.Status == PaymentTransactionStatuses.Confirmed &&
            other.ConfirmedDate.HasValue &&
            !other.RefundedDate.HasValue);

        if (!hasOtherValidPayment &&
            entitlement.Status != UserPackageEntitlementStatuses.Expired)
        {
            entitlement.Status = UserPackageEntitlementStatuses.Cancelled;
        }

        entitlement.ModifiedDate = now;

        _context.AuditLogs.Add(new AuditLog
        {
            UserID = adminUserId,
            Action = "PACKAGE_PAYMENT_REFUNDED",
            EntityName = "PaymentTransaction",
            EntityID = payment.PaymentTransactionID.ToString(),
            OldValues = JsonSerializer.Serialize(new
            {
                Status = oldPaymentStatus,
                RefundedDate = oldRefundedDate,
                EntitlementStatus = oldEntitlementStatus
            }),
            NewValues = JsonSerializer.Serialize(new
            {
                Status = payment.Status,
                RefundedDate = payment.RefundedDate,
                ExternalRefundReference = refundReference,
                Reason = reason,
                Amount = payment.Amount,
                CurrencyCode = payment.CurrencyCode,
                UserPackageEntitlementID =
                    entitlement.UserPackageEntitlementID,
                EntitlementStatus = entitlement.Status,
                ModifiedDate = entitlement.ModifiedDate
            }),
            CreatedDate = now
        });

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return true;
    }
    private static AdminPackagePurchaseRequestResponse
        MapAdminPurchaseRequest(PackagePurchaseRequest request)
    {
        return new AdminPackagePurchaseRequestResponse
        {
            PackagePurchaseRequestID = request.PackagePurchaseRequestID,
            UserID = request.UserID,
            EnquiryPackageID = request.EnquiryPackageID,
            PackageCode = request.EnquiryPackage.PackageCode,
            PackageName = request.EnquiryPackage.Name,
            Amount = request.Amount,
            CurrencyCode = request.CurrencyCode,
            Status = request.Status,
            CreatedDate = request.CreatedDate,
            ModifiedDate = request.ModifiedDate
        };
    }
    private static PackagePurchaseRequestResponse MapPurchaseRequest(
        PackagePurchaseRequest request)
    {
        return new PackagePurchaseRequestResponse
        {
            PackagePurchaseRequestID = request.PackagePurchaseRequestID,
            EnquiryPackageID = request.EnquiryPackageID,
            PackageCode = request.EnquiryPackage.PackageCode,
            PackageName = request.EnquiryPackage.Name,
            Amount = request.Amount,
            CurrencyCode = request.CurrencyCode,
            Status = request.Status,
            CreatedDate = request.CreatedDate,
            ModifiedDate = request.ModifiedDate
        };
    }
}









