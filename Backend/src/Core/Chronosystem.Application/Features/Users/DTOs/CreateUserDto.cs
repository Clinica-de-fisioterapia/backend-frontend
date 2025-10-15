// ======================================================================================
// ARQUIVO: CreateUserDto.cs
// CAMADA: Application / Features / Users / DTOs
// OBJETIVO: Objeto de transferência de dados usado para criação de usuários via API.
// ======================================================================================

using Chronosystem.Domain.Entities;

namespace Chronosystem.Application.Features.Users.DTOs;

public class CreateUserDto
{
    /// <summary>
    /// Nome completo do usuário.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// E-mail do usuário (usado para login).
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Senha em texto simples — será convertida em hash no Application Layer.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Papel (role) do usuário no sistema.
    /// </summary>
    public UserRole Role { get; set; } = UserRole.Receptionist;
}
