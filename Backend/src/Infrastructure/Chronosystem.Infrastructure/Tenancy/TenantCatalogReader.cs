using System;
using System.Threading;
using System.Threading.Tasks;
using Chronosystem.Application.Common.Interfaces.Tenancy;
using Chronosystem.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Chronosystem.Infrastructure.Tenancy;

/// <summary>
/// Provides read-only access to the tenants registry stored in the public schema.
/// </summary>
public sealed class TenantCatalogReader : ITenantCatalogReader
{
    private readonly ApplicationDbContext _dbContext;

    public TenantCatalogReader(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> SubdomainExistsAsync(string subdomain, CancellationToken cancellationToken)
    {
        var candidate = Normalize(subdomain);
        if (string.IsNullOrWhiteSpace(candidate))
        {
            return false;
        }

        // IMPORTANT: no trailing semicolon inside the EXISTS subquery and add alias "Value"
        return await _dbContext.Database
            .SqlQueryRaw<bool>(@"
                SELECT EXISTS (
                    SELECT 1
                    FROM public.tenants
                    WHERE deleted_at IS NULL
                      AND LOWER(slug) = LOWER({0})
                ) AS ""Value""", candidate)
            .SingleAsync(cancellationToken);
    }

    private static string Normalize(string? value)
        => string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim().ToLowerInvariant();
}
