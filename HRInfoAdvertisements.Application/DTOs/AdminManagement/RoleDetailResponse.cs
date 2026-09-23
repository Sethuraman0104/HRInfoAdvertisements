namespace HRInfoAdvertisements.Application.DTOs.AdminManagement;

public class RoleDetailResponse
{
    public int RoleID { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? RoleDescription { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }

    public int UserCount { get; set; }

    public List<RolePermissionResponse> Permissions { get; set; } = new();
}