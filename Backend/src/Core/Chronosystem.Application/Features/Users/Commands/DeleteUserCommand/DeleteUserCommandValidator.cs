// ======================================================================================
// ARQUIVO: DeleteUserCommandValidator.cs
// CAMADA: Application / Features / Users / Commands / DeleteUserCommand
// OBJETIVO: Define as validações do comando DeleteUserCommand.
// ======================================================================================

using Chronosystem.Application.Resources;
using FluentValidation;
using System;

namespace Chronosystem.Application.Features.Users.Commands.DeleteUserCommand;

/// <summary>
/// Validador responsável por garantir a integridade da requisição de exclusão.
/// </summary>
public class DeleteUserCommandValidator : AbstractValidator<global::Chronosystem.Application.Features.Users.Commands.DeleteUserCommand.DeleteUserCommand>
{
    public DeleteUserCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(Messages.Validation_UserId_Required);
    }
}
