using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Domain.Entities;
using MediatR;

// Evita conflito entre Domain.Entities.Unit e MediatR.Unit
using Unit = MediatR.Unit;

namespace Chronosystem.Application.Features.Users.Commands.UpdateUser;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Unit>
{
    private readonly IUserRepository _userRepository;

    public UpdateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Unit> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        // Busca o usuário pelo Tenant
        var user = await _userRepository.GetByIdAsync(request.UserId, request.TenantId);
        if (user is null)
            throw new KeyNotFoundException("Usuário não encontrado.");

        // Valida o role
        if (!Enum.TryParse<UserRole>(request.Role, true, out var userRole))
            throw new ArgumentException("Role inválido.");

        user.UpdateName(request.FullName);
        user.UpdateRole(userRole);
        user.IsActive = request.IsActive;
        user.UpdatedBy = request.UpdatedByUserId;
        user.UpdatedAt = DateTime.UtcNow;

        // Persiste
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        return Unit.Value;
    }
}
