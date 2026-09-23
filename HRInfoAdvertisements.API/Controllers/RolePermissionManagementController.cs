using HRInfoAdvertisements.Application.DTOs.AdminManagement;
using HRInfoAdvertisements.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HRInfoAdvertisements.API.Controllers;

[ApiController]
[Route("api/v1/roles")]
[Authorize]
public class RolePermissionManagementController : ControllerBase
{
    private readonly IRolePermissionManagementService _service;

    public RolePermissionManagementController(
        IRolePermissionManagementService service)
    {
        _service = service;
    }

    // GET: api/v1/roles/{roleId}/permissions
    [HttpGet("{roleId:int}/permissions")]
    [Authorize(Policy = "USER_VIEW")]
    public async Task<ActionResult<RolePermissionDetailResponse>>
        GetRolePermissions(int roleId)
    {
        var result = await _service.GetRolePermissionsAsync(roleId);

        if (result == null)
        {
            return NotFound(new
            {
                message = "Role not found."
            });
        }

        return Ok(result);
    }

    // POST: api/v1/roles/{roleId}/permissions
    [HttpPost("{roleId:int}/permissions")]
    [Authorize(Policy = "USER_EDIT")]
    public async Task<IActionResult> AssignPermission(
        int roleId,
        [FromBody] AssignRolePermissionRequest request)
    {
        if (request.PermissionID <= 0)
        {
            return BadRequest(new
            {
                message = "PermissionID must be greater than zero."
            });
        }

        var userIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!long.TryParse(userIdClaim, out var modifiedBy))
        {
            return Unauthorized(new
            {
                message = "Unable to identify the current user."
            });
        }

        var role = await _service.GetRolePermissionsAsync(roleId);

        if (role == null)
        {
            return NotFound(new
            {
                message = "Role not found."
            });
        }

        if (!role.IsActive)
        {
            return BadRequest(new
            {
                message = "Cannot assign a permission to an inactive role."
            });
        }

        var alreadyAssigned = role.Permissions.Any(
            x => x.PermissionID == request.PermissionID);

        if (alreadyAssigned)
        {
            return Conflict(new
            {
                message = "Permission is already assigned to this role."
            });
        }

        var result = await _service.AssignPermissionAsync(
            roleId,
            request.PermissionID,
            modifiedBy);

        if (!result)
        {
            return BadRequest(new
            {
                message = "Unable to assign the permission. Verify that the role and permission are active."
            });
        }

        return Ok(new
        {
            message = "Permission assigned successfully.",
            roleID = roleId,
            permissionID = request.PermissionID
        });
    }

        // DELETE: api/v1/roles/{roleId}/permissions/{permissionId}
    [HttpDelete("{roleId:int}/permissions/{permissionId:int}")]
    [Authorize(Policy = "USER_EDIT")]
    public async Task<IActionResult> RemovePermission(
        int roleId,
        int permissionId)
    {
        if (permissionId <= 0)
        {
            return BadRequest(new
            {
                message = "PermissionID must be greater than zero."
            });
        }

        var userIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!long.TryParse(userIdClaim, out var modifiedBy))
        {
            return Unauthorized(new
            {
                message = "Unable to identify the current user."
            });
        }

        var role = await _service.GetRolePermissionsAsync(roleId);

        if (role == null)
        {
            return NotFound(new
            {
                message = "Role not found."
            });
        }

        if (!role.IsActive)
        {
            return BadRequest(new
            {
                message = "Cannot remove a permission from an inactive role."
            });
        }

        var permission =
            role.Permissions.FirstOrDefault(
                x => x.PermissionID == permissionId);

        if (permission == null)
        {
            return NotFound(new
            {
                message = "Permission is not assigned to this role."
            });
        }

        if (string.Equals(
            role.RoleName,
            "SuperAdmin",
            StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                message = "Permissions cannot be removed from the SuperAdmin role."
            });
        }

        var result = await _service.RemovePermissionAsync(
            roleId,
            permissionId,
            modifiedBy);

        if (!result)
        {
            return BadRequest(new
            {
                message = "Unable to remove the permission."
            });
        }

        return Ok(new
        {
            message = "Permission removed successfully.",
            roleID = roleId,
            permissionID = permissionId
        });
    }
}