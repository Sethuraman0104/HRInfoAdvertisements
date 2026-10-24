using HRInfoAdvertisements.Application.DTOs.Advertisement;
using HRInfoAdvertisements.Application.DTOs.Common;
using HRInfoAdvertisements.Application.Interfaces;
using HRInfoAdvertisements.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;

namespace HRInfoAdvertisements.Infrastructure.Services;

public class AdvertisementSearchService
    : IAdvertisementSearchService
{
    private readonly ApplicationDbContext _context;

    public AdvertisementSearchService(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResponse<AdvertisementSearchResponse>>
        SearchAsync(
            AdvertisementSearchRequest request)
    {
        request.PageNumber =
            request.PageNumber < 1
                ? 1
                : request.PageNumber;

        request.PageSize =
            request.PageSize < 1
                ? 20
                : Math.Min(request.PageSize, 100);

        var query = _context.Advertisements
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.AdvertisementType)
            .Include(x => x.Status)
            .Include(x => x.User)
            .AsQueryable();

        // --------------------------------------------------
        // Keyword
        // --------------------------------------------------

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim();

            query = query.Where(x =>
                x.AdvertisementNumber.Contains(keyword) ||
                x.Title.Contains(keyword) ||
                (x.TitleAr != null &&
                 x.TitleAr.Contains(keyword)) ||
                (x.Description != null &&
                 x.Description.Contains(keyword)) ||
                (x.DescriptionAr != null &&
                 x.DescriptionAr.Contains(keyword)) ||
                (x.AddressLine != null &&
                 x.AddressLine.Contains(keyword)));
        }

        // --------------------------------------------------
        // Category
        // --------------------------------------------------

        if (request.CategoryID.HasValue)
        {
            query = query.Where(x =>
                x.CategoryID ==
                request.CategoryID.Value);
        }

        // --------------------------------------------------
        // Advertisement Type
        // --------------------------------------------------

        if (request.AdvertisementTypeID.HasValue)
        {
            query = query.Where(x =>
                x.AdvertisementTypeID ==
                request.AdvertisementTypeID.Value);
        }

        // --------------------------------------------------
        // Status ID
        // --------------------------------------------------

        if (request.StatusID.HasValue)
        {
            query = query.Where(x =>
                x.StatusID ==
                request.StatusID.Value);
        }

        // --------------------------------------------------
        // Status Code
        // --------------------------------------------------

        if (!string.IsNullOrWhiteSpace(request.StatusCode))
        {
            var statusCode =
                request.StatusCode.Trim();

            query = query.Where(x =>
                x.Status.StatusCode ==
                statusCode);
        }

        // --------------------------------------------------
        // Country
        // --------------------------------------------------

        if (request.CountryID.HasValue)
        {
            query = query.Where(x =>
                x.CountryID ==
                request.CountryID.Value);
        }

        // --------------------------------------------------
        // State
        // --------------------------------------------------

        if (request.StateID.HasValue)
        {
            query = query.Where(x =>
                x.StateID ==
                request.StateID.Value);
        }

        // --------------------------------------------------
        // Price
        // --------------------------------------------------

        if (request.MinPrice.HasValue)
        {
            query = query.Where(x =>
                x.Price.HasValue &&
                x.Price.Value >=
                request.MinPrice.Value);
        }

        if (request.MaxPrice.HasValue)
        {
            query = query.Where(x =>
                x.Price.HasValue &&
                x.Price.Value <=
                request.MaxPrice.Value);
        }

        // --------------------------------------------------
        // Bedrooms
        // --------------------------------------------------

        if (request.MinBedrooms.HasValue)
        {
            query = query.Where(x =>
                x.Bedrooms.HasValue &&
                x.Bedrooms.Value >=
                request.MinBedrooms.Value);
        }

        if (request.MaxBedrooms.HasValue)
        {
            query = query.Where(x =>
                x.Bedrooms.HasValue &&
                x.Bedrooms.Value <=
                request.MaxBedrooms.Value);
        }

        // --------------------------------------------------
        // Bathrooms
        // --------------------------------------------------

        if (request.MinBathrooms.HasValue)
        {
            query = query.Where(x =>
                x.Bathrooms.HasValue &&
                x.Bathrooms.Value >=
                request.MinBathrooms.Value);
        }

        if (request.MaxBathrooms.HasValue)
        {
            query = query.Where(x =>
                x.Bathrooms.HasValue &&
                x.Bathrooms.Value <=
                request.MaxBathrooms.Value);
        }

        // --------------------------------------------------
        // Land Area
        // --------------------------------------------------

        if (request.MinLandArea.HasValue)
        {
            query = query.Where(x =>
                x.LandArea.HasValue &&
                x.LandArea.Value >=
                request.MinLandArea.Value);
        }

        if (request.MaxLandArea.HasValue)
        {
            query = query.Where(x =>
                x.LandArea.HasValue &&
                x.LandArea.Value <=
                request.MaxLandArea.Value);
        }

        // --------------------------------------------------
        // Built-up Area
        // --------------------------------------------------

        if (request.MinBuiltUpArea.HasValue)
        {
            query = query.Where(x =>
                x.BuiltUpArea.HasValue &&
                x.BuiltUpArea.Value >=
                request.MinBuiltUpArea.Value);
        }

        if (request.MaxBuiltUpArea.HasValue)
        {
            query = query.Where(x =>
                x.BuiltUpArea.HasValue &&
                x.BuiltUpArea.Value <=
                request.MaxBuiltUpArea.Value);
        }

        // --------------------------------------------------
        // Negotiable
        // --------------------------------------------------

        if (request.IsNegotiable.HasValue)
        {
            query = query.Where(x =>
                x.IsNegotiable ==
                request.IsNegotiable.Value);
        }

        // --------------------------------------------------
        // Featured
        // --------------------------------------------------

        if (request.IsFeatured.HasValue)
        {
            query = query.Where(x =>
                x.IsFeatured ==
                request.IsFeatured.Value);
        }

        // --------------------------------------------------
        // Created Date
        // --------------------------------------------------

        if (request.CreatedFrom.HasValue)
        {
            var from =
                request.CreatedFrom.Value.Date;

            query = query.Where(x =>
                x.CreatedDate >= from);
        }

        if (request.CreatedTo.HasValue)
        {
            var to =
                request.CreatedTo.Value.Date.AddDays(1);

            query = query.Where(x =>
                x.CreatedDate < to);
        }

        // --------------------------------------------------
        // Published Date
        // --------------------------------------------------

        if (request.PublishedFrom.HasValue)
        {
            var from =
                request.PublishedFrom.Value.Date;

            query = query.Where(x =>
                x.PublishedDate.HasValue &&
                x.PublishedDate.Value >= from);
        }

        if (request.PublishedTo.HasValue)
        {
            var to =
                request.PublishedTo.Value.Date.AddDays(1);

            query = query.Where(x =>
                x.PublishedDate.HasValue &&
                x.PublishedDate.Value < to);
        }

        // --------------------------------------------------
        // Total Records
        // --------------------------------------------------

        var totalRecords =
            await query.CountAsync();

        // --------------------------------------------------
        // Sorting
        // --------------------------------------------------

        var sortBy =
            request.SortBy?
                .Trim()
                .ToLowerInvariant();

        var sortDirection =
            request.SortDirection?
                .Trim()
                .ToLowerInvariant();

        var descending =
            sortDirection == "desc" ||
            sortDirection == "descending";

        switch (sortBy)
        {
            case "price":

                query = descending
                    ? query.OrderByDescending(x => x.Price)
                    : query.OrderBy(x => x.Price);

                break;

            case "createddate":

                query = descending
                    ? query.OrderByDescending(x => x.CreatedDate)
                    : query.OrderBy(x => x.CreatedDate);

                break;

            case "publisheddate":

                query = descending
                    ? query.OrderByDescending(x => x.PublishedDate)
                    : query.OrderBy(x => x.PublishedDate);

                break;

            case "expirydate":

                query = descending
                    ? query.OrderByDescending(x => x.ExpiryDate)
                    : query.OrderBy(x => x.ExpiryDate);

                break;

            case "title":

                query = descending
                    ? query.OrderByDescending(x => x.Title)
                    : query.OrderBy(x => x.Title);

                break;

            case "bedrooms":

                query = descending
                    ? query.OrderByDescending(x => x.Bedrooms)
                    : query.OrderBy(x => x.Bedrooms);

                break;

            case "landarea":

                query = descending
                    ? query.OrderByDescending(x => x.LandArea)
                    : query.OrderBy(x => x.LandArea);

                break;

            case "builtuparea":

                query = descending
                    ? query.OrderByDescending(x => x.BuiltUpArea)
                    : query.OrderBy(x => x.BuiltUpArea);

                break;

            default:

                query = query
                    .OrderByDescending(x => x.CreatedDate);

                break;
        }

        // --------------------------------------------------
        // Pagination
        // --------------------------------------------------

        var advertisements =
            await query
                .Skip(
                    (request.PageNumber - 1) *
                    request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

        // --------------------------------------------------
        // Mapping
        // --------------------------------------------------

        var items =
            advertisements
                .Select(x => new AdvertisementSearchResponse
                {
                    AdvertisementID =
                        x.AdvertisementID,

                    AdvertisementNumber =
                        x.AdvertisementNumber,

                    UserID =
                        x.UserID,

                    UserName =
                        x.User.UserName,

                    Title =
                        x.Title,

                    TitleAr =
                        x.TitleAr,

                    Description =
                        x.Description,

                    DescriptionAr =
                        x.DescriptionAr,

                    Price =
                        x.Price,

                    CurrencyCode =
                        x.CurrencyCode,

                    IsNegotiable =
                        x.IsNegotiable,

                    CategoryID =
                        x.CategoryID,

                    CategoryName =
                        x.Category.CategoryName,

                    AdvertisementTypeID =
                        x.AdvertisementTypeID,

                    AdvertisementTypeName =
                        x.AdvertisementType.TypeName,

                    StatusID =
                        x.StatusID,

                    StatusCode =
                        x.Status.StatusCode,

                    StatusName =
                        x.Status.StatusName,

                    CountryID =
                        x.CountryID,

                    StateID =
                        x.StateID,

                    AddressLine =
                        x.AddressLine,

                    Latitude =
                        x.Latitude,

                    Longitude =
                        x.Longitude,

                    LandArea =
                        x.LandArea,

                    BuiltUpArea =
                        x.BuiltUpArea,

                    Bedrooms =
                        x.Bedrooms,

                    Bathrooms =
                        x.Bathrooms,

                    PropertyAge =
                        x.PropertyAge,

                    IsFeatured =
                        x.IsFeatured,

                    FeaturedUntil =
                        x.FeaturedUntil,

                    CreatedDate =
                        x.CreatedDate,

                    PublishedDate =
                        x.PublishedDate,

                    ExpiryDate =
                        x.ExpiryDate
                })
                .ToList();

        var totalPages =
            totalRecords == 0
                ? 0
                : (int)Math.Ceiling(
                    totalRecords /
                    (double)request.PageSize);

        return new PagedResponse<AdvertisementSearchResponse>
        {
            Items = items,

            PageNumber =
                request.PageNumber,

            PageSize =
                request.PageSize,

            TotalRecords =
                totalRecords,

            TotalPages =
                totalPages
        };
    }
}