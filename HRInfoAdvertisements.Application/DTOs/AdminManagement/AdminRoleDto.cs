namespace HRInfoAdvertisements.Application.DTOs.AdminManagement;

public class AdminRoleDto
{
    public long RoleID { get; set; }

    public string? RoleName { get; set; }

    public string? RoleDescription { get; set; }

    public bool IsActive { get; set; }
}