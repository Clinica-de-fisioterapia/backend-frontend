using Chronosystem.Application.Features.Services.DTOs;
using MediatR;

namespace Chronosystem.Application.Features.Services.Commands.CreateService;

// ✅ Apenas a definição dos dados de entrada
public record CreateServiceCommand(
    string Name, 
    int DurationMinutes, 
    decimal Price, 
    Guid ActorId
) : IRequest<ServiceDto>;