namespace HRInfoAdvertisements.Application.DTOs.AdminManagement;

public class CreateRoleRequest
{
    public string RoleName { get; set; } = string.Empty;
    public string? RoleDescription { get; set; }
}