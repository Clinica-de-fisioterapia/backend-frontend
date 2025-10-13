// Features/Users/Commands/CreateUser/CreateUserCommandHandler.cs
using System.Security.Cryptography;
using System.Text;
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
        // 1) Email must be unique per tenant
        var exists = await userRepository.UserExistsByEmailAsync(request.Email, request.TenantId);
        if (exists)
            throw new InvalidOperationException("Já existe um usuário com este e-mail para este tenant.");

        // 2) Hash password (no external packages to avoid build errors)
        var passwordHash = HashPasswordSha256(request.Password);

        // 3) Parse role
        if (!Enum.TryParse<UserRole>(request.Role, true, out var userRole))
            throw new ArgumentException("Role inválido. Use: Admin, Professional ou Receptionist.");

        // 4) Create domain entity using factory (respects private setters)
        var user = User.Create(request.FullName, request.Email, passwordHash, userRole);

        // 5) Set multi-tenant and defaults
        user.TenantId = request.TenantId;
        user.IsActive = true;
        user.RowVersion = 1;
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        // 6) Persist
        await userRepository.AddAsync(user);
        await userRepository.SaveChangesAsync();

        // 7) Return DTO
        return new UserDto(user.Id, user.FullName, user.Email, user.Role.ToString(), user.IsActive);
    }

    // Simple SHA256 hashing to avoid external dependencies during build
    private static string HashPasswordSha256(string input)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        var sb = new StringBuilder(bytes.Length * 2);
        foreach (var b in bytes) sb.Append(b.ToString("x2"));
        return sb.ToString();
    }
}
