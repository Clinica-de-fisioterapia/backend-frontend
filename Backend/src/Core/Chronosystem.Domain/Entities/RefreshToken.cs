// ======================================================================================
// ARQUIVO: RefreshToken.cs
// CAMADA: Domain / Entities
// OBJETIVO: Define a entidade de domínio responsável pelo controle de refresh tokens,
//           usados para renovar o JWT de acesso de forma segura e auditável.
// ======================================================================================

using Chronosystem.Domain.Common;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Chronosystem.Domain.Entities;

/// <summary>
/// Entidade de domínio que representa um token de atualização (refresh token)
/// emitido para um usuário autenticado.
/// </summary>
/// <remarks>
/// O refresh token permite renovar o token JWT de acesso sem exigir novo login,
/// respeitando o isolamento multi-tenant (via schema ativo).
/// 
/// Cada token:
/// <list type="bullet">
/// <item><description>É único e associado a um usuário.</description></item>
/// <item><description>Expira após um período pré-definido (ex.: 7 dias).</description></item>
/// <item><description>É revogado automaticamente ao ser utilizado ou no logout.</description></item>
/// </list>
/// </remarks>
public class RefreshToken : AuditableEntity
{
    // -------------------------------------------------------------------------
    // 🧱 PROPRIEDADES
    // -------------------------------------------------------------------------

    /// <summary>
    /// Valor criptografado (hash SHA256) do refresh token.
    /// </summary>
    public string TokenHash { get; private set; } = string.Empty;

    /// <summary>
    /// Token bruto retornado ao cliente durante a geração. Não é persistido.
    /// </summary>
    [NotMapped]
    public string PlainToken { get; private set; } = string.Empty;

    /// <summary>
    /// Identificador do usuário ao qual o token pertence.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Tenant (schema) no qual o token foi emitido.
    /// </summary>
    public string Tenant { get; private set; } = string.Empty;

    /// <summary>
    /// Data e hora UTC de expiração do token.
    /// </summary>
    public DateTime ExpiresAtUtc { get; private set; }

    /// <summary>
    /// Indica se o token foi revogado manualmente ou por rotação.
    /// </summary>
    public bool IsRevoked { get; private set; }

    // -------------------------------------------------------------------------
    // 🧩 CONSTRUTORES
    // -------------------------------------------------------------------------

    /// <summary>
    /// Construtor protegido para uso do Entity Framework.
    /// </summary>
    protected RefreshToken() { }

    private RefreshToken(Guid userId, string tenant, string tokenHash, DateTime expiresAtUtc)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Tenant = tenant.Trim().ToLowerInvariant();
        TokenHash = tokenHash;
        ExpiresAtUtc = expiresAtUtc;
        IsRevoked = false;
        CreatedAt = DateTime.UtcNow;
    }

    // -------------------------------------------------------------------------
    // 🏗️ MÉTODOS DE FÁBRICA
    // -------------------------------------------------------------------------

    /// <summary>
    /// Cria uma nova instância de <see cref="RefreshToken"/>, gerando um novo identificador.
    /// </summary>
    /// <param name="userId">Identificador do usuário autenticado.</param>
    /// <param name="tenant">Nome do tenant (schema ativo).</param>
    /// <param name="plainToken">Token bruto gerado antes da hash.</param>
    /// <param name="expiryDays">Dias de validade do token.</param>
    /// <returns>Nova instância de <see cref="RefreshToken"/>.</returns>
    public static RefreshToken Create(Guid userId, string tenant, string plainToken, int expiryDays)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("O ID do usuário é obrigatório.", nameof(userId));

        if (string.IsNullOrWhiteSpace(tenant))
            throw new ArgumentException("O tenant é obrigatório.", nameof(tenant));

        if (string.IsNullOrWhiteSpace(plainToken))
            throw new ArgumentException("O token não pode ser nulo ou vazio.", nameof(plainToken));

        var tokenHash = ComputeHash(plainToken);
        var expiresAtUtc = DateTime.UtcNow.AddDays(expiryDays);

        var refreshToken = new RefreshToken(userId, tenant, tokenHash, expiresAtUtc)
        {
            PlainToken = plainToken
        };

        return refreshToken;
    }

    // -------------------------------------------------------------------------
    // 🔒 MÉTODOS DE DOMÍNIO
    // -------------------------------------------------------------------------

    /// <summary>
    /// Revoga o token, impedindo seu uso futuro.
    /// </summary>
    public void Revoke()
    {
        IsRevoked = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Verifica se o token ainda é válido (não expirado e não revogado).
    /// </summary>
    /// <returns><c>true</c> se o token estiver ativo e válido.</returns>
    public bool IsValid()
    {
        return !IsRevoked && DeletedAt is null && DateTime.UtcNow < ExpiresAtUtc;
    }

    /// <summary>
    /// Calcula o hash SHA256 do valor bruto do token.
    /// </summary>
    /// <param name="token">Valor do token em texto simples.</param>
    /// <returns>Hash criptográfico em formato Base64.</returns>
    private static string ComputeHash(string token)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(token);
        var hashBytes = sha.ComputeHash(bytes);
        return Convert.ToBase64String(hashBytes);
    }
}
