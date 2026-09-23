namespace HRInfoAdvertisements.Application.DTOs.Admin;

public class RejectAdvertisementRequest
{
    public long? RejectionReasonID { get; set; }

    public string? Comments { get; set; }
}