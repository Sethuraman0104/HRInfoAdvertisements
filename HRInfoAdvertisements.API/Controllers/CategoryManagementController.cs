using HRInfoAdvertisements.Application.DTOs.CategoryManagement;
using HRInfoAdvertisements.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HRInfoAdvertisements.API.Controllers;

[ApiController]
[Route("api/v1/categories")]
[Authorize]
public class CategoryManagementController : ControllerBase
{
    private readonly ICategoryManagementService _categoryManagementService;

    public CategoryManagementController(
        ICategoryManagementService categoryManagementService)
    {
        _categoryManagementService = categoryManagementService;
    }

    // ------------------------------------------------------------
    // Get Categories
    // ------------------------------------------------------------

    [HttpGet]
    [Authorize(Policy = "ADVERTISEMENT_VIEW")]
    public async Task<IActionResult> GetCategories(
        [FromQuery] CategoryListRequest request)
    {
        var result =
            await _categoryManagementService
                .GetCategoriesAsync(request);

        return Ok(result);
    }

    // ------------------------------------------------------------
    // Get Category
    // ------------------------------------------------------------

    [HttpGet("{categoryId:int}")]
    [Authorize(Policy = "ADVERTISEMENT_VIEW")]
    public async Task<IActionResult> GetCategory(
        int categoryId)
    {
        var result =
            await _categoryManagementService
                .GetCategoryByIdAsync(categoryId);

        if (result == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Category not found."
            });
        }

        return Ok(result);
    }

    // ------------------------------------------------------------
    // Get Category Features
    // ------------------------------------------------------------

    [HttpGet("{categoryId:int}/features")]
    [Authorize(Policy = "ADVERTISEMENT_VIEW")]
    public async Task<IActionResult> GetCategoryFeatures(
        int categoryId)
    {
        var category =
            await _categoryManagementService
                .GetCategoryByIdAsync(categoryId);

        if (category == null || !category.IsActive)
        {
            return NotFound(new
            {
                success = false,
                message = "Active category not found."
            });
        }

        var result =
            await _categoryManagementService
                .GetCategoryFeaturesAsync(categoryId);

        return Ok(result);
    }
    // ------------------------------------------------------------
    [HttpGet("{categoryId:int}/features/manage")]
    [Authorize(Policy = "ADVERTISEMENT_EDIT")]
    public async Task<IActionResult> GetCategoryFeaturesForAdmin(int categoryId)
    {
        var category = await _categoryManagementService.GetCategoryByIdAsync(categoryId);

        if (category == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Category not found."
            });
        }

        var result = await _categoryManagementService
            .GetCategoryFeaturesForAdminAsync(categoryId);

        return Ok(result);
    }
    // Create Category
    // ------------------------------------------------------------

    [HttpPost]
    [Authorize(Policy = "ADVERTISEMENT_EDIT")]
    public async Task<IActionResult> CreateCategory(
        [FromBody] CreateCategoryRequest request)
    {
        var createdBy = GetCurrentUserId();

        if (createdBy == null)
            return Unauthorized();

        var categoryId =
            await _categoryManagementService
                .CreateCategoryAsync(
                    request,
                    createdBy.Value);

        if (categoryId == null)
        {
            return BadRequest(new
            {
                success = false,
                message =
                    "Unable to create category. " +
                    "The category name may already exist."
            });
        }

        var result =
            await _categoryManagementService
                .GetCategoryByIdAsync(categoryId.Value);

        return CreatedAtAction(
            nameof(GetCategory),
            new
            {
                categoryId = categoryId.Value
            },
            result);
    }

    // ------------------------------------------------------------
    // Update Category
    // ------------------------------------------------------------

    [HttpPut("{categoryId:int}")]
    [Authorize(Policy = "ADVERTISEMENT_EDIT")]
    public async Task<IActionResult> UpdateCategory(
        int categoryId,
        [FromBody] UpdateCategoryRequest request)
    {
        var modifiedBy = GetCurrentUserId();

        if (modifiedBy == null)
            return Unauthorized();

        var updated =
            await _categoryManagementService
                .UpdateCategoryAsync(
                    categoryId,
                    request,
                    modifiedBy.Value);

        if (!updated)
        {
            return BadRequest(new
            {
                success = false,
                message =
                    "Unable to update category. " +
                    "The category may not exist or " +
                    "the category name may already be in use."
            });
        }

        var result =
            await _categoryManagementService
                .GetCategoryByIdAsync(categoryId);

        return Ok(result);
    }

    // ------------------------------------------------------------
    // Update Status
    // ------------------------------------------------------------

    [HttpPut("{categoryId:int}/status")]
    [Authorize(Policy = "ADVERTISEMENT_EDIT")]
    public async Task<IActionResult> UpdateCategoryStatus(
        int categoryId,
        [FromBody] UpdateCategoryStatusRequest request)
    {
        var modifiedBy = GetCurrentUserId();

        if (modifiedBy == null)
            return Unauthorized();

        var updated =
            await _categoryManagementService
                .UpdateCategoryStatusAsync(
                    categoryId,
                    request.IsActive,
                    modifiedBy.Value);

        if (!updated)
        {
            return NotFound(new
            {
                success = false,
                message = "Category not found."
            });
        }

        return Ok(new
        {
            success = true,
            message = request.IsActive
                ? "Category activated successfully."
                : "Category deactivated successfully."
        });
    }

    // ------------------------------------------------------------
    // Create Feature
    // ------------------------------------------------------------

    [HttpPost("{categoryId:int}/features")]
    [Authorize(Policy = "ADVERTISEMENT_EDIT")]
    public async Task<IActionResult> CreateCategoryFeature(
        int categoryId,
        [FromBody] CreateCategoryFeatureRequest request)
    {
        try
        {
            var featureId = await _categoryManagementService
                .CreateFeatureAsync(categoryId, request);

            if (featureId == null)
                return NotFound(new { success = false, message = "Active category not found." });

            return Ok(new
            {
                success = true,
                message = "Feature created successfully.",
                featureId = featureId.Value
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    // ------------------------------------------------------------
    // Update Feature
    // ------------------------------------------------------------

    [HttpPut("{categoryId:int}/features/{featureId:int}")]
    [Authorize(Policy = "ADVERTISEMENT_EDIT")]
    public async Task<IActionResult> UpdateCategoryFeature(
        int categoryId,
        int featureId,
        [FromBody] UpdateCategoryFeatureRequest request)
    {
        try
        {
            var updated = await _categoryManagementService
                .UpdateFeatureAsync(categoryId, featureId, request);

            if (!updated)
                return NotFound(new { success = false, message = "Feature not found in this category." });

            return Ok(new { success = true, message = "Feature updated successfully." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    // ------------------------------------------------------------
    // Update Feature Status
    // ------------------------------------------------------------

    [HttpPut("{categoryId:int}/features/{featureId:int}/status")]
    [Authorize(Policy = "ADVERTISEMENT_EDIT")]
    public async Task<IActionResult> UpdateCategoryFeatureStatus(
        int categoryId,
        int featureId,
        [FromBody] CategoryFeatureStatusRequest request)
    {
        var updated = await _categoryManagementService
            .UpdateFeatureStatusAsync(categoryId, featureId, request.IsActive);

        if (!updated)
            return NotFound(new { success = false, message = "Feature not found in this category." });

        return Ok(new
        {
            success = true,
            message = request.IsActive
                ? "Feature activated successfully."
                : "Feature deactivated successfully."
        });
    }

    // ------------------------------------------------------------
    // Create Feature Option
    // ------------------------------------------------------------

    [HttpPost("{categoryId:int}/features/{featureId:int}/options")]
    [Authorize(Policy = "ADVERTISEMENT_EDIT")]
    public async Task<IActionResult> CreateCategoryFeatureOption(
        int categoryId,
        int featureId,
        [FromBody] CategoryFeatureOptionRequest request)
    {
        try
        {
            var optionId = await _categoryManagementService
                .CreateFeatureOptionAsync(categoryId, featureId, request);

            if (optionId == null)
                return NotFound(new { success = false, message = "Feature not found in this category." });

            return Ok(new
            {
                success = true,
                message = "Feature option created successfully.",
                optionId = optionId.Value
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    // ------------------------------------------------------------
    // Update Feature Option
    // ------------------------------------------------------------

    [HttpPut("{categoryId:int}/features/{featureId:int}/options/{optionId:int}")]
    [Authorize(Policy = "ADVERTISEMENT_EDIT")]
    public async Task<IActionResult> UpdateCategoryFeatureOption(
        int categoryId,
        int featureId,
        int optionId,
        [FromBody] CategoryFeatureOptionRequest request)
    {
        try
        {
            var updated = await _categoryManagementService
                .UpdateFeatureOptionAsync(categoryId, featureId, optionId, request);

            if (!updated)
                return NotFound(new { success = false, message = "Feature or option not found in this category." });

            return Ok(new { success = true, message = "Feature option updated successfully." });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    // ------------------------------------------------------------
    // Update Feature Option Status
    // ------------------------------------------------------------

    [HttpPut("{categoryId:int}/features/{featureId:int}/options/{optionId:int}/status")]
    [Authorize(Policy = "ADVERTISEMENT_EDIT")]
    public async Task<IActionResult> UpdateCategoryFeatureOptionStatus(
        int categoryId,
        int featureId,
        int optionId,
        [FromBody] CategoryFeatureStatusRequest request)
    {
        var updated = await _categoryManagementService
            .UpdateFeatureOptionStatusAsync(
                categoryId, featureId, optionId, request.IsActive);

        if (!updated)
            return NotFound(new { success = false, message = "Feature or option not found in this category." });

        return Ok(new
        {
            success = true,
            message = request.IsActive
                ? "Feature option activated successfully."
                : "Feature option deactivated successfully."
        });
    }
    private long? GetCurrentUserId()
    {
        var userIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("userId")?.Value
            ?? User.FindFirst("UserID")?.Value;

        if (long.TryParse(userIdClaim, out var userId))
            return userId;

        return null;
    }
}