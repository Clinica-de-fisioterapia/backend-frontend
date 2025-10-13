// ======================================================================================
// ARQUIVO: GetAllUnitsQuery.cs
// CAMADA: Application / UseCases / Units / Queries / GetAllUnits
// OBJETIVO: Define o comando (query) para obter todas as unidades ativas (não deletadas).
//            Utiliza o padrão CQRS com MediatR (versão gratuita).
// ======================================================================================

using Chronosystem.Application.Features.Units.DTOs;
using MediatR;

namespace Chronosystem.Application.UseCases.Units.Queries.GetAllUnits;

/// <summary>
/// Representa a query para listar todas as unidades ativas (não deletadas).
/// </summary>
public record GetAllUnitsQuery : IRequest<IEnumerable<UnitDto>>;
