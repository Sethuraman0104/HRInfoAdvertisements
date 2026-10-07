namespace HRInfoAdvertisements.Application.DTOs.Admin;

public class ApproveAdvertisementRequest
{
    public bool IsFeatured { get; set; }
    public string? Comments { get; set; }
}