using Chronosystem.Application.Features.Users.DTOs;
using MediatR;
namespace Chronosystem.Application.Features.Users.Queries.GetUserById;

public record GetUserByIdQuery(Guid UserId) : IRequest<UserDto?>;