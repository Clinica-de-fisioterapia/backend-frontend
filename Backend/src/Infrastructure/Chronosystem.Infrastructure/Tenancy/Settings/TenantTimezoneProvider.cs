using System;
using Chronosystem.Application.Common.Interfaces.Tenancy;
using Chronosystem.Infrastructure.Common;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NodaTime;
using Npgsql;

namespace Chronosystem.Infrastructure.Tenancy.Settings;

public sealed class TenantTimezoneProvider : ITenantTimezoneProvider
{
    private readonly string _connectionString;
    private readonly ILogger<TenantTimezoneProvider> _logger;
    private readonly IMemoryCache _cache;

    public TenantTimezoneProvider(IConfiguration cfg, IMemoryCache cache, ILogger<TenantTimezoneProvider> logger)
    {
        _connectionString = cfg.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection not configured.");
        _cache = cache;
        _logger = logger;
    }

    public DateTime GetTodayDateInTenantTz(string tenant)
    {
        var tz = ResolveTz(tenant) ?? "UTC";
        try
        {
            var zone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(tz) ?? DateTimeZoneProviders.Tzdb.GetZoneOrNull("UTC")!;
            var now = SystemClock.Instance.GetCurrentInstant();
            var local = now.InZone(zone).Date;
            return new DateTime(local.Year, local.Month, local.Day, 0, 0, 0, DateTimeKind.Utc);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to compute tenant local date for {tenant} tz={tz}", tenant, tz);
            return DateTime.UtcNow.Date;
        }
    }

    private string? ResolveTz(string tenant)
    {
        var cacheKey = $"tenant-tz:{tenant}";
        return _cache.GetOrCreate(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);

            try
            {
                using var cnn = new NpgsqlConnection(_connectionString);
                cnn.Open();
                using var cmd = cnn.CreateCommand();
                cmd.CommandText = $@"
                    SELECT value
                      FROM {TenancyUtils.QuoteIdent(tenant)}.tenant_settings
                     WHERE key = 'time_zone'
                     LIMIT 1;";
                var result = cmd.ExecuteScalar();
                if (result is string s && !string.IsNullOrWhiteSpace(s))
                {
                    return s.Trim();
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Unable to resolve time_zone from tenant_settings for {tenant}", tenant);
            }

            return null;
        });
    }
}
