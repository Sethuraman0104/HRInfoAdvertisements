namespace HRInfoAdvertisements.Application.DTOs.Admin;

public class RejectionReasonResponse
{
    public long RejectionReasonID { get; set; }

    public string ReasonCode { get; set; } = string.Empty;

    public string ReasonName { get; set; } = string.Empty;

    public string? ReasonNameAr { get; set; }

    public bool IsActive { get; set; }
}