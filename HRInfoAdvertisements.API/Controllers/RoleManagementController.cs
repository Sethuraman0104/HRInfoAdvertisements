using System.Security.Claims;
using HRInfoAdvertisements.Application.DTOs.AdminManagement;
using HRInfoAdvertisements.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRInfoAdvertisements.API.Controllers;

[ApiController]
[Route("api/v1/roles")]
[Authorize]
public class RoleManagementController : ControllerBase
{
    private readonly IRoleManagementService _roleManagementService;

    public RoleManagementController(
        IRoleManagementService roleManagementService)
    {
        _roleManagementService = roleManagementService;
    }

    [HttpGet]
    [Authorize(Policy = "USER_VIEW")]
    public async Task<IActionResult> GetRoles()
    {
        var result =
            await _roleManagementService.GetRolesAsync();

        return Ok(result);
    }

    [HttpGet("{roleId:int}")]
    [Authorize(Policy = "USER_VIEW")]
    public async Task<IActionResult> GetRoleById(int roleId)
    {
        var result =
            await _roleManagementService.GetRoleByIdAsync(roleId);

        if (result == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Role not found."
            });
        }

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "USER_EDIT")]
    public async Task<IActionResult> CreateRole(
        [FromBody] CreateRoleRequest request)
    {
        var currentUserId = GetCurrentUserId();

        if (!currentUserId.HasValue)
            return Unauthorized();

        if (string.IsNullOrWhiteSpace(request.RoleName))
        {
            return BadRequest(new
            {
                success = false,
                message = "Role name is required."
            });
        }

        var roleId =
            await _roleManagementService.CreateRoleAsync(
                request.RoleName,
                request.RoleDescription,
                currentUserId.Value);

        if (!roleId.HasValue)
        {
            return BadRequest(new
            {
                success = false,
                message =
                    "Unable to create role. The role name may already exist."
            });
        }

        return CreatedAtAction(
            nameof(GetRoleById),
            new { roleId = roleId.Value },
            new
            {
                success = true,
                roleId = roleId.Value,
                message = "Role created successfully."
            });
    }

    [HttpPut("{roleId:int}")]
    [Authorize(Policy = "USER_EDIT")]
    public async Task<IActionResult> UpdateRole(
        int roleId,
        [FromBody] UpdateRoleRequest request)
    {
        var currentUserId = GetCurrentUserId();

        if (!currentUserId.HasValue)
            return Unauthorized();

        if (string.IsNullOrWhiteSpace(request.RoleName))
        {
            return BadRequest(new
            {
                success = false,
                message = "Role name is required."
            });
        }

        var result =
            await _roleManagementService.UpdateRoleAsync(
                roleId,
                request.RoleName,
                request.RoleDescription,
                currentUserId.Value);

        if (!result)
        {
            return BadRequest(new
            {
                success = false,
                message =
                    "Unable to update role. The role may not exist, "
                    + "the role name may already exist, or the role "
                    + "may be protected."
            });
        }

        return Ok(new
        {
            success = true,
            message = "Role updated successfully."
        });
    }

    [HttpPut("{roleId:int}/status")]
    [Authorize(Policy = "USER_EDIT")]
    public async Task<IActionResult> UpdateRoleStatus(
        int roleId,
        [FromBody] UpdateRoleStatusRequest request)
    {
        var currentUserId = GetCurrentUserId();

        if (!currentUserId.HasValue)
            return Unauthorized();

        var result =
            await _roleManagementService.UpdateRoleStatusAsync(
                roleId,
                request.IsActive,
                currentUserId.Value);

        if (!result)
        {
            return BadRequest(new
            {
                success = false,
                message =
                    "Unable to update role status. The role may not "
                    + "exist, may be protected, or may still be "
                    + "assigned to users."
            });
        }

        return Ok(new
        {
            success = true,
            message = request.IsActive
                ? "Role activated successfully."
                : "Role deactivated successfully."
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