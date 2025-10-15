// ======================================================================================
// ARQUIVO: CreateUserCommandHandler.cs
// CAMADA: Application / Features / Users / Commands / CreateUser
// OBJETIVO: Handler responsável pela criação de um novo usuário no sistema.
//            Utiliza o método de fábrica da entidade User (User.Create) e CQRS (MediatR).
// ======================================================================================

using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Application.Resources;
using Chronosystem.Domain.Entities;
using MediatR;

namespace Chronosystem.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
{
    private readonly IUserRepository _userRepository;

    public CreateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // 1️⃣ Verifica duplicidade de e-mail
        var emailExists = await _userRepository.UserExistsByEmailAsync(request.Email, cancellationToken);
        if (emailExists)
            throw new InvalidOperationException(Messages.User_Email_AlreadyExists);

        // 2️⃣ Cria a entidade via método de fábrica (garante validação de domínio)
        var user = User.Create(
            request.FullName.Trim(),
            request.Email.Trim().ToLower(),
            request.PasswordHash,
            request.Role
        );

        // 3️⃣ Marca como ativo e inicializa metadados
        user.IsActive = true;
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        // 4️⃣ Persiste no banco (tenant já resolvido via schema)
        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync(cancellationToken);

        // 5️⃣ Retorna o ID gerado
        return user.Id;
    }
}
