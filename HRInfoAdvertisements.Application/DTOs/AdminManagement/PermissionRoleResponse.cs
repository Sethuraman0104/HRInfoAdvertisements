namespace HRInfoAdvertisements.Application.DTOs.AdminManagement;

public class PermissionRoleResponse
{
    public int RoleID { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}