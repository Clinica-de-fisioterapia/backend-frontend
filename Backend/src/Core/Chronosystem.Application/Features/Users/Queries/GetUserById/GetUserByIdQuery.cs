// Features/Users/Queries/GetUserById/GetUserByIdQuery.cs
using Chronosystem.Application.Features.Users.DTOs;
using MediatR;

namespace Chronosystem.Application.Features.Users.Queries.GetUserById;

public record GetUserByIdQuery(Guid UserId, Guid TenantId) : IRequest<UserDto?>;
