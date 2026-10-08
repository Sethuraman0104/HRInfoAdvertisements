namespace HRInfoAdvertisements.Application.DTOs.AdvertisementRemoval;

public static class RemovalRequestTypes
{
    public const string Delete = "DELETE";
    public const string Unpublish = "UNPUBLISH";
}

public static class RemovalRequestStatuses
{
    public const string Pending = "PENDING";
    public const string Approved = "APPROVED";
    public const string Rejected = "REJECTED";
    public const string Cancelled = "CANCELLED";
}

public sealed class CreateAdvertisementRemovalRequest
{
    /// <summary>DELETE or UNPUBLISH.</summary>
    public string RequestType { get; set; } = string.Empty;

    public string? Reason { get; set; }
}

public sealed class AdvertisementRemovalRequestResponse
{
    public long RemovalRequestID { get; set; }

    public long AdvertisementID { get; set; }

    public string RequestType { get; set; } = string.Empty;

    public string? Reason { get; set; }

    public string RequestStatus { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; }

    public DateTime? ReviewedDate { get; set; }

    public string? ReviewComments { get; set; }
}

public sealed class AdminAdvertisementRemovalRequestResponse
{
    public long RemovalRequestID { get; set; }

    public long AdvertisementID { get; set; }

    public string AdvertisementNumber { get; set; } = string.Empty;

    public string AdvertisementTitle { get; set; } = string.Empty;

    public long UserID { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string RequestType { get; set; } = string.Empty;

    public string? Reason { get; set; }

    public string RequestStatus { get; set; } = string.Empty;

    public string AdvertisementStatusCode { get; set; } = string.Empty;

    public string AdvertisementStatusName { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; }

    public long? ReviewedBy { get; set; }

    public DateTime? ReviewedDate { get; set; }

    public string? ReviewComments { get; set; }
}