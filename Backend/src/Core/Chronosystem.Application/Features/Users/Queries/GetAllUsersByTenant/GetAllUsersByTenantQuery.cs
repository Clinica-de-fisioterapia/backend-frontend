using Chronosystem.Application.Features.Users.DTOs;
using MediatR;

namespace Chronosystem.Application.Features.Users.Queries.GetAllUsersByTenant;

public sealed record GetAllUsersByTenantQuery() : IRequest<IEnumerable<UserDto>>;
