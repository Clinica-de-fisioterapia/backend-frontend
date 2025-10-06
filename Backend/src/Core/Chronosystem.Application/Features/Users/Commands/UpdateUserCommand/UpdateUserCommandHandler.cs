using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Domain.Entities;
using MediatR;
namespace Chronosystem.Application.Features.Users.Commands.UpdateUser;

public class UpdateUserCommandHandler(IUserRepository userRepository) : IRequestHandler<UpdateUserCommand>
{
    public async Task Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, request.TenantId);
        if (user is null) { throw new Exception("Usuário não encontrado."); }

        if (!Enum.TryParse<UserRole>(request.Role, true, out var userRole))
        {
            throw new Exception("Role inválido.");
        }

        user.FullName = request.FullName;
        user.Role = userRole;
        user.IsActive = request.IsActive;
        user.UpdatedBy = request.UpdatedByUserId;

        userRepository.Update(user);
        await userRepository.SaveChangesAsync();
    }
}