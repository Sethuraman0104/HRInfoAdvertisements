using HRInfoAdvertisements.Application.DTOs.AdminManagement;
using HRInfoAdvertisements.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRInfoAdvertisements.API.Controllers;

[ApiController]
[Route("api/v1/permissions")]
[Authorize]
public class PermissionManagementController : ControllerBase
{
    private readonly IPermissionManagementService _permissionManagementService;

    public PermissionManagementController(
        IPermissionManagementService permissionManagementService)
    {
        _permissionManagementService = permissionManagementService;
    }

    // GET: api/v1/permissions
    [HttpGet]
    [Authorize(Policy = "USER_VIEW")]
    public async Task<ActionResult<List<PermissionListResponse>>> GetPermissions()
    {
        var permissions =
            await _permissionManagementService.GetPermissionsAsync();

        return Ok(permissions);
    }

    // GET: api/v1/permissions/{permissionId}
    [HttpGet("{permissionId:int}")]
    [Authorize(Policy = "USER_VIEW")]
    public async Task<ActionResult<PermissionDetailResponse>> GetPermissionById(
        int permissionId)
    {
        var permission =
            await _permissionManagementService.GetPermissionByIdAsync(
                permissionId);

        if (permission == null)
        {
            return NotFound(new
            {
                message = "Permission not found."
            });
        }

        return Ok(permission);
    }
}