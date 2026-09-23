namespace HRInfoAdvertisements.Application.DTOs.AdminManagement;

public class PermissionDetailResponse
{
    public int PermissionID { get; set; }
    public string PermissionCode { get; set; } = string.Empty;
    public string PermissionName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }

    public List<PermissionRoleResponse> Roles { get; set; } = new();
}