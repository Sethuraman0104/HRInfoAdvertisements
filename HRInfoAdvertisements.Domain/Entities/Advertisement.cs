namespace HRInfoAdvertisements.Domain.Entities;

public class Advertisement
{
    public long AdvertisementID { get; set; }

    public long UserID { get; set; }

    public int CategoryID { get; set; }

    public int AdvertisementTypeID { get; set; }

    public int StatusID { get; set; }

    public string AdvertisementNumber { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? TitleAr { get; set; }

    public string Description { get; set; } = string.Empty;

    public string? DescriptionAr { get; set; }

    public decimal? Price { get; set; }

    public string CurrencyCode { get; set; } = "BHD";

    public bool IsNegotiable { get; set; }

    public int? CountryID { get; set; }

    public int? StateID { get; set; }

    public int? CityID { get; set; }

    public int? AreaID { get; set; }

    public string? AddressLine { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public string? PlotNumber { get; set; }

    public decimal? LandArea { get; set; }

    public decimal? BuiltUpArea { get; set; }

    public int? Bedrooms { get; set; }

    public int? Bathrooms { get; set; }

    public int? PropertyAge { get; set; }

    public bool IsFeatured { get; set; }

    public DateTime? FeaturedUntil { get; set; }

    public DateTime? PublishedDate { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public long? CreatedBy { get; set; }

    public long? ModifiedBy { get; set; }

    public User User { get; set; } = null!;

    public AdvertisementCategory Category { get; set; } = null!;

    public AdvertisementType AdvertisementType { get; set; } = null!;

    public AdvertisementStatus Status { get; set; } = null!;

    public Country? Country { get; set; }

    public State? State { get; set; }

    public City? City { get; set; }

    public Area? Area { get; set; }

    public ICollection<AdvertisementFeatureValue> FeatureValues { get; set; }
        = new List<AdvertisementFeatureValue>();

    public ICollection<AdvertisementImage> Images { get; set; }
        = new List<AdvertisementImage>();

    public ICollection<AdvertisementVideo> Videos { get; set; }
        = new List<AdvertisementVideo>();

    public ICollection<AdvertisementDocument> Documents { get; set; }
        = new List<AdvertisementDocument>();

    public ICollection<Favorite> Favorites { get; set; }
        = new List<Favorite>();

    public ICollection<SavedSearch> SavedSearches { get; set; }
        = new List<SavedSearch>();

    public ICollection<AdvertisementView> Views { get; set; }
        = new List<AdvertisementView>();

    public ICollection<AdvertisementEnquiry> Enquiries { get; set; }
        = new List<AdvertisementEnquiry>();

    public ICollection<Message> Messages { get; set; }
        = new List<Message>();

    public ICollection<ApprovalRequest> ApprovalRequests { get; set; }
        = new List<ApprovalRequest>();

    public ICollection<Report> Reports { get; set; }
        = new List<Report>();
}