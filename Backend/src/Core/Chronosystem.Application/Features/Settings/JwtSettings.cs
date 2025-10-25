// ======================================================================================
// ARQUIVO: JwtSettings.cs
// CAMADA: Application / Common / Settings
// OBJETIVO: Define as configurações de segurança e parâmetros utilizados
//            para geração e validação de tokens JWT.
// ======================================================================================

using System;

namespace Chronosystem.Application.Common.Settings;

/// <summary>
/// Representa as configurações necessárias para emissão e validação de tokens JWT.
/// </summary>
/// <remarks>
/// Estas configurações são carregadas a partir do arquivo de configuração
/// (<c>appsettings.json</c> ou variáveis de ambiente) e utilizadas pela
/// implementação concreta de <c>IJwtTokenGenerator</c>.
/// 
/// O tempo de expiração deve ser curto para tokens de acesso (ex.: 15 minutos),
/// sendo o refresh token tratado separadamente.
/// </remarks>
public class JwtSettings
{
    /// <summary>
    /// Chave secreta simétrica utilizada para assinar os tokens.
    /// Deve ter entropia mínima de 256 bits.
    /// </summary>
    public string SecretKey { get; init; } = string.Empty;

    /// <summary>
    /// Emissor (Issuer) do token JWT.
    /// Normalmente representa o nome ou domínio da API.
    /// </summary>
    public string Issuer { get; init; } = "Chronosystem.Api";

    /// <summary>
    /// Público (Audience) que deve aceitar o token.
    /// Usualmente o mesmo valor do Issuer para aplicações monolíticas.
    /// </summary>
    public string Audience { get; init; } = "Chronosystem.Client";

    /// <summary>
    /// Tempo de expiração do token de acesso em minutos.
    /// Recomenda-se entre 10 e 30 minutos.
    /// </summary>
    public int AccessTokenExpiryMinutes { get; init; } = 15;

    /// <summary>
    /// Obtém o tempo de expiração em UTC com base na configuração atual.
    /// </summary>
    /// <returns><see cref="DateTime"/> representando a data e hora de expiração.</returns>
    public DateTime GetAccessTokenExpiryUtc() =>
        DateTime.UtcNow.AddMinutes(AccessTokenExpiryMinutes);
}
