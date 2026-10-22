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
    public async Task<List<CategoryFeatureResponse>> GetCategoryFeaturesAsync(int categoryId)
    {
        var categoryExists = await _context.AdvertisementCategories
            .AsNoTracking()
            .AnyAsync(x => x.CategoryID == categoryId && x.IsActive);

        if (!categoryExists)
            return new List<CategoryFeatureResponse>();

        return await _context.AdvertisementFeatures
            .AsNoTracking()
            .Where(x => x.CategoryID == categoryId && x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.FeatureName)
            .Select(x => new CategoryFeatureResponse
            {
                AdvertisementFeatureID = x.AdvertisementFeatureID,
                CategoryID = x.CategoryID,
                FeatureName = x.FeatureName,
                FeatureNameAr = x.FeatureNameAr,
                DataType = x.DataType,
                IsRequired = x.IsRequired,
                HelpText = x.HelpText,
                HelpTextAr = x.HelpTextAr,
                MinValue = x.MinValue,
                MaxValue = x.MaxValue,
                MinLength = x.MinLength,
                MaxLength = x.MaxLength,
                ValidationPattern = x.ValidationPattern,
                IsSearchable = x.IsSearchable,
                IsFilterable = x.IsFilterable,
                DisplayOrder = x.DisplayOrder,
                Options = x.Options
                    .Where(o => o.IsActive)
                    .OrderBy(o => o.DisplayOrder)
                    .ThenBy(o => o.OptionValue)
                    .Select(o => new CategoryFeatureOptionResponse
                    {
                        AdvertisementFeatureOptionID = o.AdvertisementFeatureOptionID,
                        OptionValue = o.OptionValue,
                        OptionValueAr = o.OptionValueAr,
                        DisplayOrder = o.DisplayOrder
                    })
                    .ToList()
            })
            .ToListAsync();
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

    private static readonly HashSet<string> AllowedFeatureDataTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "Text", "Number", "Decimal", "Numeric",
            "Integer", "Int", "WholeNumber",
            "Date", "DateTime", "Boolean", "Bool",
            "Select", "MultiSelect", "Radio"
        };

    private static void ValidateFeatureRequest(CreateCategoryFeatureRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FeatureName) ||
            request.FeatureName.Trim().Length > 150)
            throw new ArgumentException("Feature name is required and must not exceed 150 characters.");

        if (request.FeatureNameAr?.Length > 150)
            throw new ArgumentException("Arabic feature name must not exceed 150 characters.");

        if (string.IsNullOrWhiteSpace(request.DataType) ||
            request.DataType.Trim().Length > 30 ||
            !AllowedFeatureDataTypes.Contains(request.DataType.Trim()))
            throw new ArgumentException("Unsupported feature data type.");

        if (request.HelpText?.Length > 500 || request.HelpTextAr?.Length > 500)
            throw new ArgumentException("Feature help text must not exceed 500 characters.");

        if (request.ValidationPattern?.Length > 1000)
            throw new ArgumentException("Validation pattern must not exceed 1000 characters.");

        if (request.MinValue.HasValue && request.MaxValue.HasValue &&
            request.MinValue.Value > request.MaxValue.Value)
            throw new ArgumentException("Minimum value cannot exceed maximum value.");

        if (request.MinLength < 0 || request.MaxLength < 0)
            throw new ArgumentException("Feature lengths cannot be negative.");

        if (request.MinLength.HasValue && request.MaxLength.HasValue &&
            request.MinLength.Value > request.MaxLength.Value)
            throw new ArgumentException("Minimum length cannot exceed maximum length.");

        if (!string.IsNullOrWhiteSpace(request.ValidationPattern))
        {
            try
            {
                _ = new System.Text.RegularExpressions.Regex(
                    request.ValidationPattern,
                    System.Text.RegularExpressions.RegexOptions.None,
                    TimeSpan.FromSeconds(1));
            }
            catch (ArgumentException)
            {
                throw new ArgumentException("Feature validation pattern is not a valid regular expression.");
            }
        }
    }

    private static void ApplyFeatureRequest(
        AdvertisementFeature feature,
        CreateCategoryFeatureRequest request)
    {
        feature.FeatureName = request.FeatureName.Trim();
        feature.FeatureNameAr = string.IsNullOrWhiteSpace(request.FeatureNameAr)
            ? null : request.FeatureNameAr.Trim();
        feature.DataType = request.DataType.Trim();
        feature.IsRequired = request.IsRequired;
        feature.HelpText = request.HelpText;
        feature.HelpTextAr = request.HelpTextAr;
        feature.MinValue = request.MinValue;
        feature.MaxValue = request.MaxValue;
        feature.MinLength = request.MinLength;
        feature.MaxLength = request.MaxLength;
        feature.ValidationPattern = request.ValidationPattern;
        feature.IsSearchable = request.IsSearchable;
        feature.IsFilterable = request.IsFilterable;
        feature.DisplayOrder = request.DisplayOrder;
    }

    public async Task<int?> CreateFeatureAsync(
        int categoryId, CreateCategoryFeatureRequest request)
    {
        ValidateFeatureRequest(request);

        var category = await _context.AdvertisementCategories
            .FirstOrDefaultAsync(x => x.CategoryID == categoryId && x.IsActive);

        if (category == null)
            return null;

        var name = request.FeatureName.Trim();

        if (await _context.AdvertisementFeatures.AnyAsync(
            x => x.CategoryID == categoryId && x.FeatureName == name))
            throw new ArgumentException("A feature with this name already exists in the category.");

        var feature = new AdvertisementFeature { CategoryID = categoryId };
        ApplyFeatureRequest(feature, request);

        _context.AdvertisementFeatures.Add(feature);
        await _context.SaveChangesAsync();
        return feature.AdvertisementFeatureID;
    }

    public async Task<bool> UpdateFeatureAsync(
        int categoryId, int featureId, UpdateCategoryFeatureRequest request)
    {
        ValidateFeatureRequest(request);

        var feature = await _context.AdvertisementFeatures
            .FirstOrDefaultAsync(x =>
                x.AdvertisementFeatureID == featureId &&
                x.CategoryID == categoryId);

        if (feature == null)
            return false;

        var name = request.FeatureName.Trim();

        if (await _context.AdvertisementFeatures.AnyAsync(x =>
            x.CategoryID == categoryId &&
            x.AdvertisementFeatureID != featureId &&
            x.FeatureName == name))
            throw new ArgumentException("A feature with this name already exists in the category.");

        ApplyFeatureRequest(feature, request);
        feature.IsActive = request.IsActive;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateFeatureStatusAsync(
        int categoryId, int featureId, bool isActive)
    {
        var feature = await _context.AdvertisementFeatures
            .FirstOrDefaultAsync(x =>
                x.AdvertisementFeatureID == featureId &&
                x.CategoryID == categoryId);

        if (feature == null)
            return false;

        feature.IsActive = isActive;
        await _context.SaveChangesAsync();
        return true;
    }

    private async Task<AdvertisementFeature?> FindFeatureAsync(
        int categoryId, int featureId)
    {
        return await _context.AdvertisementFeatures.FirstOrDefaultAsync(x =>
            x.AdvertisementFeatureID == featureId &&
            x.CategoryID == categoryId);
    }

    public async Task<int?> CreateFeatureOptionAsync(
        int categoryId, int featureId, CategoryFeatureOptionRequest request)
    {
        var feature = await FindFeatureAsync(categoryId, featureId);
        if (feature == null)
            return null;

        var value = request.OptionValue?.Trim();

        if (string.IsNullOrWhiteSpace(value) || value.Length > 150)
            throw new ArgumentException("Option value is required and must not exceed 150 characters.");

        if (request.OptionValueAr?.Length > 150)
            throw new ArgumentException("Arabic option value must not exceed 150 characters.");

        if (await _context.AdvertisementFeatureOptions.AnyAsync(x =>
            x.AdvertisementFeatureID == featureId && x.OptionValue == value))
            throw new ArgumentException("This option value already exists for the feature.");

        var option = new AdvertisementFeatureOption
        {
            AdvertisementFeatureID = featureId,
            OptionValue = value,
            OptionValueAr = string.IsNullOrWhiteSpace(request.OptionValueAr)
                ? null : request.OptionValueAr.Trim(),
            DisplayOrder = request.DisplayOrder,
            IsActive = true
        };

        _context.AdvertisementFeatureOptions.Add(option);
        await _context.SaveChangesAsync();
        return option.AdvertisementFeatureOptionID;
    }

    public async Task<bool> UpdateFeatureOptionAsync(
        int categoryId, int featureId, int optionId,
        CategoryFeatureOptionRequest request)
    {
        var feature = await FindFeatureAsync(categoryId, featureId);
        if (feature == null)
            return false;

        var option = await _context.AdvertisementFeatureOptions.FirstOrDefaultAsync(x =>
            x.AdvertisementFeatureOptionID == optionId &&
            x.AdvertisementFeatureID == featureId);

        if (option == null)
            return false;

        var value = request.OptionValue?.Trim();

        if (string.IsNullOrWhiteSpace(value) || value.Length > 150)
            throw new ArgumentException("Option value is required and must not exceed 150 characters.");

        if (request.OptionValueAr?.Length > 150)
            throw new ArgumentException("Arabic option value must not exceed 150 characters.");

        if (await _context.AdvertisementFeatureOptions.AnyAsync(x =>
            x.AdvertisementFeatureID == featureId &&
            x.AdvertisementFeatureOptionID != optionId &&
            x.OptionValue == value))
            throw new ArgumentException("This option value already exists for the feature.");

        option.OptionValue = value;
        option.OptionValueAr = string.IsNullOrWhiteSpace(request.OptionValueAr)
            ? null : request.OptionValueAr.Trim();
        option.DisplayOrder = request.DisplayOrder;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateFeatureOptionStatusAsync(
        int categoryId, int featureId, int optionId, bool isActive)
    {
        var feature = await FindFeatureAsync(categoryId, featureId);
        if (feature == null)
            return false;

        var option = await _context.AdvertisementFeatureOptions.FirstOrDefaultAsync(x =>
            x.AdvertisementFeatureOptionID == optionId &&
            x.AdvertisementFeatureID == featureId);

        if (option == null)
            return false;

        option.IsActive = isActive;
        await _context.SaveChangesAsync();
        return true;
    }
}