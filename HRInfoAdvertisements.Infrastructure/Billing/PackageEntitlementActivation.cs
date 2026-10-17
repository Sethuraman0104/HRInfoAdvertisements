using HRInfoAdvertisements.Domain.Billing;
using HRInfoAdvertisements.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace HRInfoAdvertisements.Infrastructure.Billing;

internal static class PackageEntitlementActivation
{
    public static async Task ActivateDueAsync(
        ApplicationDbContext context,
        long userId,
        DateTime now,
        CancellationToken cancellationToken = default)
    {
        var transaction = context.Database.CurrentTransaction
            ?? throw new InvalidOperationException(
                "Package activation requires an active database transaction.");

        await using var command =
            context.Database.GetDbConnection().CreateCommand();

        command.Transaction = transaction.GetDbTransaction();
        command.CommandText = """
            DECLARE @result int;
            EXEC @result = sp_getapplock
                @Resource = @resource,
                @LockMode = 'Exclusive',
                @LockOwner = 'Transaction',
                @LockTimeout = 10000;
            SELECT @result;
            """;

        var parameter = command.CreateParameter();
        parameter.ParameterName = "@resource";
        parameter.Value =
            $"HRInfoAdvertisements:PackageActivation:User:{userId}";
        command.Parameters.Add(parameter);

        var lockResult = Convert.ToInt32(
            await command.ExecuteScalarAsync(cancellationToken));

        if (lockResult < 0)
            throw new InvalidOperationException(
                "Could not acquire the package activation lock.");

        while (true)
        {
            var expired = await context.UserPackageEntitlements
                .Where(x =>
                    x.UserID == userId &&
                    x.Status == UserPackageEntitlementStatuses.Active &&
                    x.EndDate.HasValue &&
                    x.EndDate.Value <= now)
                .ToListAsync(cancellationToken);

            foreach (var item in expired)
            {
                item.Status = UserPackageEntitlementStatuses.Expired;
                item.ModifiedDate = now;
            }

            if (expired.Count > 0)
                await context.SaveChangesAsync(cancellationToken);

            var hasActive = await context.UserPackageEntitlements
                .AnyAsync(x =>
                    x.UserID == userId &&
                    x.Status == UserPackageEntitlementStatuses.Active &&
                    x.StartDate <= now &&
                    (!x.EndDate.HasValue || x.EndDate.Value > now),
                    cancellationToken);

            if (hasActive)
                return;

            var next = await context.UserPackageEntitlements
                .Where(x =>
                    x.UserID == userId &&
                    x.Status == UserPackageEntitlementStatuses.Pending &&
                    x.StartDate <= now &&
                    x.EnquiryPackage.PackageCode != "FREE" &&
                    x.Payments.Any(p =>
                        p.Status == PaymentTransactionStatuses.Confirmed &&
                        p.ConfirmedDate.HasValue &&
                        !p.RefundedDate.HasValue))
                .OrderBy(x => x.StartDate)
                .ThenBy(x => x.UserPackageEntitlementID)
                .FirstOrDefaultAsync(cancellationToken);

            if (next == null)
                return;

            next.Status = UserPackageEntitlementStatuses.Active;
            next.ModifiedDate = now;
            await context.SaveChangesAsync(cancellationToken);

            // If an old queued package has already expired, advance again.
            if (next.EndDate.HasValue && next.EndDate.Value <= now)
            {
                next.Status = UserPackageEntitlementStatuses.Expired;
                next.ModifiedDate = now;
                await context.SaveChangesAsync(cancellationToken);
                continue;
            }

            return;
        }
    }
}