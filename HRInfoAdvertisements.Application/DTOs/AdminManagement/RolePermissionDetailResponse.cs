namespace HRInfoAdvertisements.Application.DTOs.AdminManagement;

public class RolePermissionDetailResponse
{
    public int RoleID { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    public List<RolePermissionResponse> Permissions { get; set; } = new();
}