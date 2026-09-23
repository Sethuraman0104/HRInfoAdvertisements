using System.ComponentModel.DataAnnotations;

namespace HRInfoAdvertisements.Application.DTOs.Dashboard;

public class DashboardStatisticsRequest
{
    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    [RegularExpression(
        "^(day|week|month)$",
        ErrorMessage = "GroupBy must be day, week, or month.")]
    public string GroupBy { get; set; } = "day";
}