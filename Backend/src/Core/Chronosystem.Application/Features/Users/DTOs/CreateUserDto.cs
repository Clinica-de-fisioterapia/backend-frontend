// Chronosystem.Application/Features/Users/DTOs/CreateUserDto.cs
namespace Chronosystem.Application.Features.Users.DTOs;

public record CreateUserDto(
    string FullName,
    string Email,
    string Password, // Senha em texto plano, que será hasheada
    string Role
);