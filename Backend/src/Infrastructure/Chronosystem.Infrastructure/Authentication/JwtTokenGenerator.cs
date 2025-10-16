// ======================================================================================
// ARQUIVO: JwtTokenGenerator.cs
// CAMADA: Infrastructure / Authentication
// OBJETIVO: Implementa a geração de tokens JWT para autenticação de usuários,
//           garantindo segurança e isolamento multi-tenant por schema.
// ======================================================================================

using Chronosystem.Application.Common.Interfaces.Authentication;
using Chronosystem.Application.Common.Settings;
using Chronosystem.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Chronosystem.Infrastructure.Authentication;

/// <summary>
/// Implementação concreta do serviço de geração de tokens JWT.
/// </summary>
/// <remarks>
/// Este gerador cria tokens de acesso assinados com HMAC-SHA256,  
/// incluindo as claims essenciais para autenticação multi-tenant:
/// <list type="bullet">
/// <item><description><c>sub</c> → ID do usuário (<see cref="Guid"/>).</description></item>
/// <item><description><c>role</c> → Papel do usuário (<see cref="UserRole"/>).</description></item>
/// <item><description><c>tenant</c> → Schema atual, ex.: <c>empresa_teste</c>.</description></item>
/// </list>
/// O token é válido apenas durante o período configurado em <see cref="JwtSettings"/>.
/// </remarks>
public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtSettings _settings;

    /// <summary>
    /// Inicializa o gerador de tokens JWT com as configurações definidas.
    /// </summary>
    /// <param name="jwtOptions">Opções de configuração carregadas de <c>appsettings.json</c>.</param>
    public JwtTokenGenerator(IOptions<JwtSettings> jwtOptions)
    {
        _settings = jwtOptions.Value;
    }

    /// <summary>
    /// Gera um token JWT de acesso para o usuário informado.
    /// </summary>
    /// <param name="user">Usuário autenticado.</param>
    /// <param name="tenant">Tenant (schema) ativo, ex.: <c>empresa_teste</c>.</param>
    /// <param name="customClaims">Claims adicionais opcionais.</param>
    /// <returns>Token JWT assinado como <see cref="string"/>.</returns>
    public string GenerateAccessToken(User user, string tenant, IDictionary<string, string>? customClaims = null)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("tenant", tenant),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Adiciona claims personalizadas, se houver
        if (customClaims != null)
        {
            foreach (var kvp in customClaims)
            {
                if (!claims.Exists(c => c.Type == kvp.Key)) // evita sobrescrever as principais
                    claims.Add(new Claim(kvp.Key, kvp.Value));
            }
        }

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: _settings.GetAccessTokenExpiryUtc(),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Obtém a data/hora de expiração do token de acesso, em UTC.
    /// </summary>
    public DateTime GetAccessTokenExpiryUtc() => _settings.GetAccessTokenExpiryUtc();
}
