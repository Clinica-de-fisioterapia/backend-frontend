// ======================================================================================
// ARQUIVO: DeleteUserCommandHandler.cs
// CAMADA: Application / Features / Users / Commands / DeleteUserCommand
// OBJETIVO: Manipula o comando responsável por excluir logicamente um usuário.
// ======================================================================================

using Chronosystem.Application.Common.Interfaces.Persistence;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Unit = MediatR.Unit; // ✅ Resolve ambiguidade

namespace Chronosystem.Application.Features.Users.Commands.DeleteUserCommand;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteUserCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);

        if (user is null)
            throw new InvalidOperationException($"Usuário com ID {request.Id} não encontrado.");

        user.SoftDelete();

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
