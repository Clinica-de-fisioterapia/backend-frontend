using MediatR;

namespace Chronosystem.Application.Features.Users.Commands.DeleteUser;

public sealed record DeleteUserCommand(Guid UserId, Guid DeletedByUserId) : IRequest;
