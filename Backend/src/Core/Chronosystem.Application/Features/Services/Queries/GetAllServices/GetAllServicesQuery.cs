using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Application.Features.Services.DTOs;
using MediatR;

namespace Chronosystem.Application.Features.Services.Queries.GetAllServices;

public record GetAllServicesQuery : IRequest<List<ServiceDto>>;

public class GetAllServicesQueryHandler(IServiceRepository repository) 
    : IRequestHandler<GetAllServicesQuery, List<ServiceDto>>
{
    public async Task<List<ServiceDto>> Handle(GetAllServicesQuery request, CancellationToken ct)
    {
        var services = await repository.GetAllAsync(ct);
        return services.Select(s => new ServiceDto(s.Id, s.Name, s.DurationMinutes, s.Price)).ToList();
    }
}