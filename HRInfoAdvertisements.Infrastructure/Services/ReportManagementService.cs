using HRInfoAdvertisements.Application.DTOs.Reports;
using HRInfoAdvertisements.Application.Interfaces;
using HRInfoAdvertisements.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRInfoAdvertisements.Infrastructure.Services;

public class ReportManagementService : IReportManagementService
{
    private readonly ApplicationDbContext _context;

    public ReportManagementService(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ReportListResponse> GetReportsAsync(
        ReportListRequest request)
    {
        var pageNumber = request.PageNumber < 1
            ? 1
            : request.PageNumber;

        var pageSize = request.PageSize <= 0
            ? 20
            : Math.Min(request.PageSize, 100);

        var query = _context.Reports
            .AsNoTracking()
            .Include(x => x.Advertisement)
            .Include(x => x.ReportedByUser)
            .Include(x => x.ReviewedByUser)
            .Include(x => x.ReportReason)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();

            query = query.Where(x =>
                x.Comments != null &&
                x.Comments.Contains(search)
                ||
                x.Advertisement.Title.Contains(search)
                ||
                x.Advertisement.AdvertisementNumber.Contains(search)
                ||
                x.ReportedByUser.UserName.Contains(search)
                ||
                x.ReportedByUser.Email.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var status = request.Status.Trim();

            query = query.Where(x =>
                x.Status == status);
        }

        if (request.ReportReasonID.HasValue)
        {
            query = query.Where(x =>
                x.ReportReasonID == request.ReportReasonID.Value);
        }

        if (request.ReportedByUserID.HasValue)
        {
            query = query.Where(x =>
                x.ReportedByUserID == request.ReportedByUserID.Value);
        }

        if (request.ReviewedByUserID.HasValue)
        {
            query = query.Where(x =>
                x.ReviewedByUserID == request.ReviewedByUserID.Value);
        }

        if (request.FromDate.HasValue)
        {
            var fromDate = request.FromDate.Value.Date;

            query = query.Where(x =>
                x.CreatedDate >= fromDate);
        }

        if (request.ToDate.HasValue)
        {
            var toDateExclusive =
                request.ToDate.Value.Date.AddDays(1);

            query = query.Where(x =>
                x.CreatedDate < toDateExclusive);
        }

        var totalRecords = await query.CountAsync();

        var totalPages = totalRecords == 0
            ? 0
            : (int)Math.Ceiling(
                totalRecords / (double)pageSize);

        var items = await query
            .OrderByDescending(x => x.CreatedDate)
            .ThenByDescending(x => x.ReportID)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ReportListItemResponse
            {
                ReportID = x.ReportID,

                AdvertisementID = x.AdvertisementID,

                AdvertisementTitle =
                    x.Advertisement.Title,

                ReportedByUserID =
                    x.ReportedByUserID,

                ReportedByUserName =
                    x.ReportedByUser.UserName,

                ReportedByEmail =
                    x.ReportedByUser.Email,

                ReportReasonID =
                    x.ReportReasonID,

                ReportReasonCode =
                    x.ReportReason.ReasonCode,

                ReportReasonText =
                    x.ReportReason.ReasonText,

                Comments =
                    x.Comments,

                Status =
                    x.Status,

                ReviewedByUserID =
                    x.ReviewedByUserID,

                ReviewedByUserName =
                    x.ReviewedByUser != null
                        ? x.ReviewedByUser.UserName
                        : null,

                CreatedDate =
                    x.CreatedDate,

                ReviewedDate =
                    x.ReviewedDate
            })
            .ToListAsync();

        return new ReportListResponse
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages
        };
    }
}