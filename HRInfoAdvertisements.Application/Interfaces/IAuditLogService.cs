using HRInfoAdvertisements.Application.DTOs.AuditLogs;

namespace HRInfoAdvertisements.Application.Interfaces;

public interface IAuditLogService
{
    Task<AuditLogListResponse> GetAuditLogsAsync(
        AuditLogListRequest request);
}