namespace HRInfoAdvertisements.Domain.Entities;

public class AdvertisementDocument
{
    public long AdvertisementDocumentID { get; set; }

    public long AdvertisementID { get; set; }

    public string DocumentType { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string S3Key { get; set; } = string.Empty;

    public string? FileURL { get; set; }

    public string VerificationStatus { get; set; } = string.Empty;

    public long? VerifiedBy { get; set; }

    public DateTime? VerifiedDate { get; set; }

    public string? RejectionReason { get; set; }

    public DateTime CreatedDate { get; set; }

    public Advertisement Advertisement { get; set; } = null!;
}