// ======================================================================================
// ARQUIVO: CreateUserCommandHandler.cs
// CAMADA: Application / Features / Users / Commands / CreateUser
// OBJETIVO: Manipula o comando responsável por criar um novo usuário.
// ======================================================================================

using Chronosystem.Application.Common.Interfaces.Persistence;
using Chronosystem.Application.Resources;
using Chronosystem.Domain.Entities;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Chronosystem.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateUserCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        if (await _userRepository.UserExistsByEmailAsync(request.Email, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure(nameof(request.Email), Messages.User_Email_AlreadyExists)
            });
        }

        // Gera hash seguro da senha
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = User.Create(request.FullName, request.Email, passwordHash, request.Role);

        user.CreatedBy = request.ActorUserId;
        user.UpdatedBy = request.ActorUserId;

        // AddAsync() recebe apenas a entidade
        await _userRepository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
