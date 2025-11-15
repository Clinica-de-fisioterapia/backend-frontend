using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Chronosystem.Application.Common.Interfaces.Tenancy;
using Chronosystem.Infrastructure.Common;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Chronosystem.Infrastructure.Tenancy.Settings;

public sealed class TenantSettingsProvider : ITenantSettingsProvider
{
    private const int CacheSeconds = 60;

    private readonly string _connectionString;
    private readonly IMemoryCache _cache;
    private readonly ILogger<TenantSettingsProvider> _logger;

    public TenantSettingsProvider(
        IConfiguration configuration,
        IMemoryCache cache,
        ILogger<TenantSettingsProvider> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection not configured.");
        _cache = cache;
        _logger = logger;
    }

    public string? GetValue(string tenant, string key)
    {
        var normalizedTenant = TenancyUtils.NormalizeTenant(tenant);
        var normalizedKey = NormalizeKey(key);

        if (normalizedTenant is null || normalizedKey is null)
        {
            return null;
        }

        var settings = _cache.GetOrCreate(
            $"tenant-settings:{normalizedTenant}",
            entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(CacheSeconds);
                return LoadSettings(normalizedTenant);
            });

        if (settings is null || settings.Count == 0)
        {
            return null;
        }

        return settings.TryGetValue(normalizedKey, out var value)
            ? value
            : null;
    }

    public async Task<string?> GetValueAsync(string tenant, string key, CancellationToken ct = default)
    {
        var normalizedTenant = TenancyUtils.NormalizeTenant(tenant);
        var normalizedKey = NormalizeKey(key);

        if (normalizedTenant is null || normalizedKey is null)
        {
            return null;
        }

        var settings = await _cache.GetOrCreateAsync(
            $"tenant-settings:{normalizedTenant}",
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(CacheSeconds);
                return await LoadSettingsAsync(normalizedTenant, ct);
            });

        if (settings is null || settings.Count == 0)
        {
            return null;
        }

        return settings.TryGetValue(normalizedKey, out var value)
            ? value
            : null;
    }

    private Dictionary<string, string> LoadSettings(string tenant)
    {
        var settings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = $"SELECT key, value FROM {TenancyUtils.QuoteIdent(tenant)}.tenant_settings";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (!reader.IsDBNull(0))
                {
                    var key = reader.GetString(0);
                    var value = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                    if (!settings.ContainsKey(key))
                    {
                        settings[key] = value;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load tenant settings for {tenant}", tenant);
        }

        return settings;
    }

    private async Task<Dictionary<string, string>> LoadSettingsAsync(string tenant, CancellationToken ct)
    {
        var settings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(ct);

            await using var command = connection.CreateCommand();
            command.CommandText = $"SELECT key, value FROM {TenancyUtils.QuoteIdent(tenant)}.tenant_settings";

            await using var reader = await command.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                if (!await reader.IsDBNullAsync(0, ct))
                {
                    var key = reader.GetString(0);
                    var value = await reader.IsDBNullAsync(1, ct) ? string.Empty : reader.GetString(1);
                    if (!settings.ContainsKey(key))
                    {
                        settings[key] = value;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load tenant settings (async) for {tenant}", tenant);
        }

        return settings;
    }

    private static string? NormalizeKey(string? key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        return key.Trim();
    }

}
