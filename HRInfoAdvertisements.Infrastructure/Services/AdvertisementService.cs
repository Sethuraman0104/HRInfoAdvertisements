using HRInfoAdvertisements.Application.DTOs.Advertisements;
using HRInfoAdvertisements.Application.DTOs.AdvertisementMedia;
using HRInfoAdvertisements.Application.Interfaces;
using HRInfoAdvertisements.Domain.Entities;
using HRInfoAdvertisements.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;

namespace HRInfoAdvertisements.Infrastructure.Services;

public class AdvertisementService : IAdvertisementService
{
    private readonly ApplicationDbContext _context;

    public AdvertisementService(
        ApplicationDbContext context)
    {
        _context = context;
    }

    // ============================================================
    // CREATE
    // ============================================================

    public async Task<AdvertisementResponse> CreateAsync(
        long userId,
        CreateAdvertisementRequest request)
    {
        // --------------------------------------------------------
        // Validate Category
        // --------------------------------------------------------

        var category = await _context.AdvertisementCategories
            .FirstOrDefaultAsync(x =>
                x.CategoryID == request.CategoryID &&
                x.IsActive);

        if (category == null)
        {
            throw new InvalidOperationException(
                "Advertisement category was not found or is inactive.");
        }

        // --------------------------------------------------------
        // Validate Advertisement Type
        // --------------------------------------------------------

        var type = await _context.AdvertisementTypes
            .FirstOrDefaultAsync(x =>
                x.AdvertisementTypeID ==
                request.AdvertisementTypeID &&
                x.IsActive);

        if (type == null)
        {
            throw new InvalidOperationException(
                "Advertisement type was not found or is inactive.");
        }

        // --------------------------------------------------------
        // Get Draft Status
        // --------------------------------------------------------

        var draftStatus =
            await _context.AdvertisementStatuses
                .FirstOrDefaultAsync(x =>
                    x.StatusCode == "DRAFT" &&
                    x.IsActive);

        if (draftStatus == null)
        {
            throw new InvalidOperationException(
                "Advertisement DRAFT status was not found or is inactive.");
        }

        // --------------------------------------------------------
        // Validate Contact Information
        //
        // Contact information may be saved while creating a draft.
        // Full submission validation is performed in SubmitAsync().
        // --------------------------------------------------------

        var whatsappNumber =
            NormalizeOptionalValue(request.WhatsAppNumber);

        var contactEmail =
            NormalizeOptionalValue(request.ContactEmail);

        ValidatePublicContactFlags(
            whatsappNumber,
            contactEmail,
            request.ShowWhatsAppToPublic,
            request.ShowEmailToPublic);

        // --------------------------------------------------------
        // Generate Advertisement Number
        // --------------------------------------------------------

        var advertisementNumber =
            await GenerateAdvertisementNumberAsync();

        // --------------------------------------------------------
        // Create Advertisement
        // --------------------------------------------------------

        var advertisement = new Advertisement
        {
            UserID = userId,

            CategoryID =
                request.CategoryID,

            AdvertisementTypeID =
                request.AdvertisementTypeID,

            StatusID =
                draftStatus.StatusID,

            AdvertisementNumber =
                advertisementNumber,

            Title =
                request.Title.Trim(),

            TitleAr =
                string.IsNullOrWhiteSpace(request.TitleAr)
                    ? null
                    : request.TitleAr.Trim(),

            Description =
                request.Description.Trim(),

            DescriptionAr =
                string.IsNullOrWhiteSpace(request.DescriptionAr)
                    ? null
                    : request.DescriptionAr.Trim(),

            Price =
                request.Price,

            CurrencyCode =
                string.IsNullOrWhiteSpace(request.CurrencyCode)
                    ? "BHD"
                    : request.CurrencyCode
                        .Trim()
                        .ToUpperInvariant(),

            IsNegotiable =
                request.IsNegotiable,

            CountryID =
                request.CountryID,

            StateID =
                request.StateID,

            CityID =
                request.CityID,

            AreaID =
                request.AreaID,

            AddressLine =
                string.IsNullOrWhiteSpace(request.AddressLine)
                    ? null
                    : request.AddressLine.Trim(),

            Latitude =
                request.Latitude,

            Longitude =
                request.Longitude,

            PlotNumber =
                string.IsNullOrWhiteSpace(request.PlotNumber)
                    ? null
                    : request.PlotNumber.Trim(),

            LandArea =
                request.LandArea,

            BuiltUpArea =
                request.BuiltUpArea,

            Bedrooms =
                request.Bedrooms,

            Bathrooms =
                request.Bathrooms,

            PropertyAge =
                request.PropertyAge,

            // ----------------------------------------------------
            // Contact Information
            // ----------------------------------------------------

            WhatsAppNumber =
                whatsappNumber,

            ContactEmail =
                contactEmail,

            ShowWhatsAppToPublic =
                request.ShowWhatsAppToPublic,

            ShowEmailToPublic =
                request.ShowEmailToPublic,

            // ----------------------------------------------------
            // Advertisement Flags / Dates
            // ----------------------------------------------------

            IsFeatured =
                false,

            FeaturedUntil =
                null,

            PublishedDate =
                null,

            ExpiryDate =
                request.ExpiryDate,

            CreatedDate =
                DateTime.UtcNow,

            CreatedBy =
                userId
        };

        _context.Advertisements.Add(advertisement);

        await _context.SaveChangesAsync();

        return await GetByIdAsync(
            advertisement.AdvertisementID,
            userId)
            ?? throw new InvalidOperationException(
                "Advertisement could not be retrieved after creation.");
    }


public async Task<AdvertisementResponse?>
    GetPublicAdvertisementByIdAsync(
        long advertisementId)
{
    var advertisement =
        await _context.Advertisements
            .AsNoTracking()

            .Include(x => x.Category)
            .Include(x => x.AdvertisementType)
            .Include(x => x.Status)

            .Include(x => x.Country)
            .Include(x => x.State)
            .Include(x => x.City)
            .Include(x => x.Area)

            .Include(x => x.Images)
            .Include(x => x.FeatureValues)

            .FirstOrDefaultAsync(x =>
                x.AdvertisementID == advertisementId
                &&
                x.Status.StatusCode == "PUBLISHED"
                &&
                x.Status.IsActive);

    if (advertisement is null)
    {
        return null;
    }

    return MapToResponse(
        advertisement,
        includePrivateContact: false);
}
    // ============================================================
    // GET BY ID
    // ============================================================

    public async Task<AdvertisementResponse?> GetByIdAsync(
        long advertisementId,
        long? userId = null)
    {
        var query =
            _context.Advertisements
                .AsNoTracking()
                .Include(x => x.Category)
                .Include(x => x.AdvertisementType)
                .Include(x => x.Status)
                .Include(x => x.Images)
                .Where(x =>
                    x.AdvertisementID == advertisementId);

        // --------------------------------------------------------
        // Owner can see own advertisement.
        // Other users can only see published advertisements.
        // --------------------------------------------------------

        if (userId.HasValue)
        {
            query = query.Where(x =>
                x.UserID == userId.Value ||
                x.Status.StatusCode == "PUBLISHED");
        }
        else
        {
            query = query.Where(x =>
                x.Status.StatusCode == "PUBLISHED");
        }

        var advertisement =
            await query.FirstOrDefaultAsync();

        if (advertisement == null)
        {
            return null;
        }

        // --------------------------------------------------------
        // Determine whether the current user owns the ad.
        // --------------------------------------------------------

        var isOwner =
            userId.HasValue &&
            advertisement.UserID == userId.Value;

        return MapToResponse(
            advertisement,
            includePrivateContact: isOwner);
    }

    // ============================================================
    // GET MY ADVERTISEMENTS
    // ============================================================

    public async Task<AdvertisementListResponse>
        GetMyAdvertisementsAsync(
            long userId,
            int pageNumber = 1,
            int pageSize = 20)
    {
        pageNumber =
            pageNumber < 1
                ? 1
                : pageNumber;

        pageSize =
            pageSize < 1
                ? 20
                : Math.Min(pageSize, 100);

        var query =
            _context.Advertisements
                .AsNoTracking()
                .Include(x => x.Category)
                .Include(x => x.AdvertisementType)
                .Include(x => x.Status)
                .Include(x => x.Images)
                .Where(x =>
                    x.UserID == userId);

        var totalRecords =
            await query.CountAsync();

        var items =
            await query
                .OrderByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

        return new AdvertisementListResponse
        {
            Items =
                items
                    .Select(x =>
                        MapToResponse(
                            x,
                            includePrivateContact: true))
                    .ToList(),

            PageNumber =
                pageNumber,

            PageSize =
                pageSize,

            TotalRecords =
                totalRecords,

            TotalPages =
                (int)Math.Ceiling(
                    totalRecords /
                    (double)pageSize)
        };
    }

    // ============================================================
    // GET PUBLISHED ADVERTISEMENTS
    // ============================================================

    public async Task<AdvertisementListResponse>
        GetPublishedAdvertisementsAsync(
            int pageNumber = 1,
            int pageSize = 20,
            string? search = null,
            int? categoryId = null,
            int? typeId = null,
            int? countryId = null,
            int? stateId = null,
            int? cityId = null,
            int? areaId = null,
            decimal? minPrice = null,
            decimal? maxPrice = null)
    {
        pageNumber =
            pageNumber < 1
                ? 1
                : pageNumber;

        pageSize =
            pageSize < 1
                ? 20
                : Math.Min(pageSize, 100);

        var query =
    _context.Advertisements
        .AsNoTracking()
        .Include(x => x.Category)
        .Include(x => x.AdvertisementType)
        .Include(x => x.Status)
        .Include(x => x.Images)
        .Include(x => x.Country)
        .Include(x => x.State)
        .Include(x => x.City)
        .Include(x => x.Area)
        .Where(x =>
    x.Status.StatusCode == "PUBLISHED" &&
    x.Status.IsActive &&
    (!x.ExpiryDate.HasValue ||
     x.ExpiryDate.Value.Date >= DateTime.Today));

        // --------------------------------------------------------
        // Search
        // --------------------------------------------------------

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchText =
                search.Trim();

            query = query.Where(x =>
                x.Title.Contains(searchText) ||
                x.Description.Contains(searchText) ||
                x.AdvertisementNumber.Contains(searchText));
        }

        // --------------------------------------------------------
        // Category
        // --------------------------------------------------------

        if (categoryId.HasValue)
        {
            query = query.Where(x =>
                x.CategoryID == categoryId.Value);
        }

        // --------------------------------------------------------
        // Type
        // --------------------------------------------------------

        if (typeId.HasValue)
        {
            query = query.Where(x =>
                x.AdvertisementTypeID ==
                typeId.Value);
        }

        // --------------------------------------------------------
        // Location
        // --------------------------------------------------------

        if (countryId.HasValue)
        {
            query = query.Where(x =>
                x.CountryID == countryId.Value);
        }

        if (stateId.HasValue)
        {
            query = query.Where(x =>
                x.StateID == stateId.Value);
        }

        if (cityId.HasValue)
        {
            query = query.Where(x =>
                x.CityID == cityId.Value);
        }

        if (areaId.HasValue)
        {
            query = query.Where(x =>
                x.AreaID == areaId.Value);
        }

        // --------------------------------------------------------
        // Price
        // --------------------------------------------------------

        if (minPrice.HasValue)
        {
            query = query.Where(x =>
                x.Price.HasValue &&
                x.Price.Value >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(x =>
                x.Price.HasValue &&
                x.Price.Value <= maxPrice.Value);
        }

        // --------------------------------------------------------
        // Count
        // --------------------------------------------------------

        var totalRecords =
            await query.CountAsync();

        // --------------------------------------------------------
        // Execute Query
        // --------------------------------------------------------

        var items =
            await query
                .OrderByDescending(x => x.IsFeatured)
                .ThenByDescending(x => x.PublishedDate)
                .ThenByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

        return new AdvertisementListResponse
        {
            Items =
                items
                    .Select(x =>
                        MapToResponse(
                            x,
                            includePrivateContact: false))
                    .ToList(),

            PageNumber =
                pageNumber,

            PageSize =
                pageSize,

            TotalRecords =
                totalRecords,

            TotalPages =
                (int)Math.Ceiling(
                    totalRecords /
                    (double)pageSize)
        };
    }

    // ============================================================
    // UPDATE
    // ============================================================

    public async Task<AdvertisementResponse?>
        UpdateAsync(
            long userId,
            long advertisementId,
            UpdateAdvertisementRequest request)
    {
        var advertisement =
            await _context.Advertisements
                .FirstOrDefaultAsync(x =>
                    x.AdvertisementID == advertisementId &&
                    x.UserID == userId);

        if (advertisement == null)
        {
            return null;
        }

        // --------------------------------------------------------
        // Get Current Status
        // --------------------------------------------------------

        var status =
            await _context.AdvertisementStatuses
                .FirstOrDefaultAsync(x =>
                    x.StatusID ==
                    advertisement.StatusID &&
                    x.IsActive);

        if (status == null)
        {
            return null;
        }

        // --------------------------------------------------------
        // Only Draft / Rejected advertisements can be edited.
        // --------------------------------------------------------

        if (status.StatusCode != "DRAFT" &&
            status.StatusCode != "REJECTED")
        {
            throw new InvalidOperationException(
                "Only Draft or Rejected advertisements can be edited.");
        }

        // --------------------------------------------------------
        // Validate Category
        // --------------------------------------------------------

        var categoryExists =
            await _context.AdvertisementCategories
                .AnyAsync(x =>
                    x.CategoryID ==
                    request.CategoryID &&
                    x.IsActive);

        if (!categoryExists)
        {
            throw new InvalidOperationException(
                "Advertisement category was not found or is inactive.");
        }

        // --------------------------------------------------------
        // Validate Type
        // --------------------------------------------------------

        var typeExists =
            await _context.AdvertisementTypes
                .AnyAsync(x =>
                    x.AdvertisementTypeID ==
                    request.AdvertisementTypeID &&
                    x.IsActive);

        if (!typeExists)
        {
            throw new InvalidOperationException(
                "Advertisement type was not found or is inactive.");
        }

        // --------------------------------------------------------
        // Normalize Contact Information
        // --------------------------------------------------------

        var whatsappNumber =
            NormalizeOptionalValue(request.WhatsAppNumber);

        var contactEmail =
            NormalizeOptionalValue(request.ContactEmail);

        // --------------------------------------------------------
        // Validate Public Contact Flags
        //
        // It is invalid to mark a method as public when the
        // corresponding contact value has not been supplied.
        // --------------------------------------------------------

        ValidatePublicContactFlags(
            whatsappNumber,
            contactEmail,
            request.ShowWhatsAppToPublic,
            request.ShowEmailToPublic);

        // --------------------------------------------------------
        // Update Fields
        // --------------------------------------------------------

        advertisement.CategoryID =
            request.CategoryID;

        advertisement.AdvertisementTypeID =
            request.AdvertisementTypeID;

        advertisement.Title =
            request.Title.Trim();

        advertisement.TitleAr =
            string.IsNullOrWhiteSpace(request.TitleAr)
                ? null
                : request.TitleAr.Trim();

        advertisement.Description =
            request.Description.Trim();

        advertisement.DescriptionAr =
            string.IsNullOrWhiteSpace(request.DescriptionAr)
                ? null
                : request.DescriptionAr.Trim();

        advertisement.Price =
            request.Price;

        advertisement.CurrencyCode =
            string.IsNullOrWhiteSpace(request.CurrencyCode)
                ? "BHD"
                : request.CurrencyCode
                    .Trim()
                    .ToUpperInvariant();

        advertisement.IsNegotiable =
            request.IsNegotiable;

        advertisement.CountryID =
            request.CountryID;

        advertisement.StateID =
            request.StateID;

        advertisement.CityID =
            request.CityID;

        advertisement.AreaID =
            request.AreaID;

        advertisement.AddressLine =
            string.IsNullOrWhiteSpace(request.AddressLine)
                ? null
                : request.AddressLine.Trim();

        advertisement.Latitude =
            request.Latitude;

        advertisement.Longitude =
            request.Longitude;

        advertisement.PlotNumber =
            string.IsNullOrWhiteSpace(request.PlotNumber)
                ? null
                : request.PlotNumber.Trim();

        advertisement.LandArea =
            request.LandArea;

        advertisement.BuiltUpArea =
            request.BuiltUpArea;

        advertisement.Bedrooms =
            request.Bedrooms;

        advertisement.Bathrooms =
            request.Bathrooms;

        advertisement.PropertyAge =
            request.PropertyAge;

        advertisement.ExpiryDate =
            request.ExpiryDate;

        // --------------------------------------------------------
        // Contact Information
        // --------------------------------------------------------

        advertisement.WhatsAppNumber =
            whatsappNumber;

        advertisement.ContactEmail =
            contactEmail;

        advertisement.ShowWhatsAppToPublic =
            request.ShowWhatsAppToPublic;

        advertisement.ShowEmailToPublic =
            request.ShowEmailToPublic;

        advertisement.ModifiedDate =
            DateTime.UtcNow;

        advertisement.ModifiedBy =
            userId;

        // --------------------------------------------------------
        // Rejected → Draft after editing
        // --------------------------------------------------------

        if (status.StatusCode == "REJECTED")
        {
            var draftStatus =
                await _context.AdvertisementStatuses
                    .FirstOrDefaultAsync(x =>
                        x.StatusCode == "DRAFT" &&
                        x.IsActive);

            if (draftStatus == null)
            {
                throw new InvalidOperationException(
                    "Advertisement DRAFT status was not found or is inactive.");
            }

            advertisement.StatusID =
                draftStatus.StatusID;
        }

        await _context.SaveChangesAsync();

        return await GetByIdAsync(
            advertisement.AdvertisementID,
            userId);
    }

    // ============================================================
    // DELETE
    // ============================================================

    public async Task<bool> DeleteAsync(
        long userId,
        long advertisementId)
    {
        var advertisement =
            await _context.Advertisements
                .Include(x => x.Status)
                .FirstOrDefaultAsync(x =>
                    x.AdvertisementID == advertisementId &&
                    x.UserID == userId);

        if (advertisement == null)
        {
            return false;
        }

        // --------------------------------------------------------
        // Only Draft / Rejected advertisements can be deleted.
        // --------------------------------------------------------

        if (advertisement.Status == null ||
            (advertisement.Status.StatusCode != "DRAFT" &&
             advertisement.Status.StatusCode != "REJECTED"))
        {
            return false;
        }

        _context.Advertisements.Remove(
            advertisement);

        await _context.SaveChangesAsync();

        return true;
    }

    // ============================================================
    // SUBMIT FOR APPROVAL
    // ============================================================

    public async Task<bool> SubmitAsync(
        long userId,
        long advertisementId)
    {
        var advertisement =
            await _context.Advertisements
                .Include(x => x.Status)
                .FirstOrDefaultAsync(x =>
                    x.AdvertisementID == advertisementId &&
                    x.UserID == userId);

        if (advertisement == null)
        {
            return false;
        }

        // --------------------------------------------------------
        // Only Draft advertisements can be submitted.
        // --------------------------------------------------------

        if (advertisement.Status == null ||
            advertisement.Status.StatusCode != "DRAFT")
        {
            return false;
        }

        // --------------------------------------------------------
        // Validate mandatory information
        // --------------------------------------------------------

        if (string.IsNullOrWhiteSpace(advertisement.Title) ||
            string.IsNullOrWhiteSpace(advertisement.Description))
        {
            return false;
        }

        // --------------------------------------------------------
        // Validate Contact Information
        //
        // At least one contact method must:
        //   1. contain a value
        //   2. be allowed to be shown publicly
        //
        // This prevents an advertisement from being submitted
        // without a usable public contact method.
        // --------------------------------------------------------

        var hasWhatsApp =
            !string.IsNullOrWhiteSpace(
                advertisement.WhatsAppNumber);

        var hasEmail =
            !string.IsNullOrWhiteSpace(
                advertisement.ContactEmail);

        var hasPublicWhatsApp =
            hasWhatsApp &&
            advertisement.ShowWhatsAppToPublic;

        var hasPublicEmail =
            hasEmail &&
            advertisement.ShowEmailToPublic;

        if (!hasWhatsApp && !hasEmail)
        {
            throw new InvalidOperationException(
                "At least one contact method is required before submitting the advertisement.");
        }

        if (!hasPublicWhatsApp && !hasPublicEmail)
        {
            throw new InvalidOperationException(
                "At least one contact method must be enabled for public visibility before submitting the advertisement.");
        }

        // --------------------------------------------------------
        // Get Pending Review Status
        // --------------------------------------------------------

        var pendingReviewStatus =
            await _context.AdvertisementStatuses
                .FirstOrDefaultAsync(x =>
                    x.StatusCode == "PENDING_REVIEW" &&
                    x.IsActive);

        if (pendingReviewStatus == null)
        {
            throw new InvalidOperationException(
                "Advertisement PENDING_REVIEW status was not found or is inactive.");
        }

        // --------------------------------------------------------
        // Change Status
        //
        // DRAFT → PENDING_REVIEW
        // --------------------------------------------------------

        advertisement.StatusID =
            pendingReviewStatus.StatusID;

        advertisement.ModifiedDate =
            DateTime.UtcNow;

        advertisement.ModifiedBy =
            userId;

        await _context.SaveChangesAsync();

        return true;
    }

    // ============================================================
    // GENERATE ADVERTISEMENT NUMBER
    // ============================================================

    private async Task<string>
        GenerateAdvertisementNumberAsync()
    {
        var year =
            DateTime.UtcNow.Year;

        var prefix =
            $"ADV-{year}-";

        var lastNumber =
            await _context.Advertisements
                .Where(x =>
                    x.AdvertisementNumber.StartsWith(
                        prefix))
                .OrderByDescending(x =>
                    x.AdvertisementID)
                .Select(x =>
                    x.AdvertisementNumber)
                .FirstOrDefaultAsync();

        var nextNumber = 1;

        if (!string.IsNullOrWhiteSpace(lastNumber))
        {
            var numberPart =
                lastNumber.Replace(
                    prefix,
                    "");

            if (int.TryParse(
                    numberPart,
                    out var parsedNumber))
            {
                nextNumber =
                    parsedNumber + 1;
            }
        }

        return
            $"{prefix}{nextNumber:D6}";
    }

    // ============================================================
    // MAP ENTITY → DTO
    // ============================================================

    private static AdvertisementResponse
        MapToResponse(
            Advertisement advertisement,
            bool includePrivateContact = false)
    {
        // --------------------------------------------------------
        // Contact visibility
        //
        // Owner:
        //     sees actual WhatsApp/email values.
        //
        // Public visitor:
        //     only sees a value when its public flag is enabled.
        // --------------------------------------------------------

        string? whatsappNumber = null;
        string? contactEmail = null;

        if (includePrivateContact)
        {
            whatsappNumber =
                advertisement.WhatsAppNumber;

            contactEmail =
                advertisement.ContactEmail;
        }
        else
        {
            if (advertisement.ShowWhatsAppToPublic)
            {
                whatsappNumber =
                    advertisement.WhatsAppNumber;
            }

            if (advertisement.ShowEmailToPublic)
            {
                contactEmail =
                    advertisement.ContactEmail;
            }
        }

        return new AdvertisementResponse
        {
            AdvertisementID =
                advertisement.AdvertisementID,

            UserID =
                advertisement.UserID,

            CategoryID =
                advertisement.CategoryID,

            CategoryName =
                advertisement.Category?.CategoryName
                ?? string.Empty,

            AdvertisementTypeID =
                advertisement.AdvertisementTypeID,

            AdvertisementTypeName =
                advertisement.AdvertisementType?.TypeName
                ?? string.Empty,

            StatusID =
                advertisement.StatusID,

            StatusCode =
                advertisement.Status?.StatusCode
                ?? string.Empty,

            StatusName =
                advertisement.Status?.StatusName
                ?? string.Empty,

            AdvertisementNumber =
                advertisement.AdvertisementNumber,

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

            CountryID =
    advertisement.CountryID,

CountryName =
    advertisement.Country?.CountryName,

StateID =
    advertisement.StateID,

StateName =
    advertisement.State?.StateName,

CityID =
    advertisement.CityID,

CityName =
    advertisement.City?.CityName,

AreaID =
    advertisement.AreaID,

AreaName =
    advertisement.Area?.AreaName,

            AddressLine =
                advertisement.AddressLine,

            Latitude =
                advertisement.Latitude,

            Longitude =
                advertisement.Longitude,

            PlotNumber =
                advertisement.PlotNumber,

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

            // ----------------------------------------------------
            // Contact Information
            // ----------------------------------------------------

            WhatsAppNumber =
                whatsappNumber,

            ContactEmail =
                contactEmail,

            ShowWhatsAppToPublic =
                advertisement.ShowWhatsAppToPublic,

            ShowEmailToPublic =
                advertisement.ShowEmailToPublic,

            // ----------------------------------------------------
            // Advertisement Images
            // ----------------------------------------------------

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
                            x.FileURL ?? string.Empty,

                        ContentType =
                            null,

                        FileSize =
                            x.FileSize,

                        IsPrimary =
                            x.IsPrimary,

                        DisplayOrder =
                            x.DisplayOrder,

                        CreatedDate =
                            x.CreatedDate
                    })
                    .ToList()
        };
    }

    // ============================================================
    // NORMALIZE OPTIONAL VALUE
    // ============================================================

    private static string? NormalizeOptionalValue(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    // ============================================================
    // VALIDATE PUBLIC CONTACT FLAGS
    // ============================================================

    private static void ValidatePublicContactFlags(
        string? whatsappNumber,
        string? contactEmail,
        bool showWhatsAppToPublic,
        bool showEmailToPublic)
    {
        if (showWhatsAppToPublic &&
            string.IsNullOrWhiteSpace(whatsappNumber))
        {
            throw new InvalidOperationException(
                "A WhatsApp number is required when WhatsApp visibility is enabled.");
        }

        if (showEmailToPublic &&
            string.IsNullOrWhiteSpace(contactEmail))
        {
            throw new InvalidOperationException(
                "An email address is required when email visibility is enabled.");
        }
    }
}