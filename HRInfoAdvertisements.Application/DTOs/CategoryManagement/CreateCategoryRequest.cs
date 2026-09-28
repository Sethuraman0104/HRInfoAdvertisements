using System.ComponentModel.DataAnnotations;

namespace HRInfoAdvertisements.Application.DTOs.CategoryManagement;

public class CreateCategoryRequest
{
    [Required]
    [MaxLength(150)]
    public string CategoryName { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? CategoryNameAr { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public int DisplayOrder { get; set; }
}
