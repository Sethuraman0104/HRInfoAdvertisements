namespace HRInfoAdvertisements.Domain.Entities;

public class User
{
    public long UserID { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? MobileNo { get; set; }

    public string PasswordHash { get; set; } = string.Empty;

    public bool IsEmailVerified { get; set; }

    public bool IsMobileVerified { get; set; }

    public bool IsMFAEnabled { get; set; }

    public string AccountStatus { get; set; } = "Active";

    public int FailedLoginAttempts { get; set; }

    public DateTime? LockoutEndDate { get; set; }

    public DateTime? LastLoginDate { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public long? CreatedBy { get; set; }

    public long? ModifiedBy { get; set; }

    public UserProfile? UserProfile { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public ICollection<Advertisement> Advertisements { get; set; } = new List<Advertisement>();

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public ICollection<UserAddress> UserAddresses { get; set; } = new List<UserAddress>();
}