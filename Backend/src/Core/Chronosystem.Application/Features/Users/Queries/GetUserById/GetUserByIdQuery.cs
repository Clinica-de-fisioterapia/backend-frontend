// ======================================================================================
// ARQUIVO: GetUserByIdQuery.cs
// CAMADA: Application / Features / Users / Queries / GetUserById
// OBJETIVO: Define o Query responsável por retornar um usuário específico por ID.
// ======================================================================================

using Chronosystem.Application.Features.Users.DTOs;
using MediatR;
using System;

namespace Chronosystem.Application.Features.Users.Queries.GetUserById;

/// <summary>
/// Representa a consulta que busca um usuário específico com base em seu identificador único.
/// </summary>
/// <remarks>
/// A query é processada pelo <see cref="Handlers.GetUserByIdQueryHandler"/>  
/// e validada por <see cref="Validators.GetUserByIdQueryValidator"/>.  
/// Retorna um objeto <see cref="UserDto"/> caso encontrado, ou <c>null</c> se não existir.
/// </remarks>
public record GetUserByIdQuery(Guid Id) : IRequest<UserDto?>;
