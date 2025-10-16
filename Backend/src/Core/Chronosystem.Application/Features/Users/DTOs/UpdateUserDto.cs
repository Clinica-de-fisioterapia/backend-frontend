// ======================================================================================
// ARQUIVO: UpdateUserDto.cs
// CAMADA: Application / Features / Users / DTOs
// OBJETIVO: Representa os dados de entrada para atualização de um usuário existente.
// ======================================================================================

using Chronosystem.Domain.Enums;
using System;

namespace Chronosystem.Application.Features.Users.DTOs;

/// <summary>
/// DTO utilizado na atualização de um usuário existente.
/// </summary>
public sealed class UpdateUserDto
{
    /// <summary>
    /// Identificador do usuário.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Nome completo atualizado.
    /// </summary>
    public string FullName { get; init; } = string.Empty;

    /// <summary>
    /// Novo e-mail (caso alterado).
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Nova senha (opcional). Se preenchida, será reprocessada como hash.
    /// </summary>
    public string? Password { get; init; }

    /// <summary>
    /// Novo papel do usuário.
    /// </summary>
    public UserRole Role { get; init; }

    /// <summary>
    /// Indica se o usuário está ativo.
    /// </summary>
    public bool IsActive { get; init; }
}
