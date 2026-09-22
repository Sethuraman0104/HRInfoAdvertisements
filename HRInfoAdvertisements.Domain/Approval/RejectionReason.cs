namespace HRInfoAdvertisements.Domain.Entities;

public class RejectionReason
{
    public int RejectionReasonID { get; set; }

    public string ReasonCode { get; set; } = string.Empty;

    public string ReasonText { get; set; } = string.Empty;

    public string? ReasonTextAr { get; set; }

    public bool IsActive { get; set; } = true;

    public int DisplayOrder { get; set; }
}