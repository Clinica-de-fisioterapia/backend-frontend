using MediatR;

namespace Chronosystem.Application.Features.Users.Commands.DeleteUser;

public record DeleteUserCommand(Guid UserId, Guid TenantId) : IRequest<Unit>;
