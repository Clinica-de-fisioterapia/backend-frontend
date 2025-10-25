// ======================================================================================
// ARQUIVO: RefreshTokenService.cs  (VERSÃO AJUSTADA – remove filtro por Tenant no EF)
// CAMADA: Infrastructure / Authentication
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

public class RefreshTokenService : IRefreshTokenService
{
    private readonly ApplicationDbContext _dbContext;

    public RefreshTokenService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RefreshToken> GenerateAsync(Guid userId, string tenant)
    {
        var plainToken = GenerateSecureToken();

        var refreshToken = RefreshToken.Create(userId, tenant, plainToken, expiryDays: 7);

        await _dbContext.Set<RefreshToken>().AddAsync(refreshToken);
        await _dbContext.SaveChangesAsync();

        return refreshToken;
    }

    public async Task<RefreshToken?> ValidateAsync(string token, string tenant)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(tenant))
            return null;

        var tokenHash = ComputeHash(token);

        // 🔧 Mínimo necessário: remover o filtro por Tenant (propriedade NotMapped)
        var storedToken = await _dbContext.Set<RefreshToken>()
            .AsNoTracking()
            .FirstOrDefaultAsync(rt =>
                rt.TokenHash == tokenHash &&
                rt.DeletedAt == null);

        if (storedToken is null || !storedToken.IsValid())
            return null;

        return storedToken;
    }

    public async Task RevokeAsync(Guid tokenId)
    {
        var token = await _dbContext.Set<RefreshToken>().FirstOrDefaultAsync(t => t.Id == tokenId);
        if (token is null)
            return;

        token.Revoke();
        _dbContext.Update(token);
        await _dbContext.SaveChangesAsync();
    }

    private static string GenerateSecureToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }

    private static string ComputeHash(string token)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
