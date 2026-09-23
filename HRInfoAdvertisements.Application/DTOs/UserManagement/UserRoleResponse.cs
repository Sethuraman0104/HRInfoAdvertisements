namespace HRInfoAdvertisements.Application.DTOs.UserManagement;

public class UserRoleResponse
{
    public int RoleID { get; set; }

    public string RoleName { get; set; } = string.Empty;

    public string? RoleDescription { get; set; }

    public bool IsActive { get; set; }
}