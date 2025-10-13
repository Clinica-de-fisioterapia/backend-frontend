using Chronosystem.Application.Common.Interfaces.Persistence;
using MediatR;

namespace Chronosystem.Application.Features.Users.Commands.DeleteUser;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Unit>
{
    private readonly IUserRepository _userRepository;

    public DeleteUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        // Busca o usuário pelo Tenant e ID
        var user = await _userRepository.GetByIdAsync(request.UserId, request.TenantId);
        if (user is null)
            throw new KeyNotFoundException($"Usuário com ID {request.UserId} não encontrado.");

        // Soft delete (ajuste conforme seu domínio)
        user.SoftDelete();

        // Persistência
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        return Unit.Value;
    }
}
