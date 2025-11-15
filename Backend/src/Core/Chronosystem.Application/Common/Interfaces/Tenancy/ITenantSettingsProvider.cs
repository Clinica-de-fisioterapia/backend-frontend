using System.Threading;
using System.Threading.Tasks;

namespace Chronosystem.Application.Common.Interfaces.Tenancy;

public interface ITenantSettingsProvider
{
    string? GetValue(string tenant, string key);
    Task<string?> GetValueAsync(string tenant, string key, CancellationToken ct = default);
}
