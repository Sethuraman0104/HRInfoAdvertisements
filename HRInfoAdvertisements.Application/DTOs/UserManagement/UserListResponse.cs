namespace HRInfoAdvertisements.Application.DTOs.UserManagement;

public class UserListResponse
{
    public List<UserListItemResponse> Items { get; set; } = new();

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalRecords { get; set; }

    public int TotalPages { get; set; }
}