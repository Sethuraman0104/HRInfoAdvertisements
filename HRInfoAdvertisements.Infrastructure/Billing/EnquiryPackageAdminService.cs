
using HRInfoAdvertisements.Application.Billing;
using HRInfoAdvertisements.Application.Billing.DTOs;
using HRInfoAdvertisements.Domain.Billing;
using HRInfoAdvertisements.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRInfoAdvertisements.Infrastructure.Billing;

public sealed class EnquiryPackageAdminService : IEnquiryPackageAdminService
{
    private readonly ApplicationDbContext _context;

    public EnquiryPackageAdminService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<EnquiryPackageResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var packages = await _context.EnquiryPackages
            .AsNoTracking()
            .OrderBy(x => x.EnquiryPackageID)
            .ToListAsync(cancellationToken);

        return packages.Select(Map).ToList();
    }

    public async Task<EnquiryPackageResponse?> UpdateAsync(
        int enquiryPackageId,
        UpdateEnquiryPackageRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (enquiryPackageId <= 0)
        {
            throw new InvalidOperationException("Invalid package ID.");
        }

        var name = request.Name?.Trim();
        var description = request.Description?.Trim();
        var currency = request.CurrencyCode?.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(name) || name.Length > 100)
        {
            throw new InvalidOperationException(
                "Package name is required and cannot exceed 100 characters.");
        }

        if (description?.Length > 1000)
        {
            throw new InvalidOperationException(
                "Package description cannot exceed 1000 characters.");
        }

        if (request.Price < 0)
        {
            throw new InvalidOperationException(
                "Package price cannot be negative.");
        }

        if (request.MonthlySenderEnquiryLimit < 0 ||
            request.MonthlyAdvertisementEnquiryLimit < 0 ||
            request.MonthlyReplyLimit < 0)
        {
            throw new InvalidOperationException(
                "Monthly enquiry and reply limits cannot be negative.");
        }

        if (request.ValidityDays is <= 0)
        {
            throw new InvalidOperationException(
                "Validity must be greater than zero or null.");
        }

        if (currency == null ||
            currency.Length != 3 ||
            currency.Any(c => c < 'A' || c > 'Z'))
        {
            throw new InvalidOperationException(
                "Currency must be a three-letter ISO-style code.");
        }

        var package = await _context.EnquiryPackages
            .SingleOrDefaultAsync(
                x => x.EnquiryPackageID == enquiryPackageId,
                cancellationToken);

        if (package == null)
        {
            return null;
        }

        var isFree = string.Equals(
            package.PackageCode,
            "FREE",
            StringComparison.OrdinalIgnoreCase);

        if (isFree)
        {
            if (request.Price != 0m)
            {
                throw new InvalidOperationException(
                    "The FREE package price must remain zero.");
            }

            if (!request.IsActive)
            {
                throw new InvalidOperationException(
                    "The FREE package must remain active.");
            }
        }
        else if (request.IsActive &&
                 (request.Price <= 0m ||
                  (request.MonthlySenderEnquiryLimit == 0 &&
                   request.MonthlyAdvertisementEnquiryLimit == 0 &&
                   request.MonthlyReplyLimit == 0) ||
                  !request.ValidityDays.HasValue ||
                  request.ValidityDays.Value <= 0))
        {
            throw new InvalidOperationException(
                "An active paid package requires a positive price, " +
                "at least one positive monthly enquiry or reply limit, " +
                "and a positive validity period.");
        }

        var duplicateName = await _context.EnquiryPackages.AnyAsync(
            x => x.EnquiryPackageID != enquiryPackageId &&
                 x.Name == name,
            cancellationToken);

        if (duplicateName)
        {
            throw new InvalidOperationException(
                "Another package already uses this name.");
        }

        package.Name = name;
        package.Description = string.IsNullOrWhiteSpace(description)
            ? null
            : description;

        package.MonthlySenderEnquiryLimit =
            request.MonthlySenderEnquiryLimit;

        package.MonthlyAdvertisementEnquiryLimit =
            request.MonthlyAdvertisementEnquiryLimit;

        package.MonthlyReplyLimit =
            request.MonthlyReplyLimit;

        package.ValidityDays = request.ValidityDays;
        package.Price = request.Price;
        package.CurrencyCode = currency;
        package.IsActive = request.IsActive;
        package.ModifiedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Map(package);
    }

    private static EnquiryPackageResponse Map(EnquiryPackage package)
    {
        return new EnquiryPackageResponse
        {
            EnquiryPackageID = package.EnquiryPackageID,
            PackageCode = package.PackageCode,
            Name = package.Name,
            Description = package.Description,

            MonthlySenderEnquiryLimit =
                package.MonthlySenderEnquiryLimit,

            MonthlyAdvertisementEnquiryLimit =
                package.MonthlyAdvertisementEnquiryLimit,

            MonthlyReplyLimit =
                package.MonthlyReplyLimit,

            ValidityDays = package.ValidityDays,
            Price = package.Price,
            CurrencyCode = package.CurrencyCode,
            IsActive = package.IsActive
        };
    }
}
