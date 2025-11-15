using System.Globalization;
using Chronosystem.Application.Common.Interfaces.Tenancy;
using Chronosystem.Infrastructure.Common;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Chronosystem.Infrastructure.Tenancy.Plans;

public sealed class PlanQuotaService : IPlanQuotaService
{
    private readonly string _connectionString;
    private readonly ITenantSettingsProvider _tenantSettingsProvider;

    public PlanQuotaService(
        IConfiguration configuration,
        ITenantSettingsProvider tenantSettingsProvider)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection not configured.");
        _tenantSettingsProvider = tenantSettingsProvider;
    }

    public QuotaSnapshot GetEffectiveQuotas(string tenant)
    {
        var normalizedTenant = TenancyUtils.NormalizeTenant(tenant);
        if (normalizedTenant is null)
        {
            return new QuotaSnapshot(null, null, int.MaxValue);
        }

        var maxUsers = ParseQuota(_tenantSettingsProvider.GetValue(normalizedTenant, "max_users"));
        var maxUnits = ParseQuota(_tenantSettingsProvider.GetValue(normalizedTenant, "max_units"));
        var horizon = ParsePositiveInt(_tenantSettingsProvider.GetValue(normalizedTenant, "availability_horizon_days"))
            ?? int.MaxValue;

        return new QuotaSnapshot(maxUsers, maxUnits, horizon);
    }

    public async Task<int> CountActiveUsersAsync(string tenant, CancellationToken ct)
    {
        var normalizedTenant = TenancyUtils.NormalizeTenant(tenant);
        if (normalizedTenant is null)
        {
            return 0;
        }

        const string sqlTemplate = "SELECT COUNT(*) FROM {0}.users WHERE deleted_at IS NULL";
        return await ExecuteCountAsync(
            string.Format(
                CultureInfo.InvariantCulture,
                sqlTemplate,
                TenancyUtils.QuoteIdent(normalizedTenant)),
            ct);
    }

    public async Task<int> CountActiveUnitsAsync(string tenant, CancellationToken ct)
    {
        var normalizedTenant = TenancyUtils.NormalizeTenant(tenant);
        if (normalizedTenant is null)
        {
            return 0;
        }

        const string sqlTemplate = "SELECT COUNT(*) FROM {0}.units WHERE deleted_at IS NULL";
        return await ExecuteCountAsync(
            string.Format(
                CultureInfo.InvariantCulture,
                sqlTemplate,
                TenancyUtils.QuoteIdent(normalizedTenant)),
            ct);
    }

    private async Task<int> ExecuteCountAsync(string sql, CancellationToken ct)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(ct);

        await using var command = connection.CreateCommand();
        command.CommandText = sql;

        var result = await command.ExecuteScalarAsync(ct);
        return result is null
            ? 0
            : Convert.ToInt32(result, CultureInfo.InvariantCulture);
    }

    private static int? ParseQuota(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
        {
            return null;
        }

        return parsed < 0 ? null : parsed;
    }

    private static int? ParsePositiveInt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
        {
            return null;
        }

        return parsed <= 0 ? null : parsed;
    }

}
