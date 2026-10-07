using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRInfoAdvertisements.Domain.Entities;

[Table("AdvertisementRemovalRequests")]
public class AdvertisementRemovalRequest
{
    [Key]
    public long RemovalRequestID { get; set; }

    public long AdvertisementID { get; set; }

    public long UserID { get; set; }

    [Required, MaxLength(20)]
    public string RequestType { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Reason { get; set; }

    [Required, MaxLength(20)]
    public string RequestStatus { get; set; } = "PENDING";

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public long? ReviewedBy { get; set; }

    public DateTime? ReviewedDate { get; set; }

    [MaxLength(500)]
    public string? ReviewComments { get; set; }
}