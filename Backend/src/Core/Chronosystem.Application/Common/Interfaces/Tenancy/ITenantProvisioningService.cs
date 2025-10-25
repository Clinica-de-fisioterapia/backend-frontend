using System;
using System.Threading;
using System.Threading.Tasks;

namespace Chronosystem.Application.Common.Interfaces.Tenancy;

public interface ITenantProvisioningService
{
    Task<Guid> ProvisionAsync(TenantProvisionRequest request, CancellationToken ct);
}

public sealed record TenantProvisionRequest(
    string CompanyName,
    string Subdomain,
    string AdminFullName,
    string AdminEmail,
    string AdminPasswordPlain
);
