// Features/Users/Commands/CreateUser/CreateUserCommand.cs
using Chronosystem.Application.Features.Users.DTOs;
using MediatR;

namespace Chronosystem.Application.Features.Users.Commands.CreateUser;

public record CreateUserCommand(
    string FullName,
    string Email,
    string Password,
    string Role,
    Guid TenantId
) : IRequest<UserDto>;
