// ======================================================================================
// ARQUIVO: GetUserByIdQuery.cs
// CAMADA: Application / Features / Users / Queries / GetUserById
// OBJETIVO: Define o Query responsável por retornar um usuário específico por ID.
// ======================================================================================

using Chronosystem.Domain.Entities;
using MediatR;

namespace Chronosystem.Application.Features.Users.Queries.GetUserById;

/// <summary>
/// Representa a consulta para obter os dados de um usuário específico.
/// </summary>
public record GetUserByIdQuery(Guid Id) : IRequest<User?>;
