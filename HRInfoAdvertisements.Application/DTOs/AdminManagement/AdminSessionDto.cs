using System;

namespace HRInfoAdvertisements.Application.DTOs.AdminManagement;

public class AdminSessionDto
{
    public long UserSessionID { get; set; }

    public string? DeviceType { get; set; }

    public string? DeviceName { get; set; }

    public string? IPAddress { get; set; }

    public DateTime LastActivityDate { get; set; }

    public bool IsCurrentSession { get; set; }
}