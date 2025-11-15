using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Chronosystem.Application.Common.Interfaces.Tenancy;
using Chronosystem.Infrastructure.Common;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Chronosystem.Infrastructure.Caching;

public sealed class AvailabilityTtlProvider : IAvailabilityTtlProvider
{
    private const int CacheSeconds = 60;
    private const int MinimumTtlSeconds = 5;
    private const int DefaultPlanMaxTtlSeconds = 60;

    private readonly string _connectionString;
    private readonly ITenantSettingsProvider _tenantSettingsProvider;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AvailabilityTtlProvider> _logger;

    public AvailabilityTtlProvider(
        IConfiguration configuration,
        ITenantSettingsProvider tenantSettingsProvider,
        IMemoryCache cache,
        ILogger<AvailabilityTtlProvider> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection not configured.");
        _tenantSettingsProvider = tenantSettingsProvider;
        _cache = cache;
        _logger = logger;
    }

    public TimeSpan GetEffectiveTtl(string tenant)
    {
        var normalizedTenant = TenancyUtils.NormalizeTenant(tenant);
        if (normalizedTenant is null)
        {
            return TimeSpan.FromSeconds(DefaultPlanMaxTtlSeconds);
        }

        return _cache.GetOrCreate(
            $"availability-ttl:{normalizedTenant}",
            entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(CacheSeconds);
                return ResolveTtl(normalizedTenant);
            });
    }

    public async Task<TimeSpan> GetEffectiveTtlAsync(string tenant, CancellationToken ct = default)
    {
        var normalizedTenant = TenancyUtils.NormalizeTenant(tenant);
        if (normalizedTenant is null)
        {
            return TimeSpan.FromSeconds(DefaultPlanMaxTtlSeconds);
        }

        if (_cache.TryGetValue($"availability-ttl:{normalizedTenant}", out TimeSpan cached))
        {
            return cached;
        }

        var resolved = await ResolveTtlAsync(normalizedTenant, ct);
        _cache.Set($"availability-ttl:{normalizedTenant}", resolved, TimeSpan.FromSeconds(CacheSeconds));
        return resolved;
    }

    private TimeSpan ResolveTtl(string tenant)
    {
        var planMaxSeconds = GetPlanMaxSeconds(tenant);
        var tenantSettingRaw = _tenantSettingsProvider.GetValue(tenant, "redis_availability_ttl_seconds");
        var tenantOverrideSeconds = ParsePositiveInt(tenantSettingRaw);

        var candidateSeconds = tenantOverrideSeconds ?? planMaxSeconds;
        if (candidateSeconds <= 0)
        {
            candidateSeconds = planMaxSeconds;
        }

        var effectiveSeconds = Math.Max(
            MinimumTtlSeconds,
            Math.Min(candidateSeconds, planMaxSeconds));

        _logger.LogDebug(
            "AvailabilityTTL resolved {tenant} planMax={planMax}s tenantSetting={tenantSetting}s effective={effective}s",
            tenant,
            planMaxSeconds,
            tenantSettingRaw ?? "null",
            effectiveSeconds);

        return TimeSpan.FromSeconds(effectiveSeconds);
    }

    private int GetPlanMaxSeconds(string tenant)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT p.feature_flags->>'redis_max_ttl_seconds'
                FROM public.tenants t
                JOIN public.plans p ON p.code = t.plan_code
                WHERE t.deleted_at IS NULL AND LOWER(t.slug) = LOWER(@slug)
                LIMIT 1;";
            command.Parameters.AddWithValue("slug", tenant);

            var result = command.ExecuteScalar();
            if (result is string raw && int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) && parsed > 0)
            {
                return parsed;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resolve plan max TTL for {tenant}", tenant);
        }

        return DefaultPlanMaxTtlSeconds;
    }

    private async Task<TimeSpan> ResolveTtlAsync(string tenant, CancellationToken ct)
    {
        var planMaxSeconds = await GetPlanMaxSecondsAsync(tenant, ct);
        var tenantSettingRaw = await _tenantSettingsProvider.GetValueAsync(tenant, "redis_availability_ttl_seconds", ct);
        var tenantOverrideSeconds = ParsePositiveInt(tenantSettingRaw);

        var candidateSeconds = tenantOverrideSeconds ?? planMaxSeconds;
        if (candidateSeconds <= 0)
        {
            candidateSeconds = planMaxSeconds;
        }

        var effectiveSeconds = Math.Max(
            MinimumTtlSeconds,
            Math.Min(candidateSeconds, planMaxSeconds));

        _logger.LogDebug(
            "AvailabilityTTL resolved {tenant} planMax={planMax}s tenantSetting={tenantSetting}s effective={effective}s",
            tenant,
            planMaxSeconds,
            tenantSettingRaw ?? "null",
            effectiveSeconds);

        return TimeSpan.FromSeconds(effectiveSeconds);
    }

    private async Task<int> GetPlanMaxSecondsAsync(string tenant, CancellationToken ct)
    {
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(ct);

            await using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT p.feature_flags->>'redis_max_ttl_seconds'
                FROM public.tenants t
                JOIN public.plans p ON p.code = t.plan_code
                WHERE t.deleted_at IS NULL AND LOWER(t.slug) = LOWER(@slug)
                LIMIT 1;";
            command.Parameters.AddWithValue("slug", tenant);

            var result = await command.ExecuteScalarAsync(ct);
            if (result is string raw && int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) && parsed > 0)
            {
                return parsed;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resolve plan max TTL (async) for {tenant}", tenant);
        }

        return DefaultPlanMaxTtlSeconds;
    }

    private static int? ParsePositiveInt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) && parsed > 0
            ? parsed
            : null;
    }

}
