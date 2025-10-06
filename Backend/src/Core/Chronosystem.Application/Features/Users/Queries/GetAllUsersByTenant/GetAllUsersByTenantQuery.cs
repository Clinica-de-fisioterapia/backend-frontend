using Chronosystem.Application.Features.Users.DTOs;
using MediatR;
namespace Chronosystem.Application.Features.Users.Queries.GetAllUsersByTenant;

public record GetAllUsersByTenantQuery(Guid TenantId) : IRequest<IEnumerable<UserDto>>;