using HRInfoAdvertisements.Application.DTOs.Favorite;
using HRInfoAdvertisements.Application.Interfaces;
using HRInfoAdvertisements.Domain.Entities;
using HRInfoAdvertisements.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;

namespace HRInfoAdvertisements.Infrastructure.Services;

public class AdvertisementFavoriteService
    : IAdvertisementFavoriteService
{
    private readonly ApplicationDbContext _context;

    public AdvertisementFavoriteService(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> AddFavoriteAsync(
        long userId,
        long advertisementId)
    {
        // --------------------------------------------------
        // Check advertisement
        // --------------------------------------------------

        var advertisement =
            await _context.Advertisements
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.AdvertisementID ==
                    advertisementId);

        if (advertisement == null)
        {
            return false;
        }

        // --------------------------------------------------
        // Only published advertisements can be favorited.
        // --------------------------------------------------

        var publishedStatus =
            await _context.AdvertisementStatuses
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.StatusCode == "PUBLISHED" &&
                    x.IsActive);

        if (publishedStatus == null)
        {
            throw new InvalidOperationException(
                "Published advertisement status was not found.");
        }

        if (advertisement.StatusID !=
            publishedStatus.StatusID)
        {
            throw new InvalidOperationException(
                "Only published advertisements can be added to favorites.");
        }

        // --------------------------------------------------
        // Check duplicate
        // --------------------------------------------------

        var existingFavorite =
            await _context.AdvertisementFavorites
                .FirstOrDefaultAsync(x =>
                    x.UserID == userId &&
                    x.AdvertisementID ==
                    advertisementId);

        if (existingFavorite != null)
        {
            return true;
        }

        // --------------------------------------------------
        // Add favorite
        // --------------------------------------------------

        var favorite =
            new AdvertisementFavorite
            {
                UserID = userId,

                AdvertisementID =
                    advertisementId,

                CreatedDate =
                    DateTime.UtcNow
            };

        _context.AdvertisementFavorites.Add(
            favorite);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RemoveFavoriteAsync(
        long userId,
        long advertisementId)
    {
        var favorite =
            await _context.AdvertisementFavorites
                .FirstOrDefaultAsync(x =>
                    x.UserID == userId &&
                    x.AdvertisementID ==
                    advertisementId);

        if (favorite == null)
        {
            return false;
        }

        _context.AdvertisementFavorites.Remove(
            favorite);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<FavoriteStatusResponse>
        GetFavoriteStatusAsync(
            long userId,
            long advertisementId)
    {
        var favorite =
            await _context.AdvertisementFavorites
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.UserID == userId &&
                    x.AdvertisementID ==
                    advertisementId);

        return new FavoriteStatusResponse
        {
            AdvertisementID =
                advertisementId,

            IsFavorite =
                favorite != null,

            FavoritedDate =
                favorite?.CreatedDate
        };
    }

    public async Task<List<FavoriteResponse>>
        GetMyFavoritesAsync(
            long userId)
    {
        var favorites =
            await _context.AdvertisementFavorites
                .AsNoTracking()
                .Where(x =>
                    x.UserID == userId)
                .Include(x =>
                    x.Advertisement)
                    .ThenInclude(x =>
                        x.Category)
                .Include(x =>
                    x.Advertisement)
                    .ThenInclude(x =>
                        x.AdvertisementType)
                .Include(x =>
                    x.Advertisement)
                    .ThenInclude(x =>
                        x.Status)
                .OrderByDescending(x =>
                    x.CreatedDate)
                .ToListAsync();

        return favorites
            .Select(x =>
                new FavoriteResponse
                {
                    AdvertisementFavoriteID =
                        x.AdvertisementFavoriteID,

                    AdvertisementID =
                        x.Advertisement.AdvertisementID,

                    AdvertisementNumber =
                        x.Advertisement.AdvertisementNumber,

                    Title =
                        x.Advertisement.Title,

                    TitleAr =
                        x.Advertisement.TitleAr,

                    Price =
                        x.Advertisement.Price,

                    CurrencyCode =
                        x.Advertisement.CurrencyCode,

                    CategoryName =
                        x.Advertisement
                            .Category
                            .CategoryName,

                    AdvertisementTypeName =
                        x.Advertisement
                            .AdvertisementType
                            .TypeName,

                    StatusCode =
                        x.Advertisement
                            .Status
                            .StatusCode,

                    StatusName =
                        x.Advertisement
                            .Status
                            .StatusName,

                    AddressLine =
                        x.Advertisement.AddressLine,

                    IsFeatured =
                        x.Advertisement.IsFeatured,

                    CreatedDate =
                        x.Advertisement.CreatedDate,

                    PublishedDate =
                        x.Advertisement.PublishedDate,

                    ExpiryDate =
                        x.Advertisement.ExpiryDate,

                    FavoritedDate =
                        x.CreatedDate
                })
            .ToList();
    }

        // ============================================================
    // ADMIN - FAVORITES LIST
    // ============================================================

    public async Task<FavoriteAdminListResponse>
        GetAdminFavoritesAsync(
            FavoriteAdminListRequest request)
    {
        // --------------------------------------------------------
        // Normalize pagination
        // --------------------------------------------------------

        var pageNumber =
            request.PageNumber < 1
                ? 1
                : request.PageNumber;

        var pageSize =
            request.PageSize switch
            {
                < 10 => 10,
                > 100 => 100,
                _ => request.PageSize
            };

        // --------------------------------------------------------
        // Base query
        // --------------------------------------------------------

        var query =
            _context.AdvertisementFavorites
                .AsNoTracking()
                .Include(x => x.User)
                .Include(x => x.Advertisement)
                    .ThenInclude(x => x.Category)
                .Include(x => x.Advertisement)
                    .ThenInclude(x => x.AdvertisementType)
                .Include(x => x.Advertisement)
                    .ThenInclude(x => x.Status)
                .AsQueryable();

        // --------------------------------------------------------
        // Search
        // --------------------------------------------------------

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search =
                request.Search.Trim();

            query =
                query.Where(x =>
                    x.User.UserName.Contains(search) ||
                    x.User.Email.Contains(search) ||
                    x.Advertisement.AdvertisementNumber
                        .Contains(search) ||
                    x.Advertisement.Title
                        .Contains(search));
        }

        // --------------------------------------------------------
        // User filter
        // --------------------------------------------------------

        if (request.UserID.HasValue)
        {
            query =
                query.Where(x =>
                    x.UserID ==
                    request.UserID.Value);
        }

        // --------------------------------------------------------
        // Advertisement filter
        // --------------------------------------------------------

        if (request.AdvertisementID.HasValue)
        {
            query =
                query.Where(x =>
                    x.AdvertisementID ==
                    request.AdvertisementID.Value);
        }

        // --------------------------------------------------------
        // Summary
        // --------------------------------------------------------

        var totalRecords =
            await query.CountAsync();

        var uniqueUsers =
            await query
                .Select(x => x.UserID)
                .Distinct()
                .CountAsync();

        var uniqueAdvertisements =
            await query
                .Select(x => x.AdvertisementID)
                .Distinct()
                .CountAsync();

        // --------------------------------------------------------
        // Pagination
        // --------------------------------------------------------

        var totalPages =
            totalRecords == 0
                ? 0
                : (int)Math.Ceiling(
                    totalRecords /
                    (double)pageSize);

        if (totalPages > 0 &&
            pageNumber > totalPages)
        {
            pageNumber = totalPages;
        }

        var favorites =
            await query
                .OrderByDescending(x =>
                    x.CreatedDate)
                .Skip(
                    (pageNumber - 1) *
                    pageSize)
                .Take(pageSize)
                .ToListAsync();

        // --------------------------------------------------------
        // Map response
        // --------------------------------------------------------

        var items =
            favorites
                .Select(x =>
                    new FavoriteAdminListItemResponse
                    {
                        AdvertisementFavoriteID =
                            x.AdvertisementFavoriteID,

                        FavoritedDate =
                            x.CreatedDate,

                        // User
                        UserID =
                            x.UserID,

                        UserName =
                            x.User.UserName,

                        Email =
                            x.User.Email,

                        MobileNo =
                            x.User.MobileNo,

                        // Advertisement
                        AdvertisementID =
                            x.AdvertisementID,

                        AdvertisementNumber =
                            x.Advertisement
                                .AdvertisementNumber,

                        Title =
                            x.Advertisement.Title,

                        TitleAr =
                            x.Advertisement.TitleAr,

                        Price =
                            x.Advertisement.Price,

                        CurrencyCode =
                            x.Advertisement.CurrencyCode,

                        // Classification
                        CategoryName =
                            x.Advertisement
                                .Category
                                .CategoryName,

                        AdvertisementTypeName =
                            x.Advertisement
                                .AdvertisementType
                                .TypeName,

                        // Status
                        StatusCode =
                            x.Advertisement
                                .Status
                                .StatusCode,

                        StatusName =
                            x.Advertisement
                                .Status
                                .StatusName,

                        IsFeatured =
                            x.Advertisement
                                .IsFeatured,

                        CreatedDate =
                            x.Advertisement
                                .CreatedDate,

                        PublishedDate =
                            x.Advertisement
                                .PublishedDate,

                        ExpiryDate =
                            x.Advertisement
                                .ExpiryDate
                    })
                .ToList();

        // --------------------------------------------------------
        // Return
        // --------------------------------------------------------

        return new FavoriteAdminListResponse
        {
            TotalFavorites =
                totalRecords,

            UniqueUsers =
                uniqueUsers,

            UniqueAdvertisements =
                uniqueAdvertisements,

            PageNumber =
                pageNumber,

            PageSize =
                pageSize,

            TotalPages =
                totalPages,

            TotalRecords =
                totalRecords,

            Items =
                items
        };
    }
}