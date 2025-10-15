// ======================================================================================
// ARQUIVO: UpdateUserCommandHandler.cs
// CAMADA: Application / Features / Users / Commands / UpdateUser
// OBJETIVO: Handler responsável por atualizar informações de um usuário existente.
//            Suporta multi-tenant por schema e validações via domínio.
// ======================================================================================

using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Application.Resources;
using MediatR;

namespace Chronosystem.Application.Features.Users.Commands.UpdateUser;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand>
{
    private readonly IUserRepository _userRepository;

    public UpdateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Unit> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        // 1️⃣ Busca o usuário pelo ID no schema atual
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
        if (user is null)
            throw new InvalidOperationException(Messages.User_NotFound);

        // 2️⃣ Atualiza propriedades básicas
        user.UpdateName(request.FullName);
        user.UpdateRole(request.Role);
        user.IsActive = request.IsActive;

        // 3️⃣ Atualizações opcionais
        if (!string.IsNullOrWhiteSpace(request.Email) && !request.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase))
            user.UpdateEmail(request.Email);

        if (!string.IsNullOrWhiteSpace(request.PasswordHash))
            user.UpdatePassword(request.PasswordHash);

        user.UpdatedAt = DateTime.UtcNow;

        // 4️⃣ Persiste alterações
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(cancellationToken);

        // 5️⃣ Conclusão
        return Unit.Value;
    }
}
