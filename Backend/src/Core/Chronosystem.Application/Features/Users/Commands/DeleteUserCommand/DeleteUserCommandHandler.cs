using Chronosystem.Application.Common.Interfaces.Persistence;
using MediatR;

namespace Chronosystem.Application.Features.Users.Commands.DeleteUser;

public sealed class DeleteUserCommandHandler(IUserRepository userRepository) : IRequestHandler<DeleteUserCommand>
{
    public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Unit.Value;
        }

        user.SoftDelete(request.DeletedByUserId);
        userRepository.Update(user);
        await userRepository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
