using System.Threading;
using System.Threading.Tasks;

namespace Chronosystem.Application.Common.Interfaces.Tenancy;

/// <summary>
/// Provides read-only access to the tenant catalog stored in the public schema.
/// </summary>
public interface ITenantCatalogReader
{
    Task<bool> SubdomainExistsAsync(string subdomain, CancellationToken cancellationToken);
}
