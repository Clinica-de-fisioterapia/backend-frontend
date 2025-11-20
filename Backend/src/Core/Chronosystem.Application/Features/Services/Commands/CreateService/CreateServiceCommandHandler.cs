using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Application.Features.Services.DTOs;
using Chronosystem.Domain.Entities;
using MediatR;

namespace Chronosystem.Application.Features.Services.Commands.CreateService;

// ✅ Apenas a lógica de execução
public class CreateServiceCommandHandler(IServiceRepository repository) 
    : IRequestHandler<CreateServiceCommand, ServiceDto>
{
    public async Task<ServiceDto> Handle(CreateServiceCommand request, CancellationToken ct)
    {
        // 1. Validação de regra de negócio
        if (await repository.ExistsByNameAsync(request.Name, ct))
            throw new InvalidOperationException("Já existe um serviço com este nome.");

        // 2. Criação da Entidade
        // Usa o método Factory com 3 argumentos (conforme ajustamos na entidade Service)
        var service = Service.Create(request.Name, request.DurationMinutes, request.Price);

        // 3. Auditoria Manual (Preenchimento das propriedades da base)
        service.CreatedBy = request.ActorId;
        service.UpdatedBy = request.ActorId;

        // 4. Persistência
        await repository.AddAsync(service, ct);
        await repository.SaveChangesAsync(ct);

        return new ServiceDto(service.Id, service.Name, service.DurationMinutes, service.Price);
    }
}