// ======================================================================================
// ARQUIVO: UpdateUnitCommand.cs
// CAMADA: Application / UseCases / Units / Commands / UpdateUnit
// OBJETIVO: Define o comando de atualização de uma unidade existente.
//            Utiliza CQRS com MediatR para envio e processamento do comando.
// ======================================================================================

using Chronosystem.Application.Features.Units.DTOs;
using MediatR;

namespace Chronosystem.Application.UseCases.Units.Commands.UpdateUnit;

/// <summary>
/// Representa o comando de atualização de uma unidade existente.
/// </summary>
public record UpdateUnitCommand(Guid Id, string Name, Guid UserId) : IRequest<UnitDto>;
