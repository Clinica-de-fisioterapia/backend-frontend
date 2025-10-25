// ======================================================================================
// ARQUIVO: GetAllUsersQueryValidator.cs
// CAMADA: Application / Features / Users / Queries / GetAllUsers
// OBJETIVO: Define as regras de validação para o query GetAllUsersQuery.
// ======================================================================================

using FluentValidation;

namespace Chronosystem.Application.Features.Users.Queries.GetAllUsers;

/// <summary>
/// Validador para a query <see cref="GetAllUsersQuery"/>.
/// </summary>
/// <remarks>
/// Mesmo sem parâmetros, este validador mantém a padronização do pipeline CQRS,
/// permitindo futuras extensões (ex.: paginação, filtros, roles).
/// </remarks>
public class GetAllUsersQueryValidator : AbstractValidator<GetAllUsersQuery>
{
    /// <summary>
    /// Inicializa o validador da query de listagem de usuários.
    /// </summary>
    public GetAllUsersQueryValidator()
    {
        // Nenhuma regra necessária no momento.
        // Estrutura mantida para consistência e extensibilidade.
    }
}
