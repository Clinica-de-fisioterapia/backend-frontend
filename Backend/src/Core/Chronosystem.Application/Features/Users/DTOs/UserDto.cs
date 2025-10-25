// ======================================================================================
// ARQUIVO: UserDto.cs
// CAMADA: Application / Features / Users / DTOs
// OBJETIVO: Representa os dados de saída (output) de um usuário retornado pela API.
// ======================================================================================

using System;

namespace Chronosystem.Application.Features.Users.DTOs;

/// <summary>
/// DTO de saída que representa um usuário retornado pela API.
/// </summary>
public sealed class UserDto
{
    /// <summary>
    /// Identificador do usuário.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Nome completo.
    /// </summary>
    public string FullName { get; init; } = string.Empty;

    /// <summary>
    /// Endereço de e-mail.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Papel (role) do usuário.
    /// </summary>
    public string Role { get; init; } = string.Empty;

    /// <summary>
    /// Indica se o usuário está ativo.
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Data e hora de criação (UTC).
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Data e hora da última atualização (UTC).
    /// </summary>
    public DateTime UpdatedAt { get; init; }
}
