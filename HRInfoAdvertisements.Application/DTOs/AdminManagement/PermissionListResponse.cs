namespace HRInfoAdvertisements.Application.DTOs.AdminManagement;

public class PermissionListResponse
{
    public int PermissionID { get; set; }
    public string PermissionCode { get; set; } = string.Empty;
    public string PermissionName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int RoleCount { get; set; }
}