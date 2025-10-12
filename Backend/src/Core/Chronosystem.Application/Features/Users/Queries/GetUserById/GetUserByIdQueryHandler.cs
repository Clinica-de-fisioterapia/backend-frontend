using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Application.Features.Users.DTOs;
using MediatR;

namespace Chronosystem.Application.Features.Users.Queries.GetUserById;

public sealed class GetUserByIdQueryHandler(IUserRepository userRepository)
    : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return null;
        }

        return new UserDto(user.Id, user.FullName, user.Email, user.Role.ToString(), user.IsActive);
    }
}
