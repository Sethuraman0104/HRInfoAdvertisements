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
        // When userId is supplied, allow owner to see own ad.
        // Otherwise only published ads are returned.
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

        return MapToResponse(advertisement);
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
                    .Select(MapToResponse)
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
                .Where(x =>
                    x.Status.StatusCode == "PUBLISHED" &&
                    x.Status.IsActive);

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
                    .Select(MapToResponse)
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
            Advertisement advertisement)
    {
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

            StateID =
                advertisement.StateID,

            CityID =
                advertisement.CityID,

            AreaID =
                advertisement.AreaID,

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
            // Advertisement Images
            // ----------------------------------------------------
            // ContentType is intentionally not mapped here because
            // AdvertisementImage does not contain a ContentType
            // property and the database table does not have a
            // ContentType column.
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
}