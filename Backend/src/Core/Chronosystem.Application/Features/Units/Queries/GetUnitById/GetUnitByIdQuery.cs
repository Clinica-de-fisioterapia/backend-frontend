// ======================================================================================
// ARQUIVO: GetUnitByIdQuery.cs
// CAMADA: Application / UseCases / Units / Queries / GetUnitById
// OBJETIVO: Define a query para buscar uma unidade específica pelo seu ID.
//            Utiliza o padrão CQRS com MediatR.
// ======================================================================================

using Chronosystem.Application.Features.Units.DTOs;
using MediatR;

namespace Chronosystem.Application.UseCases.Units.Queries.GetUnitById;

/// <summary>
/// Representa a query para obter uma unidade pelo seu identificador.
/// </summary>
public record GetUnitByIdQuery(Guid Id) : IRequest<UnitDto>;
