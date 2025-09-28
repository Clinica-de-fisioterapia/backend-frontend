using System.Text.RegularExpressions;

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

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("X-Tenant", out var tenantSubdomainValue))
        {
            var schema = tenantSubdomainValue.ToString();

            if (string.IsNullOrWhiteSpace(schema) || !Regex.IsMatch(schema, @"^[a-zA-Z0-9_]+$"))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Invalid tenant format in X-Tenant header.");
                return;
            }
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("X-Tenant header is required.");
            return;
        }

        // Apenas continua a requisição. O DbContext cuidará do resto.
        await _next(context);
    }
}