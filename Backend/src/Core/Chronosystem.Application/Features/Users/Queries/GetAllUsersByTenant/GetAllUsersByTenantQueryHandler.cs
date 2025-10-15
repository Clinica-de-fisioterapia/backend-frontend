// ======================================================================================
// ARQUIVO: GetAllUsersQueryHandler.cs
// CAMADA: Application / Features / Users / Queries / GetAllUsers
// OBJETIVO: Handler responsável por retornar todos os usuários ativos do tenant atual.
// ======================================================================================

using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Application.Resources;
using Chronosystem.Domain.Entities;
using MediatR;

namespace Chronosystem.Application.Features.Users.Queries.GetAllUsers;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, IEnumerable<User>>
{
    private readonly IUserRepository _userRepository;

    public GetAllUsersQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<User>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        // 1️⃣ Busca todos os usuários ativos no schema atual
        var users = await _userRepository.GetAllAsync(cancellationToken);

        // 2️⃣ Caso não existam usuários, lança exceção controlada
        if (!users.Any())
            throw new InvalidOperationException(Messages.User_NotFound);

        // 3️⃣ Retorna a lista encontrada
        return users;
    }
}
