using Chronosystem.Application.Common.Interfaces.Persistence;
using MediatR;
namespace Chronosystem.Application.Features.Users.Commands.DeleteUser;

public class DeleteUserCommandHandler(IUserRepository userRepository) : IRequestHandler<DeleteUserCommand>
{
    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, request.TenantId);
        if (user is not null)
        {
            userRepository.Remove(user); // Soft delete
            await userRepository.SaveChangesAsync();
        }
    }
}