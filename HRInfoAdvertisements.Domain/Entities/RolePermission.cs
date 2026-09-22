namespace HRInfoAdvertisements.Domain.Entities;

public class RolePermission
{
    public long RolePermissionID { get; set; }

    public int RoleID { get; set; }

    public int PermissionID { get; set; }

    public DateTime CreatedDate { get; set; }

    public Role Role { get; set; } = null!;

    public Permission Permission { get; set; } = null!;
}