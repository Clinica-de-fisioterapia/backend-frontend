namespace Chronosystem.Application.Features.Users.DTOs;

public record UpdateUserDto(
    string FullName,
    string Role,
    bool IsActive
);