using System;
using System.Text.RegularExpressions;
using Chronosystem.Application.Resources;
using Microsoft.AspNetCore.Http;

namespace Chronosystem.Api.Middleware;

// O middleware agora é mais simples. Ele apenas valida o header.
// A lógica de mudar o schema foi movida para a configuração do DbContext.
public class TenantResolverMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolverMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    private static readonly Regex TenantRegex = new("^[a-z0-9]([a-z0-9-]*[a-z0-9])?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly PathString SignUpPath = new("/api/auth/signup");

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.Equals(SignUpPath, StringComparison.OrdinalIgnoreCase) &&
            HttpMethods.IsPost(context.Request.Method))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue("X-Tenant", out var tenantSubdomainValue))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new { error = Messages.Tenant_Header_Required });
            return;
        }

        var schema = tenantSubdomainValue.ToString();
        if (string.IsNullOrWhiteSpace(schema) || !TenantRegex.IsMatch(schema))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new { error = Messages.Tenant_Header_InvalidFormat });
            return;
        }

        // Apenas continua a requisição. O DbContext cuidará do resto.
        await _next(context);
    }
}
