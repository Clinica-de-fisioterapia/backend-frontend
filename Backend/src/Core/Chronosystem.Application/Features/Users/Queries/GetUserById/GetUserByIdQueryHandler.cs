// ======================================================================================
// ARQUIVO: GetUserByIdQueryHandler.cs
// CAMADA: Application / Features / Users / Queries / GetUserById
// OBJETIVO: Handler responsável por obter um usuário específico, considerando
//            o tenant atual (multi-tenant via schema) e apenas usuários ativos.
// ======================================================================================

using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Application.Resources;
using Chronosystem.Domain.Entities;
using MediatR;

namespace Chronosystem.Application.Features.Users.Queries.GetUserById;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, User?>
{
    private readonly IUserRepository _userRepository;

    public GetUserByIdQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<User?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        // 1️⃣ Busca o usuário ativo no schema atual
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);

        // 2️⃣ Caso não encontrado, lança exceção controlada
        if (user is null)
            throw new InvalidOperationException(Messages.User_NotFound);

        // 3️⃣ Retorna a entidade encontrada
        return user;
    }
}
