using System.Security.Claims;
using HRInfoAdvertisements.Application.DTOs.UserManagement;
using HRInfoAdvertisements.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRInfoAdvertisements.API.Controllers;

[ApiController]
[Route("api/v1/users")]
[Authorize]
public class UserManagementController : ControllerBase
{
    private readonly IUserManagementService _userManagementService;

    public UserManagementController(
        IUserManagementService userManagementService)
    {
        _userManagementService = userManagementService;
    }

    // ============================================================
    // GET USERS
    // ============================================================

    [HttpGet]
    [Authorize(Policy = "USER_VIEW")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] UserListRequest request)
    {
        var result =
            await _userManagementService.GetUsersAsync(request);

        return Ok(result);
    }

    // ============================================================
    // GET USER BY ID
    // ============================================================

    [HttpGet("{userId:long}")]
    [Authorize(Policy = "USER_VIEW")]
    public async Task<IActionResult> GetUserById(
        long userId)
    {
        var result =
            await _userManagementService
                .GetUserByIdAsync(userId);

        if (result == null)
        {
            return NotFound(new
            {
                success = false,
                message = "User not found."
            });
        }

        return Ok(result);
    }

    // ============================================================
    // UPDATE ACCOUNT STATUS
    // ============================================================

    [HttpPut("{userId:long}/status")]
    [Authorize(Policy = "USER_SUSPEND")]
    public async Task<IActionResult> UpdateUserStatus(
        long userId,
        [FromBody] UpdateUserStatusRequest request)
    {
        var currentUserId = GetCurrentUserId();

        if (!currentUserId.HasValue)
        {
            return Unauthorized();
        }

        if (currentUserId.Value == userId)
        {
            return BadRequest(new
            {
                success = false,
                message =
                    "You cannot change your own account status."
            });
        }

        var result =
            await _userManagementService
                .UpdateUserStatusAsync(
                    userId,
                    request.AccountStatus,
                    currentUserId.Value);

        if (!result)
        {
            return BadRequest(new
            {
                success = false,
                message =
                    "Unable to update user account status."
            });
        }

        return Ok(new
        {
            success = true,
            message =
                "User account status updated successfully."
        });
    }

    // ============================================================
    // LOCK / UNLOCK USER
    // ============================================================

    [HttpPut("{userId:long}/lock")]
    [Authorize(Policy = "USER_SUSPEND")]
    public async Task<IActionResult> UpdateUserLock(
        long userId,
        [FromBody] UpdateUserLockRequest request)
    {
        var currentUserId = GetCurrentUserId();

        if (!currentUserId.HasValue)
        {
            return Unauthorized();
        }

        if (currentUserId.Value == userId)
        {
            return BadRequest(new
            {
                success = false,
                message =
                    "You cannot lock or unlock your own account."
            });
        }

        var result =
            await _userManagementService
                .UpdateUserLockAsync(
                    userId,
                    request.IsLocked,
                    request.LockoutEndDate,
                    currentUserId.Value);

        if (!result)
        {
            return BadRequest(new
            {
                success = false,
                message =
                    "Unable to update user lock status."
            });
        }

        return Ok(new
        {
            success = true,
            message =
                request.IsLocked
                    ? "User account locked successfully."
                    : "User account unlocked successfully."
        });
    }

    // ============================================================
    // ASSIGN ROLE
    // ============================================================

    [HttpPost("{userId:long}/roles")]
    [Authorize(Policy = "USER_EDIT")]
    public async Task<IActionResult> AssignRole(
        long userId,
        [FromBody] AssignUserRoleRequest request)
    {
        var currentUserId = GetCurrentUserId();

        if (!currentUserId.HasValue)
        {
            return Unauthorized();
        }

        var result =
            await _userManagementService
                .AssignRoleAsync(
                    userId,
                    request.RoleID,
                    currentUserId.Value);

        if (!result)
        {
            return BadRequest(new
            {
                success = false,
                message =
                    "Unable to assign the requested role."
            });
        }

        return Ok(new
        {
            success = true,
            message =
                "Role assigned successfully."
        });
    }

    // ============================================================
    // REMOVE ROLE
    // ============================================================

    [HttpDelete("{userId:long}/roles/{roleId:int}")]
    [Authorize(Policy = "USER_EDIT")]
    public async Task<IActionResult> RemoveRole(
        long userId,
        int roleId)
    {
        var currentUserId = GetCurrentUserId();

        if (!currentUserId.HasValue)
        {
            return Unauthorized();
        }

        if (currentUserId.Value == userId)
        {
            return BadRequest(new
            {
                success = false,
                message =
                    "You cannot remove your own role."
            });
        }

        var result =
            await _userManagementService
                .RemoveRoleAsync(
                    userId,
                    roleId,
                    currentUserId.Value);

        if (!result)
        {
            return BadRequest(new
            {
                success = false,
                message =
                    "Unable to remove the requested role."
            });
        }

        return Ok(new
        {
            success = true,
            message =
                "Role removed successfully."
        });
    }

    // ============================================================
    // CURRENT USER ID
    // ============================================================

    private long? GetCurrentUserId()
    {
        var userIdClaim =
            User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value
            ??
            User.FindFirst("userId")?.Value
            ??
            User.FindFirst("UserID")?.Value;

        if (long.TryParse(
                userIdClaim,
                out var userId))
        {
            return userId;
        }

        return null;
    }
}