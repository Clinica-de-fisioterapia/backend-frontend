using MediatR;

namespace Chronosystem.Application.Features.Units.Commands.UpdateUnit;

// O comando agora carrega o ID da unidade a ser atualizada e os novos dados.
public record UpdateUnitCommand(Guid UnitId, string Name, Guid UserId) : IRequest;