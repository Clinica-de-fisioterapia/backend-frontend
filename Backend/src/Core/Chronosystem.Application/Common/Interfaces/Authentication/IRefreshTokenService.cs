// ======================================================================================
// ARQUIVO: IRefreshTokenService.cs
// CAMADA: Application / Common / Interfaces / Authentication
// OBJETIVO: Define o contrato para geração, validação e rotação segura de refresh tokens.
// ======================================================================================

using Chronosystem.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace Chronosystem.Application.Common.Interfaces.Authentication;

/// <summary>
/// Define as operações necessárias para gerenciamento seguro de refresh tokens.
/// </summary>
/// <remarks>
/// O refresh token é usado para obter um novo access token (JWT) sem necessidade de reautenticação completa,
/// mantendo a sessão ativa com segurança.
/// 
/// Regras obrigatórias:
/// <list type="bullet">
/// <item><description>Cada refresh token é único e vinculado a um usuário e tenant.</description></item>
/// <item><description>Tokens expiram e são invalidados após uso ou logout.</description></item>
/// <item><description>Os tokens são armazenados de forma segura e verificados antes da renovação.</description></item>
/// </list>
/// </remarks>
public interface IRefreshTokenService
{
    /// <summary>
    /// Gera e persiste um novo refresh token associado ao usuário e tenant.
    /// </summary>
    /// <param name="userId">Identificador do usuário autenticado.</param>
    /// <param name="tenant">Tenant (schema) ativo no momento da autenticação.</param>
    /// <returns>Entidade <see cref="RefreshToken"/> recém-criada.</returns>
    Task<RefreshToken> GenerateAsync(Guid userId, string tenant);

    /// <summary>
    /// Valida um refresh token recebido, verificando expiração e integridade.
    /// </summary>
    /// <param name="token">Valor bruto do refresh token recebido do cliente.</param>
    /// <param name="tenant">Tenant associado à requisição atual.</param>
    /// <returns>Entidade <see cref="RefreshToken"/> válida, ou <c>null</c> se inválida.</returns>
    Task<RefreshToken?> ValidateAsync(string token, string tenant);

    /// <summary>
    /// Invalida (revoga) um refresh token específico após uso ou logout.
    /// </summary>
    /// <param name="tokenId">Identificador único do refresh token.</param>
    Task RevokeAsync(Guid tokenId);
}
