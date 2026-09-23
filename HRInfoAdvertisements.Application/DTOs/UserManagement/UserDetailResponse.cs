namespace HRInfoAdvertisements.Application.DTOs.UserManagement;

public class UserDetailResponse
{
    public long UserID { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? MobileNo { get; set; }

    public string AccountStatus { get; set; } = string.Empty;

    public bool IsEmailVerified { get; set; }

    public bool IsMobileVerified { get; set; }

    public bool IsMFAEnabled { get; set; }

    public int FailedLoginAttempts { get; set; }

    public DateTime? LockoutEndDate { get; set; }

    public DateTime? LastLoginDate { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public UserProfileResponse? Profile { get; set; }

    public List<UserRoleResponse> Roles { get; set; } = new();
}