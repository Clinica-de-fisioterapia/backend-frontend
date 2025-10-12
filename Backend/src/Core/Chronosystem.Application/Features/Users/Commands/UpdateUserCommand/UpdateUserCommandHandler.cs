using System;
using System.Threading;
using System.Threading.Tasks;
using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Domain.Entities;
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
        var user = await _userRepository.GetByIdAsync(request.UserId, request.TenantId);
        if (user is null)
        {
            throw new Exception("Usuário não encontrado.");
        }

        if (!Enum.TryParse<UserRole>(request.Role, true, out var userRole))
        {
            throw new Exception("Role inválido.");
        }

        user.FullName = request.FullName;
        user.Role = userRole;
        user.IsActive = request.IsActive;
        user.UpdatedBy = request.UpdatedByUserId;

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        return Unit.Value;
    }
}
