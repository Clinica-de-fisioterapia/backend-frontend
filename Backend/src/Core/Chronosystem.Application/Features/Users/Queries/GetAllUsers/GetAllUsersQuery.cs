// ======================================================================================
// ARQUIVO: GetAllUsersQuery.cs
// CAMADA: Application / Features / Users / Queries / GetAllUsers
// OBJETIVO: Define o Query responsável por retornar todos os usuários ativos
//            dentro do tenant atual (multi-tenant por schema).
// ======================================================================================

using Chronosystem.Application.Features.Users.DTOs;
using MediatR;
using System.Collections.Generic;

namespace Chronosystem.Application.Features.Users.Queries.GetAllUsers;

/// <summary>
/// Representa a consulta que solicita todos os usuários ativos
/// dentro do schema (tenant) atual.
/// </summary>
/// <remarks>
/// Esta query é manipulada pelo <see cref="Handlers.GetAllUsersQueryHandler"/>  
/// e validada por <see cref="Validators.GetAllUsersQueryValidator"/>.  
/// O retorno é uma coleção de objetos <see cref="UserDto"/> simplificados.
/// </remarks>
public record GetAllUsersQuery() : IRequest<IEnumerable<UserDto>>;
