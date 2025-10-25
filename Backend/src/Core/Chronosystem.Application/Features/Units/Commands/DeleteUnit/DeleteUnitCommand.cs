// ======================================================================================
// ARQUIVO: DeleteUnitCommand.cs
// CAMADA: Application / UseCases / Units / Commands / DeleteUnit
// OBJETIVO: Define o comando de exclusão lógica de uma unidade existente.
//            Utiliza o padrão CQRS com MediatR (versão gratuita).
// ======================================================================================

using MediatR;

namespace Chronosystem.Application.UseCases.Units.Commands.DeleteUnit;

/// <summary>
/// Representa o comando para realizar o soft delete de uma unidade.
/// </summary>
public record DeleteUnitCommand(Guid Id, Guid UserId) : IRequest<Unit>;
