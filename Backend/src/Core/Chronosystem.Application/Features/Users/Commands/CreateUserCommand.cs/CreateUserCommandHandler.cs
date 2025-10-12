// Chronosystem.Application/Features/Users/Commands/CreateUserCommand.cs/CreateUserCommandHandler.cs
using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Application.Features.Users.DTOs;
using Chronosystem.Domain.Entities;
using MediatR;

namespace Chronosystem.Application.Features.Users.Commands.CreateUser;

public sealed class CreateUserCommandHandler(IUserRepository userRepository)
    : IRequestHandler<CreateUserCommand, UserDto>
{
    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        if (await userRepository.UserExistsByEmailAsync(request.Email, cancellationToken))
        {
            throw new InvalidOperationException("Um usuário com este e-mail já existe.");
        }

        if (!Enum.TryParse<UserRole>(request.Role, true, out var userRole))
        {
            throw new ArgumentException("Role inválida para o usuário.", nameof(request.Role));
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = User.Create(request.FullName, request.Email, passwordHash, userRole, request.CreatedByUserId);

        await userRepository.AddAsync(user, cancellationToken);
        await userRepository.SaveChangesAsync(cancellationToken);

        return new UserDto(user.Id, user.FullName, user.Email, user.Role.ToString(), user.IsActive);
    }
}
