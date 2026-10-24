using HRInfoAdvertisements.Application.DTOs.CategoryManagement;

namespace HRInfoAdvertisements.Application.Interfaces;

public interface ICategoryManagementService
{
    Task<CategoryListResponse> GetCategoriesAsync(
        CategoryListRequest request);

    Task<CategoryDetailResponse?> GetCategoryByIdAsync(
        int categoryId);

    Task<List<CategoryFeatureResponse>> GetCategoryFeaturesAsync(int categoryId);
    Task<List<CategoryFeatureResponse>> GetCategoryFeaturesForAdminAsync(int categoryId);
    Task<int?> CreateCategoryAsync(
        CreateCategoryRequest request,
        long createdBy);

    Task<bool> UpdateCategoryAsync(
        int categoryId,
        UpdateCategoryRequest request,
        long modifiedBy);

    Task<bool> UpdateCategoryStatusAsync(
        int categoryId,
        bool isActive,
        long modifiedBy);

    Task<int?> CreateFeatureAsync(
        int categoryId, CreateCategoryFeatureRequest request);

    Task<bool> UpdateFeatureAsync(
        int categoryId, int featureId, UpdateCategoryFeatureRequest request);

    Task<bool> UpdateFeatureStatusAsync(
        int categoryId, int featureId, bool isActive);

    Task<int?> CreateFeatureOptionAsync(
        int categoryId, int featureId, CategoryFeatureOptionRequest request);

    Task<bool> UpdateFeatureOptionAsync(
        int categoryId, int featureId, int optionId, CategoryFeatureOptionRequest request);

    Task<bool> UpdateFeatureOptionStatusAsync(
        int categoryId, int featureId, int optionId, bool isActive);
}