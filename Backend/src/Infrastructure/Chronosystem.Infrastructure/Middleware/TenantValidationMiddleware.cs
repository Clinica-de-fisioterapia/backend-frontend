// ======================================================================================
// ARQUIVO: TenantValidationMiddleware.cs
// CAMADA: Infrastructure / Middleware
// OBJETIVO: Middleware responsável por validar a coerência entre o tenant informado
//           no cabeçalho HTTP e o tenant presente no token JWT.
// ======================================================================================

using Chronosystem.Application.Resources;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System;

namespace Chronosystem.Infrastructure.Middleware;

/// <summary>
/// Middleware responsável por garantir que o tenant do cabeçalho HTTP (X-Tenant)
/// coincide com o tenant contido nas claims do JWT.
/// </summary>
/// <remarks>
/// Esse middleware atua após a autenticação JWT, interceptando todas as requisições
/// autenticadas e comparando os valores para evitar ataques cross-tenant.
///
/// Se houver discrepância entre o header e o token, o acesso é negado imediatamente
/// com HTTP 403 (Forbidden).
/// </remarks>
public class TenantValidationMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly PathString SignUpPath = new("/api/auth/signup");

    /// <summary>
    /// Inicializa o middleware de validação de tenant.
    /// </summary>
    /// <param name="next">Delegate do próximo middleware no pipeline.</param>
    public TenantValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Executa a validação entre o tenant do token JWT e o tenant do cabeçalho.
    /// </summary>
    /// <param name="context">Contexto HTTP atual.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.Equals(SignUpPath, StringComparison.OrdinalIgnoreCase) &&
            HttpMethods.IsPost(context.Request.Method))
        {
            await _next(context);
            return;
        }

        // 1️⃣ Se não houver autenticação, apenas segue
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            await _next(context);
            return;
        }

        // 2️⃣ Extrai tenant do header HTTP
        var headerTenant = context.Request.Headers["X-Tenant"].ToString().Trim();

        // 3️⃣ Extrai tenant da claim do token
        var tokenTenant = context.User.Claims.FirstOrDefault(c => c.Type == "tenant")?.Value?.Trim();

        // 4️⃣ Validação de consistência
        if (string.IsNullOrWhiteSpace(headerTenant))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = Messages.Tenant_Header_Required });
            return;
        }

        if (string.IsNullOrWhiteSpace(tokenTenant))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = Messages.Tenant_Header_MissingClaim });
            return;
        }

        if (!string.Equals(headerTenant, tokenTenant, StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new { error = Messages.Tenant_Header_Mismatch });
            return;
        }

        // 5️⃣ Tudo OK — segue o fluxo
        await _next(context);
    }
}
