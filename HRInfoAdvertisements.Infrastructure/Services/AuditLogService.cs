using HRInfoAdvertisements.Application.DTOs.AuditLogs;
using HRInfoAdvertisements.Application.Interfaces;
using HRInfoAdvertisements.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRInfoAdvertisements.Infrastructure.Services;

public class AuditLogService : IAuditLogService
{
    private readonly ApplicationDbContext _context;

    public AuditLogService(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AuditLogListResponse> GetAuditLogsAsync(
        AuditLogListRequest request)
    {
        var pageNumber = request.PageNumber < 1
            ? 1
            : request.PageNumber;

        var pageSize = request.PageSize <= 0
            ? 20
            : Math.Min(request.PageSize, 100);

        var query = _context.AuditLogs
            .AsNoTracking()
            .Include(x => x.User)
            .AsQueryable();

        // ------------------------------------------------------------
        // USER FILTER
        // ------------------------------------------------------------

        if (request.UserID.HasValue)
        {
            query = query.Where(x =>
                x.UserID == request.UserID.Value);
        }

        // ------------------------------------------------------------
        // ACTION FILTER
        // ------------------------------------------------------------

        if (!string.IsNullOrWhiteSpace(request.Action))
        {
            var action = request.Action.Trim();

            query = query.Where(x =>
                x.Action == action);
        }

        // ------------------------------------------------------------
        // ENTITY NAME FILTER
        // ------------------------------------------------------------

        if (!string.IsNullOrWhiteSpace(request.EntityName))
        {
            var entityName = request.EntityName.Trim();

            query = query.Where(x =>
                x.EntityName == entityName);
        }

        // ------------------------------------------------------------
        // ENTITY ID FILTER
        // ------------------------------------------------------------

        if (!string.IsNullOrWhiteSpace(request.EntityID))
        {
            var entityId = request.EntityID.Trim();

            query = query.Where(x =>
                x.EntityID == entityId);
        }

        // ------------------------------------------------------------
        // GENERAL SEARCH
        // ------------------------------------------------------------

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();

            query = query.Where(x =>
                x.Action.Contains(search)
                ||
                x.EntityName.Contains(search)
                ||
                (x.EntityID != null &&
                 x.EntityID.Contains(search))
                ||
                (x.OldValues != null &&
                 x.OldValues.Contains(search))
                ||
                (x.NewValues != null &&
                 x.NewValues.Contains(search))
                ||
                (x.IPAddress != null &&
                 x.IPAddress.Contains(search))
                ||
                (x.UserAgent != null &&
                 x.UserAgent.Contains(search))
                ||
                (x.User != null &&
                 x.User.UserName.Contains(search))
                ||
                (x.User != null &&
                 x.User.Email.Contains(search)));
        }

        // ------------------------------------------------------------
        // DATE RANGE
        // ------------------------------------------------------------

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

        // ------------------------------------------------------------
        // TOTAL COUNT
        // ------------------------------------------------------------

        var totalRecords = await query.CountAsync();

        var totalPages = totalRecords == 0
            ? 0
            : (int)Math.Ceiling(
                totalRecords / (double)pageSize);

        // ------------------------------------------------------------
        // PAGED RESULTS
        // ------------------------------------------------------------

        var items = await query
            .OrderByDescending(x => x.CreatedDate)
            .ThenByDescending(x => x.AuditLogID)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new AuditLogListItemResponse
            {
                AuditLogID = x.AuditLogID,
                UserID = x.UserID,

                UserName = x.User != null
                    ? x.User.UserName
                    : null,

                UserEmail = x.User != null
                    ? x.User.Email
                    : null,

                Action = x.Action,
                EntityName = x.EntityName,
                EntityID = x.EntityID,
                OldValues = x.OldValues,
                NewValues = x.NewValues,
                IPAddress = x.IPAddress,
                UserAgent = x.UserAgent,
                CreatedDate = x.CreatedDate
            })
            .ToListAsync();

        return new AuditLogListResponse
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages
        };
    }
}