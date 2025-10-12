using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Domain.Entities;
using MediatR;

namespace Chronosystem.Application.Features.Users.Commands.UpdateUser;

public sealed class UpdateUserCommandHandler(IUserRepository userRepository) : IRequestHandler<UpdateUserCommand>
{
    public async Task<Unit> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            throw new KeyNotFoundException("Usuário não encontrado.");
        }

        if (!Enum.TryParse<UserRole>(request.Role, true, out var userRole))
        {
            throw new ArgumentException("Role inválida para o usuário.", nameof(request.Role));
        }

        user.UpdateName(request.FullName, request.UpdatedByUserId);
        user.UpdateRole(userRole, request.UpdatedByUserId);
        user.SetActiveStatus(request.IsActive, request.UpdatedByUserId);

        userRepository.Update(user);
        await userRepository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
