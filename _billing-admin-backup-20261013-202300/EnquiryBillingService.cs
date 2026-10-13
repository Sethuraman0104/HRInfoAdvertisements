using HRInfoAdvertisements.Application.Billing;
using HRInfoAdvertisements.Application.Billing.DTOs;
using HRInfoAdvertisements.Domain.Billing;
using HRInfoAdvertisements.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRInfoAdvertisements.Infrastructure.Billing;

public sealed class EnquiryBillingService : IEnquiryBillingService
{
    private readonly ApplicationDbContext _context;

    public EnquiryBillingService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<EnquiryPackageResponse>>
        GetAvailablePackagesAsync(
            CancellationToken cancellationToken = default)
    {
        return await _context.EnquiryPackages
            .AsNoTracking()
            .Where(package => package.IsActive)
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
                ValidityDays = package.ValidityDays,
                Price = package.Price,
                CurrencyCode = package.CurrencyCode
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

        var package = await _context.EnquiryPackages
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
             package.MonthlyAdvertisementEnquiryLimit <= 0))
        {
            throw new InvalidOperationException(
                "This package is not configured for paid purchase.");
        }

        var hasPendingRequest = await _context.PackagePurchaseRequests
            .AnyAsync(
                x => x.UserID == userId &&
                     x.Status == PackagePurchaseRequestStatuses.Pending,
                cancellationToken);

        if (hasPendingRequest)
        {
            throw new InvalidOperationException(
                "You already have a pending package purchase request.");
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
