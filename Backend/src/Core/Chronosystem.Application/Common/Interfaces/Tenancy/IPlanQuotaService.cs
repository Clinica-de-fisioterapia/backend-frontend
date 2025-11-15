namespace Chronosystem.Application.Common.Interfaces.Tenancy;

public sealed record QuotaSnapshot(
    int? MaxUsers,
    int? MaxUnits,
    int AvailabilityHorizonDays
);

public interface IPlanQuotaService
{
    QuotaSnapshot GetEffectiveQuotas(string tenant);
    Task<int> CountActiveUsersAsync(string tenant, CancellationToken ct);
    Task<int> CountActiveUnitsAsync(string tenant, CancellationToken ct);
}
