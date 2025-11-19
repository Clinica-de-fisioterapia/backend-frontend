using Chronosystem.Application.Common.Interfaces.Persistence;
using MediatR;

namespace Chronosystem.Application.Features.Services.Commands.DeleteService;

public class DeleteServiceCommandHandler(IServiceRepository repository, IUnitOfWork unitOfWork) 
    : IRequestHandler<DeleteServiceCommand, Unit> // Explicitamos o retorno Unit
{
    public async Task<Unit> Handle(DeleteServiceCommand request, CancellationToken ct)
    {
        // 1. Buscar a entidade
        var service = await repository.GetByIdAsync(request.Id, ct);

        if (service is null)
        {
            throw new KeyNotFoundException($"Serviço com ID {request.Id} não encontrado.");
        }

        // 2. Aplicar Soft Delete
        service.SoftDelete(request.ActorId);
        service.UpdatedBy = request.ActorId;

        // 3. Persistência
        repository.Update(service);
        await unitOfWork.SaveChangesAsync(ct);

        // ✅ Retorno obrigatório do MediatR para comandos void
        return Unit.Value;
    }
}