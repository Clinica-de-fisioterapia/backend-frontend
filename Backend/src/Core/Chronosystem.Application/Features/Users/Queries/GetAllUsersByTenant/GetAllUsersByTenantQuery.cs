// ======================================================================================
// ARQUIVO: GetAllUsersQuery.cs
// CAMADA: Application / Features / Users / Queries / GetAllUsers
// OBJETIVO: Define o Query responsável por retornar todos os usuários ativos
//            dentro do tenant atual (multi-tenant por schema).
// ======================================================================================

using Chronosystem.Domain.Entities;
using MediatR;

namespace Chronosystem.Application.Features.Users.Queries.GetAllUsers;

/// <summary>
/// Representa a consulta para obter todos os usuários do schema atual.
/// </summary>
public record GetAllUsersQuery() : IRequest<IEnumerable<User>>;
