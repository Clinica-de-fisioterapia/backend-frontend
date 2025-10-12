// Chronosystem.Application/Features/Users/Commands/CreateUserCommand.cs/CreateUser.cs
using Chronosystem.Application.Features.Users.DTOs;
using MediatR;

namespace Chronosystem.Application.Features.Users.Commands.CreateUser;

public sealed record CreateUserCommand(
    string FullName,
    string Email,
    string Password,
    string Role,
    Guid CreatedByUserId) : IRequest<UserDto>;
