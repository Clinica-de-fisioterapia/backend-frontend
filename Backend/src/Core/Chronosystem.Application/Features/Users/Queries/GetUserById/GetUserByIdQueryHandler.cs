// ======================================================================================
// ARQUIVO: GetUserByIdQueryHandler.cs
// CAMADA: Application / Features / Users / Queries / GetUserById
// OBJETIVO: Retorna um usuário específico pelo seu ID.
// ======================================================================================

using Chronosystem.Application.Features.Users.DTOs;
using Chronosystem.Application.Common.Interfaces.Persistence;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Chronosystem.Application.Features.Users.Queries.GetUserById;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    private readonly IUserRepository _userRepository;

    public GetUserByIdQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
        if (user is null)
            return null;

        return new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}
