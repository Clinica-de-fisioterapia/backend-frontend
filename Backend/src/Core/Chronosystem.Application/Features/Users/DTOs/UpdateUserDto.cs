// ======================================================================================
// ARQUIVO: UpdateUserDto.cs
// CAMADA: Application / Features / Users / DTOs
// OBJETIVO: DTO usado para atualização de usuários via API.
//           Agora permite atualização opcional de e-mail e senha, mantendo compatibilidade.
// ======================================================================================

using Chronosystem.Domain.Entities;

namespace Chronosystem.Application.Features.Users.DTOs;

public class UpdateUserDto
{
    /// <summary>
    /// Nome completo atualizado do usuário.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Novo papel (role) do usuário.
    /// </summary>
    public UserRole Role { get; set; }

    /// <summary>
    /// Define se o usuário está ativo.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Novo e-mail (opcional). Caso não seja informado, o e-mail atual permanece.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Nova senha (opcional). Caso não seja informada, a senha atual permanece.
    /// </summary>
    public string? Password { get; set; }
}
