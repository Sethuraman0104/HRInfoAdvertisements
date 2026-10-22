namespace HRInfoAdvertisements.Application.DTOs.CategoryManagement;

public sealed class UpdateCategoryFeatureRequest : CreateCategoryFeatureRequest
{
    public bool IsActive { get; set; } = true;
}