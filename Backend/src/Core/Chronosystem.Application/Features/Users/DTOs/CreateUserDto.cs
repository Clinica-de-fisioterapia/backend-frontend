// ======================================================================================
// ARQUIVO: CreateUserDto.cs
// CAMADA: Application / Features / Users / DTOs
// OBJETIVO: Representa os dados de entrada para criação de um novo usuário.
// ======================================================================================

using Chronosystem.Domain.Enums;
using System;

namespace Chronosystem.Application.Features.Users.DTOs;

/// <summary>
/// DTO utilizado na criação de um novo usuário.
/// </summary>
public sealed class CreateUserDto
{
    /// <summary>
    /// Nome completo do usuário.
    /// </summary>
    public string FullName { get; init; } = string.Empty;

    /// <summary>
    /// Endereço de e-mail único do usuário.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Senha em texto simples (será criptografada pelo backend antes de persistir).
    /// </summary>
    public string Password { get; init; } = string.Empty;

    /// <summary>
    /// Papel (role) atribuído ao usuário.
    /// </summary>
    public UserRole Role { get; init; }
}
