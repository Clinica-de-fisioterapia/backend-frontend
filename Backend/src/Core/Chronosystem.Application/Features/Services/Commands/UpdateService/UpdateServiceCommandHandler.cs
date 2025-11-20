using Chronosystem.Application.Common.Interfaces.Persistence;
using MediatR;

namespace Chronosystem.Application.Features.Services.Commands.UpdateService;

public class UpdateServiceCommandHandler(IServiceRepository repository, IUnitOfWork unitOfWork) 
    : IRequestHandler<UpdateServiceCommand, Unit> // Explicitamos o retorno Unit
{
    public async Task<Unit> Handle(UpdateServiceCommand request, CancellationToken ct)
    {
        // 1. Buscar a entidade
        var service = await repository.GetByIdAsync(request.Id, ct);

        if (service is null)
        {
            throw new KeyNotFoundException($"Serviço com ID {request.Id} não encontrado.");
        }

        // 2. Atualizar estado
        service.UpdateName(request.Name);
        service.UpdateDuration(request.DurationMinutes);
        service.UpdatePrice(request.Price);
        
        // 3. Auditoria
        service.UpdatedBy = request.ActorId;

        // 4. Persistência
        repository.Update(service);
        await unitOfWork.SaveChangesAsync(ct);

        // ✅ Retorno obrigatório do MediatR para comandos void
        return Unit.Value;
    }
}