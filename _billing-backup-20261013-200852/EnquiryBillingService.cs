using HRInfoAdvertisements.Application.Billing;
using HRInfoAdvertisements.Application.Billing.DTOs;
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
}
