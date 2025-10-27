// ======================================================================================
// ARQUIVO: UpdateUserCommandHandler.cs
// CAMADA: Application / Features / Users / Commands / UpdateUserCommand
// OBJETIVO: Manipula o comando responsável por atualizar um usuário existente.
// ======================================================================================

using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Domain.Entities;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Unit = MediatR.Unit; // ✅ Resolvendo ambiguidade

namespace Chronosystem.Application.Features.Users.Commands.UpdateUserCommand;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        if (request.ActorUserId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Actor user id is required for auditing.");
        }

        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Usuário com ID {request.Id} não encontrado.");

        user.UpdateName(request.FullName);
        user.UpdateEmail(request.Email);

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            user.UpdatePassword(passwordHash);
        }

        user.UpdateRole(request.Role);
        user.UpdateIsActive(request.IsActive);

        user.UpdatedBy = request.ActorUserId;

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
