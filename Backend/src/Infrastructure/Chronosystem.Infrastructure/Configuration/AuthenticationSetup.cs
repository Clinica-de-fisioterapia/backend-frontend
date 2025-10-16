// ======================================================================================
// ARQUIVO: AuthenticationSetup.cs
// CAMADA: Infrastructure / Configuration
// OBJETIVO: Configura a autenticação JWT para a aplicação, vinculando as
//            configurações de JwtSettings e o serviço JwtTokenGenerator.
// ======================================================================================

using Chronosystem.Application.Common.Interfaces.Authentication;
using Chronosystem.Application.Common.Settings;
using Chronosystem.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Chronosystem.Infrastructure.Configuration;

/// <summary>
/// Classe estática responsável por registrar e configurar os serviços de autenticação JWT.
/// </summary>
/// <remarks>
/// Este setup vincula as configurações definidas em <see cref="JwtSettings"/> e ativa
/// o middleware de autenticação JWT no pipeline da API.  
/// 
/// As validações implementadas garantem:
/// <list type="bullet">
/// <item><description>Assinatura válida com chave simétrica.</description></item>
/// <item><description>Validação de emissor (<c>Issuer</c>) e audiência (<c>Audience</c>).</description></item>
/// <item><description>Expiração obrigatória e rejeição de tokens expirados.</description></item>
/// </list>
/// </remarks>
public static class AuthenticationSetup
{
    /// <summary>
    /// Registra e configura o JWT Authentication no container de injeção de dependências.
    /// </summary>
    /// <param name="services">Coleção de serviços da aplicação.</param>
    /// <param name="configuration">Configuração de ambiente (<c>appsettings.json</c>).</param>
    /// <returns>A coleção de serviços configurada.</returns>
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 1️⃣ Lê as configurações tipadas do appsettings
        var jwtSection = configuration.GetSection("JwtSettings");
        services.Configure<JwtSettings>(jwtSection);

        var jwtSettings = jwtSection.Get<JwtSettings>()
            ?? throw new InvalidOperationException("As configurações de JWT não foram encontradas.");

        // 2️⃣ Registra o serviço de geração de tokens
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        // 3️⃣ Configura o middleware de autenticação JWT
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey));

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = key,
                    ClockSkew = TimeSpan.Zero // 🔒 evita tolerância de expiração
                };

                // 🔐 Permite leitura do token do header Authorization: Bearer {token}
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = ctx =>
                    {
                        Console.WriteLine($"[AUTH ERROR] {ctx.Exception.Message}");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = ctx =>
                    {
                        var tenantClaim = ctx.Principal?.FindFirst("tenant")?.Value;
                        if (string.IsNullOrWhiteSpace(tenantClaim))
                        {
                            ctx.Fail("O token não contém a claim 'tenant'.");
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }
}
