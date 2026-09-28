using HRInfoAdvertisements.Application.DTOs.CategoryManagement;
using HRInfoAdvertisements.Application.Interfaces;
using HRInfoAdvertisements.Domain.Entities;
using HRInfoAdvertisements.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRInfoAdvertisements.Infrastructure.Services;

public class CategoryManagementService : ICategoryManagementService
{
    private readonly ApplicationDbContext _context;

    public CategoryManagementService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CategoryListResponse> GetCategoriesAsync(
        CategoryListRequest request)
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

        var query = _context.AdvertisementCategories
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();

            query = query.Where(x =>
                x.CategoryName.Contains(search) ||
                (x.CategoryNameAr != null &&
                 x.CategoryNameAr.Contains(search)) ||
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
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.CategoryName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new CategoryListItemResponse
            {
                CategoryID = x.CategoryID,
                CategoryName = x.CategoryName,
                CategoryNameAr = x.CategoryNameAr,
                Description = x.Description,
                IsActive = x.IsActive,
                DisplayOrder = x.DisplayOrder,
                AdvertisementCount = x.Advertisements.Count()
            })
            .ToListAsync();

        return new CategoryListResponse
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages
        };
    }

    public async Task<CategoryDetailResponse?> GetCategoryByIdAsync(
        int categoryId)
    {
        return await _context.AdvertisementCategories
            .AsNoTracking()
            .Where(x => x.CategoryID == categoryId)
            .Select(x => new CategoryDetailResponse
            {
                CategoryID = x.CategoryID,
                CategoryName = x.CategoryName,
                CategoryNameAr = x.CategoryNameAr,
                Description = x.Description,
                IsActive = x.IsActive,
                DisplayOrder = x.DisplayOrder,
                AdvertisementCount = x.Advertisements.Count()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<int?> CreateCategoryAsync(
        CreateCategoryRequest request,
        long createdBy)
    {
        var categoryName = request.CategoryName.Trim();

        if (string.IsNullOrWhiteSpace(categoryName))
            return null;

        var exists = await _context.AdvertisementCategories
            .AnyAsync(x =>
                x.CategoryName.ToLower() ==
                categoryName.ToLower());

        if (exists)
            return null;

        var category = new AdvertisementCategory
        {
            CategoryName = categoryName,

            CategoryNameAr =
                string.IsNullOrWhiteSpace(request.CategoryNameAr)
                    ? null
                    : request.CategoryNameAr.Trim(),

            Description =
                string.IsNullOrWhiteSpace(request.Description)
                    ? null
                    : request.Description.Trim(),

            IsActive = request.IsActive,

            DisplayOrder = request.DisplayOrder
        };

        _context.AdvertisementCategories.Add(category);

        await _context.SaveChangesAsync();

        return category.CategoryID;
    }

    public async Task<bool> UpdateCategoryAsync(
        int categoryId,
        UpdateCategoryRequest request,
        long modifiedBy)
    {
        var category = await _context.AdvertisementCategories
            .FirstOrDefaultAsync(x =>
                x.CategoryID == categoryId);

        if (category == null)
            return false;

        var categoryName = request.CategoryName.Trim();

        if (string.IsNullOrWhiteSpace(categoryName))
            return false;

        var duplicateName =
            await _context.AdvertisementCategories
                .AnyAsync(x =>
                    x.CategoryID != categoryId &&
                    x.CategoryName.ToLower() ==
                    categoryName.ToLower());

        if (duplicateName)
            return false;

        category.CategoryName = categoryName;

        category.CategoryNameAr =
            string.IsNullOrWhiteSpace(request.CategoryNameAr)
                ? null
                : request.CategoryNameAr.Trim();

        category.Description =
            string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim();

        category.IsActive = request.IsActive;

        category.DisplayOrder = request.DisplayOrder;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdateCategoryStatusAsync(
        int categoryId,
        bool isActive,
        long modifiedBy)
    {
        var category =
            await _context.AdvertisementCategories
                .FirstOrDefaultAsync(x =>
                    x.CategoryID == categoryId);

        if (category == null)
            return false;

        category.IsActive = isActive;

        await _context.SaveChangesAsync();

        return true;
    }
}