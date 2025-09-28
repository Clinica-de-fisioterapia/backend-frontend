// Features/Units/Commands/CreateUnit/CreateUnitCommand.cs
using Chronosystem.Application.Features.Units.DTOs;
using MediatR;

namespace Chronosystem.Application.Features.Units.Commands.CreateUnit;

// O TenantId foi removido.
// Nota: Em um sistema com autenticação, o UserId viria do token do usuário (claims)
// e não precisaria ser passado no corpo da requisição.
public record CreateUnitCommand(string Name, Guid UserId) : IRequest<UnitDto>;