// ======================================================================================
// ARQUIVO: IJwtTokenGenerator.cs
// CAMADA: Application / Common / Interfaces / Authentication
// OBJETIVO: Contrato para geração de JWTs de acesso para usuários do tenant atual.
// ======================================================================================

using Chronosystem.Domain.Entities;
using System;
using System.Collections.Generic;

namespace Chronosystem.Application.Common.Interfaces.Authentication;

/// <summary>
/// Define o contrato para geração de tokens JWT de acesso,
/// encapsulando a criação com base no usuário autenticado e no tenant atual.
/// </summary>
/// <remarks>
/// A implementação concreta deve:
/// <list type="bullet">
/// <item><description>Emitir o token assinado com chave simétrica segura.</description></item>
/// <item><description>Incluir claims obrigatórias: <c>sub</c> (user id), <c>role</c>, <c>tenant</c>.</description></item>
/// <item><description>Definir tempos de expiração curtos (ex.: 15 minutos) para o access token.</description></item>
/// </list>
/// </remarks>
public interface IJwtTokenGenerator
{
    /// <summary>
    /// Gera um token de acesso (JWT) para o usuário informado, no contexto do tenant especificado.
    /// </summary>
    /// <param name="user">Usuário autenticado.</param>
    /// <param name="tenant">Identificador do tenant (schema), por exemplo: <c>empresa_teste</c>.</param>
    /// <param name="customClaims">
    /// Claims adicionais a serem incluídas no token (opcional).  
    /// Se fornecidas, não devem sobrescrever as claims padrão (<c>sub</c>, <c>role</c>, <c>tenant</c>).
    /// </param>
    /// <returns>Uma string contendo o JWT assinado.</returns>
    string GenerateAccessToken(User user, string tenant, IDictionary<string, string>? customClaims = null);

    /// <summary>
    /// Obtém a data/hora de expiração prevista para o token de acesso que será criado.
    /// </summary>
    /// <returns><see cref="DateTime"/> em UTC representando a expiração.</returns>
    DateTime GetAccessTokenExpiryUtc();
}
