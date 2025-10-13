// ======================================================================================
// ARQUIVO: CreateUnitCommand.cs
// CAMADA: Application / UseCases / Units / Commands / CreateUnit
// OBJETIVO: Define o comando (request) para criação de uma nova unidade (Unit).
//            Utiliza o padrão CQRS com MediatR para comunicação entre a API e o Handler.
// ======================================================================================

using Chronosystem.Application.Features.Units.DTOs;
using MediatR;

namespace Chronosystem.Application.UseCases.Units.Commands.CreateUnit;

/// <summary>
/// Representa o comando de criação de uma unidade.
/// </summary>
public record CreateUnitCommand(string Name, Guid UserId) : IRequest<UnitDto>;
