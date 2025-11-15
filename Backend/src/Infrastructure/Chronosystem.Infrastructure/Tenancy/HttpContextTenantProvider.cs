using Chronosystem.Application.Common.Interfaces.Tenancy;
using Chronosystem.Infrastructure.Common;
using Chronosystem.Infrastructure.Tenancy.Exceptions;
using Microsoft.AspNetCore.Http;

namespace Chronosystem.Infrastructure.Tenancy;

public sealed class HttpContextTenantProvider : ICurrentTenantProvider
{
    private const string HeaderName = "X-Tenant";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextTenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetCurrentTenant()
    {
        var context = _httpContextAccessor.HttpContext
            ?? throw new TenantHeaderMissingException("HTTP context is not available.");

        if (!context.Request.Headers.TryGetValue(HeaderName, out var values))
            throw new TenantHeaderMissingException("X-Tenant header is required.");

        var normalizedTenant = TenancyUtils.NormalizeTenant(values.ToString());
        if (normalizedTenant is null)
            throw new TenantHeaderMissingException("X-Tenant header is empty.");

        return normalizedTenant;
    }
}
