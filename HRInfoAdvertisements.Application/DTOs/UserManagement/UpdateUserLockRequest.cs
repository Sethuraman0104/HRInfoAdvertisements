namespace HRInfoAdvertisements.Application.DTOs.UserManagement;

public class UpdateUserLockRequest
{
    public bool IsLocked { get; set; }

    public DateTime? LockoutEndDate { get; set; }
}