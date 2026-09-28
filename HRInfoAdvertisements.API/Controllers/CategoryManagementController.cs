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