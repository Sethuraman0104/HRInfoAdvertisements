using System;
using System.Collections.Generic;

namespace HRInfoAdvertisements.Application.DTOs.AdminManagement;

public class AdminProfileResponse
{
    public long UserID { get; set; }

    public string? UserName { get; set; }

    public string? Email { get; set; }

    public string? MobileNo { get; set; }

    public bool IsActive { get; set; }

    public bool IsEmailVerified { get; set; }

    public bool IsMobileVerified { get; set; }

    public bool IsMFAEnabled { get; set; }

    public int FailedLoginAttempts { get; set; }

    public DateTime? LastLoginDate { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public DateTime? LockoutEndDate { get; set; }

    public string? AccountStatus { get; set; }

    public AdminProfileDto? Profile { get; set; }

    public AdminAddressDto? PrimaryAddress { get; set; }

    public List<AdminRoleDto> Roles { get; set; } = new();

    public List<AdminSessionDto> ActiveSessions { get; set; } = new();
}