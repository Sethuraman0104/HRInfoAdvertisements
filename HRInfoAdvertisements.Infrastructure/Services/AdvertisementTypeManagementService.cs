using HRInfoAdvertisements.Application.DTOs.AdvertisementTypeManagement;
using HRInfoAdvertisements.Application.Interfaces;
using HRInfoAdvertisements.Domain.Entities;
using HRInfoAdvertisements.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRInfoAdvertisements.Infrastructure.Services;

public class AdvertisementTypeManagementService
    : IAdvertisementTypeManagementService
{
    private readonly ApplicationDbContext _context;

    public AdvertisementTypeManagementService(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdvertisementTypeListResponse>
        GetAdvertisementTypesAsync(
            AdvertisementTypeListRequest request)
    {
        var pageNumber = request.PageNumber < 1
            ? 1
            : request.PageNumber;

        var pageSize = request.PageSize switch
        {
            <= 0 => 20,
            > 100 => 100,
            _ => request.PageSize
        };

        var query = _context.AdvertisementTypes
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();

            query = query.Where(x =>
                x.TypeName.Contains(search) ||
                (x.TypeNameAr != null &&
                 x.TypeNameAr.Contains(search)) ||
                (x.Description != null &&
                 x.Description.Contains(search)));
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x =>
                x.IsActive == request.IsActive.Value);
        }

        var totalRecords = await query.CountAsync();

        var totalPages = totalRecords == 0
            ? 0
            : (int)Math.Ceiling(
                totalRecords / (double)pageSize);

        if (totalPages > 0 && pageNumber > totalPages)
        {
            pageNumber = totalPages;
        }

        var items = await query
    .OrderBy(x => x.TypeName)
    .Skip((pageNumber - 1) * pageSize)
    .Take(pageSize)
    .Select(x => new AdvertisementTypeListItemResponse
    {
        AdvertisementTypeID =
            x.AdvertisementTypeID,

        TypeName = x.TypeName,

        TypeNameAr = x.TypeNameAr,

        Description = x.Description,

        IsActive = x.IsActive,

        AdvertisementCount =
            x.Advertisements.Count()
    })
    .ToListAsync();

        // Apply pagination after ordering.
        items = items
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new AdvertisementTypeListResponse
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages
        };
    }

    public async Task<AdvertisementTypeDetailResponse?>
        GetAdvertisementTypeByIdAsync(
            int advertisementTypeId)
    {
        return await _context.AdvertisementTypes
            .AsNoTracking()
            .Where(x =>
                x.AdvertisementTypeID ==
                advertisementTypeId)
            .Select(x => new AdvertisementTypeDetailResponse
            {
                AdvertisementTypeID =
                    x.AdvertisementTypeID,

                TypeName = x.TypeName,

                TypeNameAr = x.TypeNameAr,

                Description = x.Description,

                IsActive = x.IsActive,

                AdvertisementCount =
                    x.Advertisements.Count()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<int?> CreateAdvertisementTypeAsync(
        CreateAdvertisementTypeRequest request,
        long createdBy)
    {
        var typeName = request.TypeName.Trim();

        if (string.IsNullOrWhiteSpace(typeName))
            return null;

        var exists =
            await _context.AdvertisementTypes
                .AnyAsync(x =>
                    x.TypeName.ToLower() ==
                    typeName.ToLower());

        if (exists)
            return null;

        var advertisementType = new AdvertisementType
        {
            TypeName = typeName,

            TypeNameAr =
                string.IsNullOrWhiteSpace(request.TypeNameAr)
                    ? null
                    : request.TypeNameAr.Trim(),

            Description =
                string.IsNullOrWhiteSpace(request.Description)
                    ? null
                    : request.Description.Trim(),

            IsActive = request.IsActive
        };

        _context.AdvertisementTypes.Add(
            advertisementType);

        await _context.SaveChangesAsync();

        return advertisementType.AdvertisementTypeID;
    }

    public async Task<bool> UpdateAdvertisementTypeAsync(
        int advertisementTypeId,
        UpdateAdvertisementTypeRequest request,
        long modifiedBy)
    {
        var advertisementType =
            await _context.AdvertisementTypes
                .FirstOrDefaultAsync(x =>
                    x.AdvertisementTypeID ==
                    advertisementTypeId);

        if (advertisementType == null)
            return false;

        var typeName = request.TypeName.Trim();

        if (string.IsNullOrWhiteSpace(typeName))
            return false;

        var duplicateName =
            await _context.AdvertisementTypes
                .AnyAsync(x =>
                    x.AdvertisementTypeID !=
                    advertisementTypeId &&
                    x.TypeName.ToLower() ==
                    typeName.ToLower());

        if (duplicateName)
            return false;

        advertisementType.TypeName =
            typeName;

        advertisementType.TypeNameAr =
            string.IsNullOrWhiteSpace(request.TypeNameAr)
                ? null
                : request.TypeNameAr.Trim();

        advertisementType.Description =
            string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim();

        advertisementType.IsActive =
            request.IsActive;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdateAdvertisementTypeStatusAsync(
        int advertisementTypeId,
        bool isActive,
        long modifiedBy)
    {
        var advertisementType =
            await _context.AdvertisementTypes
                .FirstOrDefaultAsync(x =>
                    x.AdvertisementTypeID ==
                    advertisementTypeId);

        if (advertisementType == null)
            return false;

        advertisementType.IsActive = isActive;

        await _context.SaveChangesAsync();

        return true;
    }
}