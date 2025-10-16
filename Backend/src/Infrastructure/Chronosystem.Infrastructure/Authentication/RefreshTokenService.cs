// ======================================================================================
// ARQUIVO: RefreshTokenService.cs
// CAMADA: Infrastructure / Authentication
// OBJETIVO: Implementa o serviço responsável por geração, validação e revogação
//           de refresh tokens, garantindo segurança e isolamento multi-tenant.
// ======================================================================================

using Chronosystem.Application.Common.Interfaces.Authentication;
using Chronosystem.Domain.Entities;
using Chronosystem.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Chronosystem.Infrastructure.Authentication;

/// <summary>
/// Serviço responsável pela geração, validação e revogação de refresh tokens,
/// garantindo sessões longas com segurança.
/// </summary>
/// <remarks>
/// Cada token é armazenado de forma criptograficamente segura (SHA256),
/// vinculado a um <see cref="User"/> e a um tenant (schema).
/// 
/// A política padrão de expiração é de 7 dias, podendo ser alterada conforme necessidade.
/// </remarks>
public class RefreshTokenService : IRefreshTokenService
{
    private readonly ApplicationDbContext _dbContext;

    /// <summary>
    /// Inicializa o serviço de refresh tokens.
    /// </summary>
    /// <param name="dbContext">Contexto de banco de dados do tenant atual.</param>
    public RefreshTokenService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // -------------------------------------------------------------------------
    // 🧩 GERAÇÃO DE TOKEN
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public async Task<RefreshToken> GenerateAsync(Guid userId, string tenant)
    {
        // Gera o token bruto (visível apenas ao cliente)
        var plainToken = GenerateSecureToken();

        // Cria a entidade de domínio (gera hash, define expiração e metadados)
        var refreshToken = RefreshToken.Create(userId, tenant, plainToken, expiryDays: 7);

        // Persiste no banco (apenas o hash)
        await _dbContext.Set<RefreshToken>().AddAsync(refreshToken);
        await _dbContext.SaveChangesAsync();

        // Retorna uma cópia segura contendo o token original em memória (não persistido)
        var result = RefreshToken.Create(userId, tenant, plainToken, expiryDays: 7);
        return result;
    }

    // -------------------------------------------------------------------------
    // 🔍 VALIDAÇÃO
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public async Task<RefreshToken?> ValidateAsync(string token, string tenant)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(tenant))
            return null;

        var tokenHash = ComputeHash(token);

        var storedToken = await _dbContext.Set<RefreshToken>()
            .AsNoTracking()
            .FirstOrDefaultAsync(rt =>
                rt.TokenHash == tokenHash &&
                rt.Tenant == tenant.ToLowerInvariant() &&
                rt.DeletedAt == null);

        if (storedToken is null || !storedToken.IsValid())
            return null;

        return storedToken;
    }

    // -------------------------------------------------------------------------
    // 🚫 REVOGAÇÃO
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public async Task RevokeAsync(Guid tokenId)
    {
        var token = await _dbContext.Set<RefreshToken>().FirstOrDefaultAsync(t => t.Id == tokenId);
        if (token is null)
            return;

        token.Revoke();
        _dbContext.Update(token);
        await _dbContext.SaveChangesAsync();
    }

    // -------------------------------------------------------------------------
    // 🔒 MÉTODOS AUXILIARES
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gera uma string segura e aleatória para uso como refresh token.
    /// </summary>
    /// <returns>Token em formato Base64, com 64 bytes de entropia.</returns>
    private static string GenerateSecureToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }

    /// <summary>
    /// Calcula o hash SHA256 de um token em texto simples.
    /// </summary>
    private static string ComputeHash(string token)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
