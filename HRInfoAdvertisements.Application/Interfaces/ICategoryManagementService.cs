using HRInfoAdvertisements.Application.DTOs.CategoryManagement;

namespace HRInfoAdvertisements.Application.Interfaces;

public interface ICategoryManagementService
{
    Task<CategoryListResponse> GetCategoriesAsync(
        CategoryListRequest request);

    Task<CategoryDetailResponse?> GetCategoryByIdAsync(
        int categoryId);

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
}
