using MediatR;

namespace Chronosystem.Application.Features.Services.Commands.DeleteService;

public record DeleteServiceCommand(Guid Id, Guid ActorId) : IRequest;