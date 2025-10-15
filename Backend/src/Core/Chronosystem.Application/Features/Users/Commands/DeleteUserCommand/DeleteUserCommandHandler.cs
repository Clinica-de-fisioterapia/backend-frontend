// ======================================================================================
// ARQUIVO: DeleteUserCommandHandler.cs
// CAMADA: Application / Features / Users / Commands / DeleteUser
// OBJETIVO: Handler responsável por realizar a exclusão lógica (soft delete) de um usuário.
// ======================================================================================

using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Application.Resources;
using MediatR;

namespace Chronosystem.Application.Features.Users.Commands.DeleteUser;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
{
    private readonly IUserRepository _userRepository;

    public DeleteUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        // 1️⃣ Busca o usuário pelo ID no schema atual
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
        if (user is null)
            throw new InvalidOperationException(Messages.User_NotFound);

        // 2️⃣ Marca como excluído logicamente
        user.SoftDelete();
        user.UpdatedAt = DateTime.UtcNow;

        // 3️⃣ Persiste as alterações
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(cancellationToken);

        // 4️⃣ Conclusão
        return Unit.Value;
    }
}
