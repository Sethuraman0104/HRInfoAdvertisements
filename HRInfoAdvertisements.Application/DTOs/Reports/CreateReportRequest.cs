using System.ComponentModel.DataAnnotations;

namespace HRInfoAdvertisements.Application.DTOs.Reports;

public class CreateReportRequest
{
    [Range(1, long.MaxValue)]
    public long AdvertisementID { get; set; }

    [Range(1, int.MaxValue)]
    public int ReportReasonID { get; set; }

    [MaxLength(2000)]
    public string? Comments { get; set; }
}