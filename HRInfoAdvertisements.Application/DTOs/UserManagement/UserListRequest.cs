namespace HRInfoAdvertisements.Application.DTOs.UserManagement;

public class UserListRequest
{
    public string? Search { get; set; }

    public string? AccountStatus { get; set; }

    public int? RoleID { get; set; }

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}