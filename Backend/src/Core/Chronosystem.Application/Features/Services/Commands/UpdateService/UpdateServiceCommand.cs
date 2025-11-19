using MediatR;

namespace Chronosystem.Application.Features.Services.Commands.UpdateService;

public record UpdateServiceCommand(
    Guid Id, 
    string Name, 
    int DurationMinutes, 
    decimal Price, 
    Guid ActorId
) : IRequest;