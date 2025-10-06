// Features/Users/Commands/CreateUser/CreateUserCommandHandler.cs
using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Application.Features.Users.DTOs;
using Chronosystem.Domain.Entities;
using MediatR;

namespace Chronosystem.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommandHandler(IUserRepository userRepository) 
    : IRequestHandler<CreateUserCommand, UserDto>
{
    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // 1. Validar se o e-mail já existe para este tenant
        if (await userRepository.UserExistsByEmailAsync(request.Email, request.TenantId))
        {
            throw new Exception("Um usuário com este e-mail já existe."); // Usar exceções customizadas é melhor
        }
        
        // 2. Hashear a senha
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        
        // 3. Mapear o Role (string) para o Enum
        if (!Enum.TryParse<UserRole>(request.Role, true, out var userRole))
        {
            throw new Exception("Role inválido.");
        }

        // 4. Criar a entidade
        var user = new User
        {
            Id = Guid.NewGuid(),
            TenantId = request.TenantId,
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = passwordHash,
            Role = userRole,
            IsActive = true,
            RowVersion = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // 5. Persistir no banco
        await userRepository.AddAsync(user);
        await userRepository.SaveChangesAsync();

        // 6. Retornar o DTO
        return new UserDto(user.Id, user.FullName, user.Email, user.Role.ToString(), user.IsActive);
    }
}