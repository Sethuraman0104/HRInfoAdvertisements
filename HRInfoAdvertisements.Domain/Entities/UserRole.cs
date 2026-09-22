namespace HRInfoAdvertisements.Domain.Entities;

public class UserRole
{
    public long UserRoleID { get; set; }

    public long UserID { get; set; }

    public int RoleID { get; set; }

    public DateTime CreatedDate { get; set; }

    public User User { get; set; } = null!;

    public Role Role { get; set; } = null!;
}