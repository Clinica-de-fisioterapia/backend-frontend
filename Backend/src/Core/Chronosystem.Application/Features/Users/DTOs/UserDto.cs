namespace Chronosystem.Application.Features.Users.DTOs;

public record UserDto(
    Guid Id,
    string FullName,
    string Email,
    string Role,
    bool IsActive
);