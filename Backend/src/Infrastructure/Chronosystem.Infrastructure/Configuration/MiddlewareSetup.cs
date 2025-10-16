// ======================================================================================
// ARQUIVO: MiddlewareSetup.cs
// CAMADA: Infrastructure / Configuration
// OBJETIVO: Configurar e registrar os middlewares globais do sistema, incluindo
//            autenticação, validação de tenant e tratamento de exceções.
// ======================================================================================

using Chronosystem.Infrastructure.Middleware;
using Microsoft.AspNetCore.Builder;

namespace Chronosystem.Infrastructure.Configuration;

/// <summary>
/// Classe de configuração responsável por registrar os middlewares
/// utilizados pelo pipeline HTTP da aplicação.
/// </summary>
/// <remarks>
/// Este setup centraliza o registro dos middlewares essenciais à segurança:
/// <list type="bullet">
/// <item><description><b>Autenticação JWT</b> – habilita validação de tokens.</description></item>
/// <item><description><b>TenantValidationMiddleware</b> – protege contra acesso cross-tenant.</description></item>
/// <item><description><b>Autorização</b> – aplica regras de papéis e permissões.</description></item>
/// </list>
/// O método <see cref="UseCoreMiddleware"/> deve ser chamado no <c>Program.cs</c>
/// após o registro dos serviços via <c>builder.Services.AddJwtAuthentication()</c>.
/// </remarks>
public static class MiddlewareSetup
{
    /// <summary>
    /// Registra todos os middlewares de segurança e controle de fluxo do sistema.
    /// </summary>
    /// <param name="app">Aplicação ASP.NET Core atual.</param>
    /// <returns>A instância da aplicação configurada.</returns>
    public static IApplicationBuilder UseCoreMiddleware(this IApplicationBuilder app)
    {
        // 1️⃣ Ativa autenticação via JWT (configurada em AuthenticationSetup)
        app.UseAuthentication();

        // 2️⃣ Executa validação cruzada do tenant (JWT vs X-Tenant Header)
        app.UseMiddleware<TenantValidationMiddleware>();

        // 3️⃣ Ativa a autorização padrão (roles, policies etc.)
        app.UseAuthorization();

        // 4️⃣ (Opcional) — Ponto para incluir middlewares adicionais (ex.: logging, auditoria, exceções)
        // app.UseMiddleware<GlobalExceptionMiddleware>();

        return app;
    }
}
