using MediatR;

namespace Chronosystem.Application.Features.Users.Commands.UpdateUser;

public record UpdateUserCommand(
    Guid UserId,
    string FullName,
    string Role,
    bool IsActive,
    Guid TenantId,          
    Guid UpdatedByUserId) : IRequest<Unit>;
