using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Application.Features.Users.DTOs;
using MediatR;
namespace Chronosystem.Application.Features.Users.Queries.GetAllUsersByTenant;

public class GetAllUsersByTenantQueryHandler(IUserRepository userRepository) : IRequestHandler<GetAllUsersByTenantQuery, IEnumerable<UserDto>>
{
    public async Task<IEnumerable<UserDto>> Handle(GetAllUsersByTenantQuery request, CancellationToken cancellationToken)
    {
        var users = await userRepository.GetAllByTenantAsync(request.TenantId);

        return users.Select(user => 
            new UserDto(user.Id, user.FullName, user.Email, user.Role.ToString(), user.IsActive));
    }
}